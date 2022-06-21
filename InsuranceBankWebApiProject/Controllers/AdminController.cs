using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using EnsuranceProjectEntityLib.Model.AdminModel;
using Microsoft.AspNetCore.Identity;
using InsuranceBankWebApiProject.DtoClasses.Admin;
using InsuranceBankWebApiProject.DtoClasses.Common;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using EnsuranceProjectEntityLib.Model.Common;
using System.Diagnostics;
using EnsuranceProjectEntityLib.Model.Insurance;
using InsuranceBankWebApiProject.DtoClasses.Insurance;
using System.Drawing;
using Microsoft.AspNetCore.Cors;
using InsuranceBankWebApiProject.DtoClasses.Employee;
using Microsoft.EntityFrameworkCore;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableCors()]
    [Authorize(Roles =UserRoles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, BankInsuranceDbContext bankInsuranceDb)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] AdminAddDto model)
        {
            // For Admin Registration
            var userExist = await this._userManager.FindByEmailAsync(model.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Already Exists" });
            }
            var loginIdExists = this._userManager.Users.ToList().Find(x => x.LoginId == model.LoginId);

            if (loginIdExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "LoginId Already Exists Use Another LoginId" });
            }
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "PAssword and Confirm Password not Match" });
            }
            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Name,
                SecurityStamp = Guid.NewGuid().ToString(),
                LoginId = model.LoginId,
                UserStatus = model.UserStatus,
                UserRoll = model.UserRoll,

            };
            var result = await this._userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Created" });
            }
            if (!await this._roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await this._roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }
            if (await this._roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await this._userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return this.Ok(new Response { Message = "User Created Successfully", Status = "Success" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
           //For Admin Login And Return Token
            var user = await this._userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Found" });
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
        [HttpPut]
        [Route("{adminId}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(string adminId, ChangePasswordModel model)
        {
            //Update Password
            if (ModelState.IsValid)
            {
                var userExist = await this._userManager.FindByIdAsync(adminId);
                if (userExist == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Found" });
                }
                var result1 = _userManager.PasswordHasher.VerifyHashedPassword(userExist, userExist.PasswordHash, model.OldPassword);
                if (result1 != PasswordVerificationResult.Success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Password Given" });
                }
                if (model.NewPassword.Equals(model.ConfirmNewPassword))
                {
                    var result = await this._userManager.ChangePasswordAsync(userExist, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.Ok(new Response { Message = "Password Updated Successfully", Status = "Success" });
                    }
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password and Confirm Password does not match" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });

        }

        [HttpGet]
        [Route("GetAllAdmins")]

        public async Task<List<ApplicationUser>> GetAllAdmins()
        {
            // Return The List Of All Admins Plus Users For Testing
            return this._userManager.Users.ToList();

        }

        [HttpGet]
        [Route("{adminId}/GetAdminDetails")]
        public async Task<IActionResult> GetAdminDetails(string adminId)
        {
            //Method Will Return The Admin Details To See In Profile
            var admin = await this._userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Admin Not Found" });
            }
            return this.Ok(new AdminGetDto()
            {
                Email = admin.Email,
                LoginId = admin.LoginId,
                Name = admin.UserName,
                UserRoll = admin.UserRoll,
                UserStatus = admin.UserStatus,
            });
        }
        [HttpGet]
        [Route("GetAllEmployees")]
        public async Task<List<EmployeeGetDto>> GetAllEmployees()
        {
            List<EmployeeGetDto> employeesList = new List<EmployeeGetDto>();
            //var employees = this._employeeManager.GetAll().Where(x=>x.UserRoll==UserRoles.Employee);
            var employees = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Employee || x.UserRoll==UserRoles.Admin).ToListAsync();
            foreach (var employee in employees)
            {
                employeesList.Add(new EmployeeGetDto()
                {
                    Id = employee.Id,
                    Email = employee.Email,
                    LoginId = employee.LoginId,
                    Name = employee.UserName,
                    UserRoll = employee.UserRoll,
                    UserStatus = employee.UserStatus,
                });

            }
            return employeesList;
        }

    }
}