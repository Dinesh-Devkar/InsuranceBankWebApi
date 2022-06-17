using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Agent;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Customer;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors()]
    public class AgentController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAllRepository<InsuranceAccount> _insuranceAccountManager;

        public AgentController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,BankInsuranceDbContext dbContext)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(dbContext);
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] AgentAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var agentExist = await this._userManager.FindByEmailAsync(model.Email);
            if (agentExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Already Exists" });
            }
            //var loginIdExists = this._employeeManager.GetAll().Find(x => x.LoginId == model.LoginId);
            var loginIdExists = this._userManager.Users.ToList().Find(x=>x.LoginId==model.LoginId);
            Debug.WriteLine("The Login Id Exists : " + loginIdExists);
            if (loginIdExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "LoginId Already Exists Use Another LoginId" });
            }
            var agentCodeExists= this._userManager.Users.ToList().Find(x => x.AgentCode == int.Parse(model.AgentCode));
            if (agentCodeExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Code Already Exists Use Another Agent Code" });
            }
            if (model.Password != model.ConfirmPassword)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password And Confirm Password Does Not Match" });
            }

            ApplicationUser agent = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Name,
                UserRoll = UserRoles.Agent,
                SecurityStamp = Guid.NewGuid().ToString(),
                LoginId = model.LoginId,
                UserStatus = model.Status,
                Address= model.Address,
                Qualification=model.Qualification,
                AgentCode=int.Parse(model.AgentCode)
                
            };
            var result = await this._userManager.CreateAsync(agent, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Created" });
            }
            if (!await this._roleManager.RoleExistsAsync(UserRoles.Agent))
            {
                await this._roleManager.CreateAsync(new IdentityRole(UserRoles.Agent));
            }
            if (await this._roleManager.RoleExistsAsync(UserRoles.Agent))
            {
                await this._userManager.AddToRoleAsync(agent, UserRoles.Agent);
            }

            return this.Ok(new Response { Message = "Agent Created Successfully", Status = "Success" });
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {

            var user = await this._userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
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
        [HttpGet]
        [Route("GetAllAgents")]
        public async Task<List<AgentGetDto>> GetAllAgents()
        {
            List<AgentGetDto> agentsList = new List<AgentGetDto>();
            var agents= await this._userManager.Users.Where(x=>x.UserRoll==UserRoles.Agent).ToListAsync();
            foreach (var agent in agents)
            {
                agentsList.Add(new AgentGetDto()
                {
                    UserRoll = agent.UserRoll,
                    Address = agent.Address,
                    AgentCode = agent.AgentCode.GetValueOrDefault(),
                    Email = agent.Email,
                    LoginId = agent.LoginId,
                    Name = agent.UserName,
                    Qualification = agent.Qualification,
                    Status = agent.UserStatus

                });
            }
            return agentsList;

        }

        [HttpGet]
        [Route("{agentCode}/GetInsuranceAccountsByAgentCode")]
        public async Task<List<InsuranceAccountGetDto>> GetInsuranceAccountsByAgentCode(string agentCode)
        {
            List<InsuranceAccountGetDto> insuranceAccountsList = new List<InsuranceAccountGetDto>();
            var insuranceAccounts = this._insuranceAccountManager.GetAll().Where(x => x.AgentCode == agentCode).ToList();
            foreach (var insuranceAccount in insuranceAccounts)
            {
                insuranceAccountsList.Add(new InsuranceAccountGetDto()
                {
                    AgentCode = insuranceAccount.AgentCode,
                    CustomerId = insuranceAccount.CustomerId,
                    CustomerName = insuranceAccount.CustomerName,
                    DateCreated = insuranceAccount.DateCreated,
                    InstallmentAmount = insuranceAccount.InstallmentAmount,
                    InsuranceScheme = insuranceAccount.InsuranceScheme,
                    InsuranceType = insuranceAccount.InsuranceType,
                    InterestAmount = insuranceAccount.InterestAmount,
                    InvestmentAmount = insuranceAccount.InvestmentAmount,
                    MaturityDate = insuranceAccount.MaturityDate,
                    NumberOfYears = insuranceAccount.NumberOfYears,
                    PremiumType = insuranceAccount.PremiumType,
                    ProfitRatio = insuranceAccount.ProfitRatio,
                    TotalAmount = insuranceAccount.TotalAmount,
                });
            }
            return insuranceAccountsList;
        }
    }
}
