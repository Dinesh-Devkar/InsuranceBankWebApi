﻿using EnsuranceProjectEntityLib.Model.Common;
using InsuranceBankWebApiProject.DtoClasses.Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/InsuranceBank/[controller]")]
    [ApiController]
    [EnableCors()]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
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
                    userId = user.Id,
                    userRoll=user.UserRoll

                });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Password" }); ;

        }
    }
}