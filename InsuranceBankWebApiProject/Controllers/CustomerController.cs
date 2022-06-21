using EnsuranceProjectEntityLib.Model.AdminModel;
using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectEntityLib.Model.Insurance;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.Owin;
using System.Transactions;
using System.Web;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAllRepository<InsuranceScheme> _insuranceSchemeManager;
        private readonly IAllRepository<CommissionRecord> _commissionRecordManager;
        private readonly IAllRepository<Payment> _paymentManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAllRepository<InsuranceAccount> _insuranceAccountManager;
        private readonly IAllRepository<Query> _queryManager;
        public CustomerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,BankInsuranceDbContext bankInsuranceDb)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(bankInsuranceDb);
            this._commissionRecordManager=new AllRepository<CommissionRecord>(bankInsuranceDb);
            this._insuranceSchemeManager=new AllRepository<InsuranceScheme>(bankInsuranceDb);
            this._queryManager=new AllRepository<Query>(bankInsuranceDb);
            this._paymentManager=new AllRepository<Payment>(bankInsuranceDb);
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
        [HttpGet]
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
            var agentCodeExists = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Agent && x.AgentCode == model.AgentCode).FirstOrDefaultAsync();
            if (agentCodeExists == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Agent Code" });
            }

            //var a = (this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceSchemeName == model.InsuranceScheme && x.InsuranceTypeName == model.InsuranceType).Select(x => x.NewRegComission).FirstOrDefault() * model.InvestmentAmount)/100 ;
            //Debug.WriteLine("The Result : " + a);
            //using (this._bankInsuranceDbContext)
            //{
            //    using (var t = this._bankInsuranceDbContext.Database.BeginTransaction())
            //    {
            //        try
            //        {
            //            //AppUserManager appUserManager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();

            //            //AppUser member = await appUserManager.FindByIdAsync(User.Identity.GetUserId());

            //            //member.HasScheduledChanges = true;

            //            //IdentityResult identityResult = appUserManager.Update(member);
            //            //scope.Complete(); 
            //            var insuranceAccount = new InsuranceAccount()
            //            {
            //                AgentCode = model.AgentCode,
            //                CustomerId = model.CustomerId,
            //                CustomerName = model.CustomerName,
            //                DateCreated = model.DateCreated,
            //                InstallmentAmount = model.InstallmentAmount,
            //                InsuranceScheme = model.InsuranceScheme,
            //                InsuranceType = model.InsuranceType,
            //                InterestAmount = model.InterestAmount,
            //                InvestmentAmount = model.InvestmentAmount,
            //                MaturityDate = model.MaturityDate,
            //                NumberOfYears = model.NumberOfYears,
            //                PremiumType = model.PremiumType,
            //                ProfitRatio = model.ProfitRatio,
            //                TotalAmount = model.TotalAmount,
            //            };
            //            await this._insuranceAccountManager.Add(insuranceAccount);


            //            //var insId =await this._insuranceAccountManager.GetAll().Where(x => x.CustomerId == model.CustomerId).Select(x => x.Id).FirstOrDefault();
            //            //var agentName = this._userManager.Users.Where(x => x.AgentCode == int.Parse(model.AgentCode) && x.UserRoll == UserRoles.Agent).Select(x=>x.UserName).FirstOrDefault();
            //            //if ("Ram" != null)
            //            //{

            //            //}
            //            await this._commissionRecordManager.Add(new CommissionRecord()
            //            {
            //                AgentCode = model.AgentCode,
            //                AgentName = "Dinesh",
            //                CustomerId = model.CustomerId,
            //                CustomerName = model.CustomerName,
            //                InsuranceAccountId = 2,
            //                InsuranceScheme = model.InsuranceScheme,
            //                PurchasedDate = model.DateCreated,
            //                CommissionAmount = 9000 //(this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceSchemeName == model.InsuranceScheme && x.InsuranceTypeName == model.InsuranceType).Select(x => x.NewRegComission).FirstOrDefault() * model.InvestmentAmount)/100

            //            });

            //            t.Commit();
            //            return this.Ok(new Response { Status = "Success", Message = "Insurance Plan Purchased Successfully" });

            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine(ex.Message);
            //            t.Rollback();
            //            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = ex.Message });
            //        }
            //    }

            //}


            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {


                try
                {
                    //using (this._bankInsuranceDbContext)
                    //{


                    //AppUserManager appUserManager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();

                    //AppUser member = await appUserManager.FindByIdAsync(User.Identity.GetUserId());

                    //member.HasScheduledChanges = true;

                    //IdentityResult identityResult = appUserManager.Update(member);
                    //scope.Complete();
                    //var appUserManager = HttpContext.GetOwinContext().GetUserManager<InsuranceAccount>();
                    var insuranceAccountNumber = Guid.NewGuid().ToString();
                       
                        var insuranceAccount = new InsuranceAccount()
                        {
                            AgentCode = model.AgentCode.ToString(),
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
                            AccountNumber=insuranceAccountNumber
                        };

                    await this._insuranceAccountManager.Add(insuranceAccount);

                    await this._paymentManager.Add(new Payment()
                    {
                        CustomerId = model.CustomerId,
                        InstallmentAmount = model.InstallmentAmount,
                        InstallmentDate = DateTime.Now.ToShortDateString(),
                        InstallmentNumber = 1,
                        PaidDate = DateTime.Now.ToShortDateString(),
                        InsuranceAccountNumber=insuranceAccountNumber,
                        PaymentStatus = "Paid"
                    });
                    await this._commissionRecordManager.Add(new CommissionRecord()
                    {
                        AgentCode = model.AgentCode.ToString(),
                        AgentName = agentCodeExists.UserName,
                        CustomerId = model.CustomerId,
                        CustomerName = model.CustomerName,
                        InsuranceAccountId = insuranceAccountNumber,
                        InsuranceScheme = model.InsuranceScheme,
                        PurchasedDate = model.DateCreated,
                        CommissionAmount = (this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceSchemeName == model.InsuranceScheme && x.InsuranceTypeName == model.InsuranceType).Select(x => x.NewRegComission).FirstOrDefault() * model.InvestmentAmount) / 100

                    });

                    //}
                    //var insId =await this._insuranceAccountManager.GetAll().Where(x => x.CustomerId == model.CustomerId).Select(x => x.Id).FirstOrDefault();
                    //var agentName = this._userManager.Users.Where(x => x.AgentCode == int.Parse(model.AgentCode) && x.UserRoll == UserRoles.Agent).Select(x=>x.UserName).FirstOrDefault();
                    //if ("Ram" != null)
                    //{
                    //using (this._bankInsuranceDbContext)
                    //{



                    scope.Complete();
                        //await this._bankInsuranceDbContext.DisposeAsync();
                        return this.Ok(new Response { Status = "Success", Message = "Insurance Plan Purchased Successfully" });
                        //}
                   // }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    scope.Dispose();
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = ex.Message });
                }
            }

            //return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong" });

        }
       

        [HttpGet]
        [Route("{customerId}/GetCustomerById")]
        public async Task<IActionResult> GetCustomerById(string customerId)
        {
            var customer=await this._userManager.FindByIdAsync(customerId);
            if(customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,new Response { Message = "User Not Found", Status = "Error" });
            }

            return this.Ok(new CustomerGetDto
            {
                Name = customer.UserName,
                LoginId = customer.LoginId,
                Address = customer.Address,
                DateOfBirth = customer.DateOfBirth,
                Email = customer.Email,
                MobileNumber = customer.PhoneNumber,
                NomineeName = customer.NomineeName,
                NomineeRelation = customer.NomineeRelation,
                Status = customer.UserStatus,
                City=customer.City,
                PinCode= customer.PinCode.ToString(),
                State=customer.State
            });
        }

        [HttpGet]
        [Route("{customerId}/GetCustomerNameAndAgentCode")]
        public async Task<IActionResult> GetCustomerNameAndAgentCode(string customerId)
        {
            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "User Not Found", Status = "Error" });
            }

            return this.Ok(new CustomerNameAndAgentCodeDto()
            {
                AgentCode=customer.AgentCode.GetValueOrDefault(),
                CustomerName=customer.UserName
            });
        }

       

        [HttpPut]
        [Route("{customerId}/UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(string customerId,CustomerUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "All Fields Are Required", Status = "Error" });
            }
            var customer=await this._userManager.FindByIdAsync(customerId);
            if(customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }

            customer.UserName = model.Name;
            customer.State=model.State;
            customer.Email = model.Email;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.PhoneNumber = model.MobileNumber;
            customer.NomineeName = model.NomineeName;
            customer.NomineeRelation= model.NomineeRelation;
            customer.PinCode= model.PinCode;
            customer.LoginId= model.LoginId;
            customer.DateOfBirth = model.DateOfBirth;

            await this._userManager.UpdateAsync(customer);
            return this.Ok(new Response { Message = "Data Updated Successfully", Status = "Success" });
        }

        //[HttpGet]
        //[Route("{customerId}/GetCutomerAge")]
        //public async Task<IActionResult> GetCustomerAge(string customerId)
        //{
        //    var customer = await this._userManager.FindByIdAsync(customerId);
        //    if(customer == null)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
        //    }

        //    var dateOfBirth= await this._userManager.Users.Where(x=>x.Id==customerId).FirstOrDefaultAsync();

        //}
        [HttpPost]
        [Route("AddQuery")]
        public async Task<IActionResult> AddQuery(AddQueryDto query)
        {
            var customer=await this._userManager.FindByIdAsync(query.CustomerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }


            await this._queryManager.Add(new Query()
            {
                ContactDate = DateTime.Now.ToShortDateString(),
                CustomerName = customer.UserName,
                Message = query.Message,
                Reply = "",
                Status = QueryStatus.Pending,
                Title = query.Title,
                CustomerId=customer.Id
            });
            return this.Ok(new Response { Status = "Success", Message = "Your Query Added Successfully" });
        }
        [HttpGet]
        [Route("{customerId}/GetAllQueriesByCustomerId")]
        public async Task<IActionResult> GetAllQueriesByCustomerId(string customerId)
        {
            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }
            List<GetQueryDto> queryList = new List<GetQueryDto>();
            var queries=this._queryManager.GetAll().Where(x=>x.CustomerId==customerId).ToList();
            foreach(var query in queries)
            {
                queryList.Add(new GetQueryDto()
                {
                    ContactDate = DateTime.Now.ToShortDateString(),
                    CustomerName = query.CustomerName,
                    Message = query.Message,
                    Reply = query.Reply,
                    Title = query.Title
                });
            }
            return this.Ok(queryList);
        }
        [HttpGet]
        [Route("GetAllQueries")]
        public async Task<IActionResult> GetAllQueries()
        {
            List<GetQueryDto> queryList = new List<GetQueryDto>();
            var queries = this._queryManager.GetAll().Where(x=>x.Status==QueryStatus.Pending);
            foreach (var query in queries)
            {
                queryList.Add(new GetQueryDto()
                {
                    ContactDate = DateTime.Now.ToShortDateString(),
                    CustomerName = query.CustomerName,
                    Message = query.Message,
                    Reply = query.Reply,
                    Title = query.Title,
                    CustomerId=query.CustomerId,
                    QueryId=query.Id
                });
            }
            return this.Ok(queryList);
        }

        [HttpPost]
        [Route("SolveQuery")]
        public async Task<IActionResult> SolveQuery(SolveQueryDto query)
        {
            var customer= await this._userManager.FindByIdAsync(query.CustomerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not TonGive Feedback Found", Status = "Error" });
            }

            var customerQuery = this._queryManager.GetAll().Find(x => x.Id == query.QueryId);
            if (customerQuery != null)
            {
                customerQuery.Reply = query.Reply;
                customerQuery.Status = QueryStatus.Success;
                await this._queryManager.Update(customerQuery);
                return this.Ok(new Response { Status = "Success", Message = "Feedback Given Successfully" });

            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Query Not To Give Feedback Found", Status = "Error" });
        }
    }
}
