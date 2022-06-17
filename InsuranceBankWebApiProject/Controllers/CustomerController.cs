using EnsuranceProjectEntityLib.Model.AdminModel;
using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAllRepository<State> _stateManager;
        private readonly IAllRepository<City> _cityManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAllRepository<InsuranceAccount> _insuranceAccountManager;
        public CustomerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,BankInsuranceDbContext bankInsuranceDb)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
            this._stateManager=new AllRepository<State>(bankInsuranceDb);
            this._cityManager = new AllRepository<City>(bankInsuranceDb);
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(bankInsuranceDb);
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] CustomerAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
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
                City= model.City,
                DateOfBirth= model.DateOfBirth.ToShortDateString(),
                NomineeName= model.NomineeName,
                NomineeRelation= model.NomineeRelation,
                PinCode= model.PinCode,
                PhoneNumber=model.MobileNumber,
                State=model.State,
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
        [HttpPost]
        [Route("GetAllCustomers")]
        public async Task<List<CustomerGetDto>> GetAllCustomers()
        {
            List<CustomerGetDto> customersList= new List<CustomerGetDto>();
            var customers = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Customer).ToListAsync();
            foreach(var customer in customers)
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
                    Status = customer.UserStatus,

                });
            }
            return customersList;

        }
        [HttpPut]
        [Route("{customerId}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(string customerId, ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var userExist = await this._userManager.FindByIdAsync(customerId);
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
        
        [HttpPost]
        [Route("AddInsuranceAccount")]
        public async Task<IActionResult> AddInsuranceAccount(InsuranceAccountAddDto model)
        {
            var customer=await this._userManager.FindByIdAsync(model.CustomerId);
            if(customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Customer Not Found With Given Id" });
            }
            var agentCodeExists=await this._userManager.Users.Where(x=>x.UserRoll==UserRoles.Agent && x.AgentCode==int.Parse(model.AgentCode)).FirstOrDefaultAsync();
            if(agentCodeExists == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Agent Code" });
            }
            var insuranceAccount = new InsuranceAccount()
            {
                AgentCode = model.AgentCode,
                CustomerId = model.CustomerId,
                CustomerName = model.CustomerName,
                DateCreated = model.DateCreated,
                InstallmentAmount = model.InstallmentAmount,
                InsuranceScheme = model.InsuranceScheme,
                InsuranceType = model.InsuranceType,
                InterestAmount = model.InterestAmount,
                InvestmentAmount = model.InvestmentAmount,
                MaturityDate = model.MaturityDate,
                NumberOfYears = model.NumberOfYears,
                PremiumType = model.PremiumType,
                ProfitRatio = model.ProfitRatio,
                TotalAmount = model.TotalAmount,
            };
            await this._insuranceAccountManager.Add(insuranceAccount);
            return this.Ok(new Response { Status = "Success", Message = "Insurance Plan Purchased Successfully" });

        }
        [HttpGet]
        [Route("GetAllInsuranceAccounts")]
        public async Task<List<InsuranceAccountGetDto>> GetAllInsuranceAccounts()
        {
            List<InsuranceAccountGetDto> insuranceAccountsList=new List<InsuranceAccountGetDto>();
            var insuranceAccounts = this._insuranceAccountManager.GetAll();
            foreach(var insuranceAccount in insuranceAccounts)
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
