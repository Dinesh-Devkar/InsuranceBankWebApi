﻿using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Employee;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IAllRepository<ApplicationUser> _employeeManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        public EmployeeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, BankInsuranceDbContext bankInsuranceDb)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
            this._employeeManager = new AllRepository<ApplicationUser>(bankInsuranceDb);
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] EmployeeAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var employeeExist = await this._userManager.FindByEmailAsync(model.Email);
            if (employeeExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Employee Already Exists" });
            }
            //var loginIdExists = this._employeeManager.GetAll().Find(x => x.LoginId == model.LoginId);
            var loginIdExists = this._userManager.Users.ToList().Find(x => x.LoginId == model.LoginId);

            if (loginIdExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "LoginId Already Exists Use Another LoginId" });
            }

            ApplicationUser employee = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Name,
                UserRoll = UserRoles.Employee,
                SecurityStamp = Guid.NewGuid().ToString(),
                LoginId = model.LoginId,
                UserStatus = model.UserStatus
            };
            var result = await this._userManager.CreateAsync(employee, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Employee Not Created" });
            }
            if (!await this._roleManager.RoleExistsAsync(UserRoles.Employee))
            {
                await this._roleManager.CreateAsync(new IdentityRole(UserRoles.Employee));
            }
            if (await this._roleManager.RoleExistsAsync(UserRoles.Employee))
            {
                await this._userManager.AddToRoleAsync(employee, UserRoles.Employee);
            }

            return this.Ok(new Response { Message = "Employee Created Successfully", Status = "Success" });
        }
        [HttpGet]
        [Route("GetAllEmployees")]
        public async Task<List<EmployeeGetDto>> GetAllEmployees()
        {
            List<EmployeeGetDto> employeesList = new List<EmployeeGetDto>();
            //var employees = this._employeeManager.GetAll().Where(x=>x.UserRoll==UserRoles.Employee);
            var employees = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Employee).ToListAsync();
            foreach (var employee in employees)
            {
                employeesList.Add(new EmployeeGetDto()
                {
                    Email = employee.Email,
                    LoginId = employee.LoginId,
                    Name = employee.UserName,
                    UserRoll = employee.UserRoll,
                    UserStatus = employee.UserStatus,
                });

            }
            return employeesList;
        }

        [HttpGet]
        [Route("{employeeId}/GetEmployeeById")]
        public async Task<IActionResult> GetEmployeeById([FromBody] string employeeId)
        {
            var employee=await this._userManager.Users.Where(x=>x.Id == employeeId).FirstOrDefaultAsync();
            if (employee == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Employee Not Found" });
            }
            return this.Ok(new EmployeeGetDto()
            {
                Email = employee.Email,
                LoginId = employee.LoginId,
                Name = employee.UserName,
                UserRoll = employee.UserRoll,
                UserStatus = employee.UserStatus,
            });
        }
        [HttpPut]
        [Route("{employeeId}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(string employeeId, ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var userExist = await this._userManager.FindByIdAsync(employeeId);
                if (userExist == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Employee Not Found" });
                }
                if (model.NewPassword.Equals(model.ConfirmNewPassword))
                {
                    var result = await this._userManager.ChangePasswordAsync(userExist, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.Ok(new Response { Message = "Password Updated Successfully", Status = "Success" });
                    }
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Password" });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password and Confirm Password does not match" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });

        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            
            var user = await this._userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Employee Not Found" });
            }
            if (user != null && await this._userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await this._userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var authSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:ValidIssuer"],
                    audience: _configuration["Jwt:ValidAudience"],
                    claims: authClaims,    //null original value
                    expires: DateTime.Now.AddMinutes(120),

             //notBefore:
             signingCredentials: new SigningCredentials(authSignInKey, SecurityAlgorithms.HmacSha256));

                return this.Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expire = token.ValidTo,
                    UserName = user.UserName,
                    userId = user.Id

                });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Password" }); ;

        }
    }
}
