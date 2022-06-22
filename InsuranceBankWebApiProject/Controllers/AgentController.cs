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
        private readonly IAllRepository<CommissionRecord> _commissionRecordManager;

        public AgentController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,BankInsuranceDbContext dbContext)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(dbContext);
            this._commissionRecordManager = new AllRepository<CommissionRecord>(dbContext);
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] AgentAddDto model)
        {
            // Registration For Agent
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var agentExist = await this._userManager.FindByEmailAsync(model.Email);
            if (agentExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Already Exists" });
            }
            
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
            //For Agent Login
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
            //Will Return The List Of All Admins
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
        [Route("{agentId}/GetAgentById")]
        public async Task<IActionResult> GetAgentById(string agentId)
        {
            //Will Return The Agent Details For Edit  And See In His Profile By Agent Id
            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }
            return this.Ok(new AgentGetDto()
            {
                Name = agent.UserName,
                Address = agent.Address,
                AgentCode = agent.AgentCode.GetValueOrDefault(),
                Email = agent.Email,
                LoginId = agent.LoginId,
                Qualification = agent.Qualification,
                Status = agent.UserStatus,
                UserRoll = agent.UserRoll

            });
        }
        [HttpGet]
        [Route("{agentCode}/GetAgentByCode")]
        public async Task<IActionResult> GetAgentByCode(int agentCode)
        {
            //Will Return The Agent Details For Edit  And See In His Profile By Agent Unique Code
            var agent = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Agent && x.AgentCode == agentCode).FirstOrDefaultAsync();
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }
            return this.Ok(new AgentGetDto()
            {
                Name = agent.UserName,
                Address = agent.Address,
                AgentCode = agent.AgentCode.GetValueOrDefault(),
                Email = agent.Email,
                LoginId = agent.LoginId,
                Qualification = agent.Qualification,
                Status = agent.UserStatus,
                UserRoll = agent.UserRoll,
                

            });
        }
        [HttpGet]
        [Route("{agentId}/GetInsuranceAccountsByAgentId")]
        public async Task<IActionResult> GetInsuranceAccountsByAgentId(string agentId)
        {
            //Method Will Return The List Of Insurance Accounts Of Particular Agent Based On His ID
            List<InsuranceAccountGetDto> insuranceAccountsList = new List<InsuranceAccountGetDto>();
            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent==null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }
            if (agent.UserRoll != UserRoles.Agent)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }
            var insuranceAccounts = this._insuranceAccountManager.GetAll().Where(x => x.AgentCode == agent.AgentCode.ToString()).ToList();
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
                    AccountNumber=insuranceAccount.AccountNumber
                    
                });
            }
            return this.Ok(insuranceAccountsList);
        }
        [HttpGet]
        [Route("{agentId}/GetAllCustomersByAgentId")]
        public async Task<IActionResult> GetAllCustomersByAgentId(string agentId)
        {
            //Method Will Return The List Of Customers  Of Particular Agent Based On His ID

            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent.UserRoll != UserRoles.Agent)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }
            List<CustomerGetDto> customersList = new List<CustomerGetDto>();
            var customers = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Customer && x.AgentCode == agent.AgentCode).ToListAsync();
            foreach (var customer in customers)
            {
                customersList.Add(new CustomerGetDto()
                {
                    Name = customer.UserName,
                    Address = customer.Address,
                    DateOfBirth = customer.DateOfBirth,
                    Email = customer.Email,
                    LoginId = customer.LoginId,
                    MobileNumber = customer.PhoneNumber,
                    NomineeName = customer.NomineeName,
                    NomineeRelation = customer.NomineeRelation,
                    Status = customer.UserStatus

                });
            }
            return this.Ok(customersList);

        }
        [HttpGet]
        [Route("{agentId}/GetCommissionRecordsByAgentId")]
        public async Task<IActionResult> GetCommissionRecordsByAgentId(string agentId)
        {
            //Method Will Return The List Of Commission Records  Of Particular Agent Based On His ID

            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }
            List<CommissionRecordGetDto> commissionRecorsList = new List<CommissionRecordGetDto>();
            var commissionRecords = this._commissionRecordManager.GetAll().Where(x => x.AgentCode == agent.AgentCode.ToString()).ToList();
            foreach (var commissionRecord in commissionRecords)
            {
                commissionRecorsList.Add(new CommissionRecordGetDto()
                {
                    AgentName = commissionRecord.AgentName,
                    CommissionAmount = commissionRecord.CommissionAmount,
                    CustomerName = commissionRecord.CustomerName,
                    InsuranceAccountId = commissionRecord.InsuranceAccountId,
                    InsuranceScheme = commissionRecord.InsuranceScheme,
                    PurchasedDate = commissionRecord.PurchasedDate
                });
            }
            return this.Ok(commissionRecorsList);
        }
      
        [HttpPut]
        [Route("{agentCode}/UpdateAgent")]
        public async Task<IActionResult> UpdateAgent(int? agentCode,UpdateAgentDto model)
        {
            //Method For Update Agent Details Based On His UniqueAgent Code

            var agent=await this._userManager.Users.Where(x=>x.UserRoll==UserRoles.Agent && x.AgentCode==agentCode).FirstOrDefaultAsync();
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }
            agent.AgentCode = model.AgentCode;
            agent.Address= model.Address;
            agent.Qualification=model.Qualification;
            agent.Email=model.Email;
            agent.UserStatus = model.Status;
            agent.LoginId=model.LoginId;
            agent.UserName = model.Name;

            await this._userManager.UpdateAsync(agent);
            return this.Ok(new Response { Message = "Data Updated Successfully", Status = "Success" });
        }
        [HttpGet]
        [Route("GetAllCommissionRecords")]
        public async Task<IActionResult> GetAllCommissionRecords()
        {
            //Will Return The List Of All Commission Records IT Will Used By Admin And Employee

            List<CommissionRecordGetDto> commissions=new List<CommissionRecordGetDto>();
            var commissionRecords = this._commissionRecordManager.GetAll();
            foreach(var commissionRecord in commissionRecords)
            {
                commissions.Add(new CommissionRecordGetDto()
                {
                    AgentName = commissionRecord.AgentName,
                    CommissionAmount = commissionRecord.CommissionAmount,
                    CustomerName = commissionRecord.CustomerName,
                    InsuranceAccountId = commissionRecord.InsuranceAccountId,
                    InsuranceScheme = commissionRecord.InsuranceScheme,
                    PurchasedDate = commissionRecord.PurchasedDate
                });
            }
            return  this.Ok(commissions);

        }
        [HttpPost]
        [Route("{agentId}/AddCustomer")]
        public async Task<IActionResult> AddCustomer(string agentId,[FromBody] CustomerAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var agentExists= await this._userManager.FindByIdAsync(agentId);
            if (agentExists == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found To Given Id" });
            }
            var customerExists = await this._userManager.FindByEmailAsync(model.Email);
            if (customerExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Customer Already Exists" });
            }

            var loginIdExists = this._userManager.Users.ToList().Find(x => x.LoginId == model.LoginId);
            Debug.WriteLine("The Login Id Exists : " + loginIdExists);
            if (loginIdExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "LoginId Already Exists Use Another LoginId" });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password And Confirm Password Does Not Match" });
            }
            ApplicationUser customer = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Name,
                UserRoll = UserRoles.Customer,
                SecurityStamp = Guid.NewGuid().ToString(),
                LoginId = model.LoginId,
                UserStatus = AccountStatus.Active,
                Address = model.Address,
                City = model.City,
                DateOfBirth = model.DateOfBirth.ToShortDateString(),
                NomineeName = model.NomineeName,
                NomineeRelation = model.NomineeRelation,
                PinCode = model.PinCode,
                PhoneNumber = model.MobileNumber,
                State = model.State,
                AgentCode = int.Parse(model.AgentCode)

            };
            var result = await this._userManager.CreateAsync(customer, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Customer Not Created" });
            }
            if (!await this._roleManager.RoleExistsAsync(UserRoles.Customer))
            {
                await this._roleManager.CreateAsync(new IdentityRole(UserRoles.Customer));
            }
            if (await this._roleManager.RoleExistsAsync(UserRoles.Customer))
            {
                await this._userManager.AddToRoleAsync(customer, UserRoles.Customer);
            }

            return this.Ok(new Response { Message = "Customer Created Successfully", Status = "Success" });
        }
    }
}
