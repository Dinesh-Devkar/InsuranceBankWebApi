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
using Microsoft.AspNetCore.Authorization;

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
        private readonly IAllRepository<AgentTransaction> _agentTransactionManager;

        public AgentController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,BankInsuranceDbContext dbContext)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(dbContext);
            this._commissionRecordManager = new AllRepository<CommissionRecord>(dbContext);
            this._agentTransactionManager=new AllRepository<AgentTransaction>(dbContext);
        }
        [HttpPost]
        [Route("Register")]
        [Authorize(Roles =UserRoles.Admin+","+UserRoles.Employee)]
        public async Task<IActionResult> Register([FromBody] AgentAddDto model)
        {
            // Registration For Agent
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }

            //Checking is email already exists
            var agentExist = await this._userManager.FindByEmailAsync(model.Email);
            if (agentExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Already Exists" });
            }
            
            //Checking is loginId already exists login id should be unique for each user
            var loginIdExists = this._userManager.Users.ToList().Find(x=>x.LoginId==model.LoginId);
            Debug.WriteLine("The Login Id Exists : " + loginIdExists);
            if (loginIdExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "LoginId Already Exists Use Another LoginId" });
            }

            //Checking is agentCode exists agentCode should be unique for each agent
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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<List<AgentGetDto>> GetAllAgents()
        {
            //Will Return The List Of All Agents
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
                    Status = agent.UserStatus,
                    Id=agent.Id

                });
            }
            return agentsList;

        }
        [HttpGet]
        [Route("{agentId}/GetAgentById")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee+","+UserRoles.Agent)]
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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee + "," + UserRoles.Agent)]
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
        [Authorize(Roles = UserRoles.Agent)]
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
        [Authorize(Roles = UserRoles.Agent)]
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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee + "," + UserRoles.Agent)]
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
                    PurchasedDate = commissionRecord.PurchasedDate,
                    CommissionType=commissionRecord.CommissionType
                });
            }
            return this.Ok(commissionRecorsList);
        }

      
        [HttpPut]
        [Route("{agentId}/UpdateAgent")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee + "," + UserRoles.Agent)]
        public async Task<IActionResult> UpdateAgent(string agentId, UpdateAgentDto model)
        {
            //Method For Update Agent Details Based On His AgentId

            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found" });
            }

            if (agent.AgentCode != model.AgentCode)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Agent Code Provided" });
            }
            if(agent.Email != model.Email)
            {
                var isEmailExists = await this._userManager.FindByEmailAsync(model.Email);
                if (isEmailExists != null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Email Already Taken Use Another Email" }); ;
                }
            }
            if (agent.LoginId != model.LoginId)
            {
                var isLoginIdExists = await this._userManager.Users.Where(x => x.LoginId == model.LoginId).FirstOrDefaultAsync();
                if (isLoginIdExists != null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "LoginId Already Exists Use Another LoginId" }); ;
                }
            }
            agent.AgentCode = model.AgentCode;
            agent.Address = model.Address;
            agent.Qualification = model.Qualification;
            agent.Email = model.Email;
            agent.UserStatus = model.Status;
            agent.LoginId = model.LoginId;
            agent.UserName = model.Name;

            await this._userManager.UpdateAsync(agent);
            return this.Ok(new Response { Message = "Data Updated Successfully", Status = "Success" });
        }
        [HttpGet]
        [Route("GetAllCommissionRecords")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> GetAllCommissionRecords()
        {
            //Will Return The List Of All Commission Records It Will Used By Admin And Employee

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
                    PurchasedDate = commissionRecord.PurchasedDate,
                    CommissionType=commissionRecord.CommissionType
                });
            }
            return  this.Ok(commissions);

        }
        [HttpPost]
        [Route("{agentId}/AddCustomer")]
        [Authorize(Roles = UserRoles.Agent)]
        public async Task<IActionResult> AddCustomer(string agentId,[FromBody] AgentCustomerAddDto model)
        {
            //Register customer method for agent customer who came via agent
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
        [HttpGet]
        [Route("{agentId}/GetBalance")]
        [Authorize(Roles = UserRoles.Agent)]
        public async Task<IActionResult> GetBalance(string agentId)
        {
            //return the total balance of the agent how much he earn total commission
            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found With Given Id" });
            }

            var balance = await this._userManager.Users.Where(x => x.Id == agentId).Select(x => x.Balance).FirstOrDefaultAsync();
            return this.Ok(balance);
        }
        
        [HttpPost]
        [Route("{agentId}/Withdraw")]
        [Authorize(Roles = UserRoles.Agent)]
        public async Task<IActionResult> Withdraw(string agentId,AgentTransactionDto model)
        {
            //method to withdraw the agent commission

            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found With Given Id" });
            }

            var balance = await this._userManager.Users.Where(x => x.Id == agentId).Select(x => x.Balance).FirstOrDefaultAsync();
            if(model.Amount < 0)
            {
                return this.Ok(new Response { Message = "Invalid Balance", Status = "Error" });
            }
            if(balance < model.Amount)
            {
                return this.Ok(new Response { Message = "Insufficient Balance", Status = "Error" });
            }
            if (model.Amount < balance)
            {
                agent.Balance -= model.Amount;
                await this._userManager.UpdateAsync(agent);
                await this._agentTransactionManager.Add(new AgentTransaction()
                {
                    AgentId = agentId,
                    Amount = model.Amount,
                    TransactionDate=DateTime.Now.ToShortDateString(),
                    TransactionType = "Withdraw"
                });
                return this.Ok(new Response { Message = "Transaction Successfull", Status = "Success" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Transaction Unsuccessfull" });
        }
        [HttpGet]
        [Route("{agentId}/GetTransactions")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee + "," + UserRoles.Agent)]
        public async Task<IActionResult> GetTransactions(string agentId)
        {
            // it will return the list of particular agent transactions it will use by agent employe and admin

            var agent = await this._userManager.FindByIdAsync(agentId);
            if (agent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found With Given Id" });
            }
            var transactionsList = new List<AgentTransactionGetDto>();
            var transactions=this._agentTransactionManager.GetAll().Where(x=>x.AgentId == agentId).ToList();
            foreach (var transaction in transactions)
            {
                transactionsList.Add(new AgentTransactionGetDto()
                {
                    AgentId = transaction.AgentId,
                    Amount = transaction.Amount,
                    TransactionDate = transaction.TransactionDate,
                    TransactionType = transaction.TransactionType
                });
            }
            return this.Ok(transactionsList);
        }
    }
}
