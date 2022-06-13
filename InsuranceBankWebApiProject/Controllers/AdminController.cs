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
using System.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using EnsuranceProjectEntityLib.Model.Common;
using System.Diagnostics;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        //private readonly AdminRepository _adminRepository=new AdminRepository();
        //private readonly AllRepository<Admin> adminRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager, IConfiguration configuration) 
        {
            // this.adminRepo = new AllRepository<Admin>();
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]AdminAddDto model)
        {
            var userExist = await _userManager.FindByEmailAsync(model.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Already Exists" });
            }
            //var loginIdExists=await 
            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                LoginId=model.LoginId,
                UserStatus="Active"
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Created" });
            }
            if(! await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return this.Ok(new Response { Message = "User Created Successfully", Status = "Success" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Debug.WriteLine("The Email is : "+model.Email);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Found" });
            }
            if(user!=null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles=await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                foreach(var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var authSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var token = new JwtSecurityToken(
                    issuer:
             _configuration["Jwt:ValidIssuer"],
                    audience:
              _configuration["Jwt:ValidAudience"],
             claims:authClaims,    //null original value
             expires: DateTime.Now.AddMinutes(120),

             //notBefore:
             signingCredentials: new SigningCredentials(authSignInKey, SecurityAlgorithms.HmacSha256));

                return this.Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expire = token.ValidTo,
                    UserName = user.UserName,
                    userId=user.Id

                });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Password" }); ;
            
        }
        [HttpPost]
        [Route("{adminId}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(string adminId, ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.FindByIdAsync(adminId);
                if (userExist == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Found" });
                }
                if (model.NewPassword.Equals(model.ConfirmNewPassword))
                {
                    var result = await _userManager.ChangePasswordAsync(userExist, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.Ok(new Response { Message = "Password Updated Successfully", Status = "Success" });
                    }
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password and Confirm Password does not match" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });

        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [Route("GetAllAdmins")]
        
        public async Task<List<ApplicationUser>> GetAllAdmins()
        {
            return _userManager.Users.ToList();
        }
        //[HttpPost]
        //[Route("AddAdmin")]
        //public async Task<IActionResult> AddAdmin(Admin admin)
        //{
        //    adminRepo.Add(admin);

        //    return this.Ok("Admin Added Successfully");
        //}

        //[HttpGet]
        //[Route("GetAllAdmins")]
        //public async Task<IEnumerable<Admin>> GetAllAdmins()
        //{
        //    return await adminRepo.GetAll();
        //}

    }
}
