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
using Microsoft.AspNetCore.Authorization;
using InsuranceBankWebApiProject.DtoClasses.Payment;

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
        private readonly IAllRepository<CustomerDocument> _documentManager;
        public CustomerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,BankInsuranceDbContext bankInsuranceDb)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(bankInsuranceDb);
            this._commissionRecordManager=new AllRepository<CommissionRecord>(bankInsuranceDb);
            this._insuranceSchemeManager=new AllRepository<InsuranceScheme>(bankInsuranceDb);
            this._queryManager=new AllRepository<Query>(bankInsuranceDb);
            this._paymentManager=new AllRepository<Payment>(bankInsuranceDb);
            this._documentManager = new AllRepository<CustomerDocument>(bankInsuranceDb);
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(CustomerAddDto model)
        {
            //method to register a customer without agent
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }

            //First check is email already exists
            var customerExists = await this._userManager.FindByEmailAsync(model.Email);
            if (customerExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Customer Already Exists With Given Email" });
            }

            //Check is LoginId already exists
            var loginIdExists = this._userManager.Users.ToList().Find(x => x.LoginId == model.LoginId);
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
                DateOfBirth = model.DateOfBirth,
                NomineeName = model.NomineeName,
                NomineeRelation = model.NomineeRelation,
                PinCode = int.Parse(model.PinCode),
                PhoneNumber = model.MobileNumber,
                State = model.State

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
        [Authorize(Roles =UserRoles.Admin+","+UserRoles.Employee)]
        public async Task<List<CustomerGetDto>> GetAllCustomers()
        {
            //return a list of all customers

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
                    CustomerId=customer.Id

                });
            }
            return customersList;

        }
        

        [HttpPut]
        [Route("{customerId}/ChangePassword")]
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> ChangePassword(string customerId, ChangePasswordModel model)
        {
            //update password method for customer

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
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> AddInsuranceAccount(InsuranceAccountAddDto model)
        {
            //method to purchase insurance plan if insurance plan purchased successfully insurance account is created

            var customer=await this._userManager.FindByIdAsync(model.CustomerId);
            if(customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Customer Not Found With Given Id" });
            }
            var agentCodeExists = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Agent && x.AgentCode == model.AgentCode).FirstOrDefaultAsync();
            string agentName = null;
            if (agentCodeExists != null)
            {
                agentName = agentCodeExists.UserName;
            }

            //created transaction scope for insurance account, payment and commissionrecords
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {                  
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
                            AccountNumber=insuranceAccountNumber,
                            NumberOfInstallments = model.NumberOfInstallments,
                            PendingInstallments=model.NumberOfInstallments-1,
                            IsPolicyClaimed="False",
                            PolicyStatus=PolicyStatus.Pending
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

                    //If customer came via agent then commission record will added for agent
                    if (agentName != null)
                    {
                        await this._commissionRecordManager.Add(new CommissionRecord()
                        {
                            AgentCode = model.AgentCode.ToString(),
                            AgentName = agentName,
                            CustomerId = model.CustomerId,
                            CustomerName = model.CustomerName,
                            InsuranceAccountId = insuranceAccountNumber,
                            InsuranceScheme = model.InsuranceScheme,
                            PurchasedDate = model.DateCreated,
                            CommissionType="New Registration",
                            CommissionAmount = (this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceSchemeName == model.InsuranceScheme && x.InsuranceTypeName == model.InsuranceType).Select(x => x.NewRegComission).FirstOrDefault() * model.InvestmentAmount) / 100

                        });
                        //If Commission Added Successfully in Database Then agent balance will be updated  with addition commission amount

                        agentCodeExists.Balance += (this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceSchemeName == model.InsuranceScheme && x.InsuranceTypeName == model.InsuranceType).Select(x => x.NewRegComission).FirstOrDefault() * model.InvestmentAmount) / 100;
                        await this._userManager.UpdateAsync(agentCodeExists);

                    }
                       scope.Complete();
                       return this.Ok(new Response { Status = "Success", Message = "Insurance Plan Purchased Successfully" });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    scope.Dispose();
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = ex.Message });
                }
            }
        }
       

        [HttpGet]
        [Route("{customerId}/GetCustomerById")]
        [Authorize(Roles = UserRoles.Customer+","+UserRoles.Admin+","+UserRoles.Agent+","+UserRoles.Employee)]
        public async Task<IActionResult> GetCustomerById(string customerId)
        {
            //return a particular customer details based on his Id

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
                State=customer.State,
                CustomerId=customer.Id
            });
        }

        [HttpGet]
        [Route("{customerId}/GetCustomerNameAndAgentCode")]
        public async Task<IActionResult> GetCustomerNameAndAgentCode(string customerId)
        {
            //return customer name and his agent code based on customer id
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
        [Authorize(Roles= UserRoles.Customer + "," + UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> UpdateCustomer(string customerId,CustomerUpdateDto model)
        {
            //To Update Customer details
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "All Fields Are Required", Status = "Error" });
            }
            var customer=await this._userManager.FindByIdAsync(customerId);
            if(customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }

            if(customer.Email != model.Email)
            {
                var isEmailExists = await this._userManager.FindByEmailAsync(model.Email);
                if (isEmailExists != null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Email Already Taken Use Another Email" }); ;
                }
            }
            if (customer.LoginId != model.LoginId)
            {
                var isLoginIdExists = await this._userManager.Users.Where(x => x.LoginId == model.LoginId).FirstOrDefaultAsync();
                if (isLoginIdExists != null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "LoginId Already Exists Use Another LoginId" }); ;
                }
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
            customer.UserStatus = model.Status;

            await this._userManager.UpdateAsync(customer);
            return this.Ok(new Response { Message = "Data Updated Successfully", Status = "Success" });
        }

        [HttpPost]
        [Route("AddQuery")]
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> AddQuery(AddQueryDto query)
        {
            //to add a customer query
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
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> GetAllQueriesByCustomerId(string customerId)
        {
            //To get all queries of a particular customer for customer dashboard

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
                    ContactDate = query.ContactDate,
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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> GetAllQueries()
        {
            //To get a list of all queries

            List<GetQueryDto> queryList = new List<GetQueryDto>();
            var queries = this._queryManager.GetAll().Where(x=>x.Status==QueryStatus.Pending);
            foreach (var query in queries)
            {
                queryList.Add(new GetQueryDto()
                {
                    ContactDate = query.ContactDate,
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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> SolveQuery(SolveQueryDto query)
        {
            //To give a feedback to a particular query
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
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Query Not Found To Give Feedback", Status = "Error" });
        }

        [HttpPost]
        [Route("{customerId}/AddDocument")]
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> AddDocument(string customerId,CustomerAddDocumentDto model)
        {
            //To add documents for a customer
            var customer =await this._userManager.FindByIdAsync(customerId);
            if(customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }
            var isDocumentExist=this._documentManager.GetAll().Where(x=>x.CustomerId == customerId && x.DocumentName==model.DocumentName).FirstOrDefault();

            //If document already exists then it will update the document
            if(isDocumentExist != null)
            {
                isDocumentExist.DocumentName = model.DocumentName;
                isDocumentExist.DocumentFile = model.DocumentFile;
                isDocumentExist.CustomerId = customerId;
                await this._documentManager.Update(isDocumentExist);
                return this.Ok(new Response { Message = "Document Added Successfully", Status = "Success" });

            }
            await this._documentManager.Add(new CustomerDocument()
            {
                CustomerId = customerId,
                DocumentFile = model.DocumentFile,
                DocumentName = model.DocumentName,               
            });

            return this.Ok(new Response { Message = "Document Added Successfully", Status = "Success" });
        }
        [HttpGet]
        [Route("{customerId}/GetDocuments")]
        [Authorize( Roles = UserRoles.Customer + "," + UserRoles.Admin + "," +"," + UserRoles.Employee)]
        public async Task<IActionResult> GetDocuments(string customerId)
        {
            //return a list of documents of a particular customer based on his id

            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }
            List<CustomerGetDocumentDto> documents=new List<CustomerGetDocumentDto>();

            var documentsList = this._documentManager.GetAll().Where(x=>x.CustomerId==customerId).ToList();
            foreach (var document in documentsList)
            {
                documents.Add(new CustomerGetDocumentDto()
                {
                    DocumentFile = document.DocumentFile,
                    DocumentName = document.DocumentName
                });
            }
            return this.Ok(documents);
        }

        [HttpDelete]
        [Route("{customerId}/DeleteDocument")]
        [Authorize(Roles =UserRoles.Customer)]
        public async Task<IActionResult> DeleteDocument(string customerId, CustomerAddDocumentDto model)
        {
            //To Delete a document
            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }

            var document = this._documentManager.GetAll().Where(x => x.CustomerId == customerId && x.DocumentName == model.DocumentName).FirstOrDefault();

            if (document == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Given Document Not Found", Status = "Error" });
            }
            //document.DocumentFile = model.DocumentFile;
            //document.DocumentName = model.DocumentName;

            //await this._documentManager.Update(document);
            this._documentManager.Delete(document);
            return this.Ok(new Response { Message = "Document Deleted Successfully", Status = "Success" });

        }

        [HttpPost]
        [Route("{customerId}/DoPayment")]
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> DoPayment(string customerId,PaymentAddDto model)
        {
            //To pay installments of insurance plan by customer

            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }

            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "All Fields Are Required", Status = "Error" });
            }

           
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var insuranceSchemeName = this._insuranceAccountManager.GetAll().Where(x => x.CustomerId == model.CustomerId).Select(x => x.InsuranceScheme).FirstOrDefault();
                        var installmentCommission = this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceSchemeName == insuranceSchemeName).Select(x => x.InstallmentComission).FirstOrDefault();
                        var agentCode = await this._userManager.Users.Where(x => x.Id == model.CustomerId).Select(x => x.AgentCode).FirstOrDefaultAsync();
                        var agent = await this._userManager.Users.Where(x => x.UserRoll == UserRoles.Agent && x.AgentCode == agentCode).FirstOrDefaultAsync();
                        var insuranceAccount=this._insuranceAccountManager.GetAll().Find(x=>x.AccountNumber==model.InsuranceAccountNumber);
                        if (agent == null)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Agent Not Found With Given AgntCode" });
                        }
                        var payment = new Payment()
                        {
                            CustomerId = model.CustomerId,
                            InstallmentAmount = model.InstallmentAmount,
                            InstallmentDate = model.InstallmentDate,
                            InstallmentNumber = model.InstallmentNumber,
                            InsuranceAccountNumber = model.InsuranceAccountNumber,
                            PaidDate = model.PaidDate,
                            PaymentStatus = "Paid"
                        };

                        await this._paymentManager.Add(payment);
                        
                        //If payment success then pending installments will reduce by 1.
                        insuranceAccount.PendingInstallments -= 1;
                        await this._insuranceAccountManager.Update(insuranceAccount);

                        //If customer came via agent then agent will receive the customer installment payment commission
                        if (agent != null)
                        {
                            await this._commissionRecordManager.Add(new CommissionRecord()
                            {
                                AgentCode = agentCode.ToString(),
                                AgentName = agent.UserName,
                                CustomerId = model.CustomerId,
                                CustomerName = model.CustomerName,
                                InsuranceAccountId = model.InsuranceAccountNumber,
                                InsuranceScheme = model.InsuranceScheme,
                                PurchasedDate = model.PaidDate,
                                CommissionType = "Installment",
                                CommissionAmount = (installmentCommission * model.InstallmentAmount) / 100 //(this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceSchemeName == model.InsuranceScheme && x.InsuranceTypeName == model.InsuranceType).Select(x => x.NewRegComission).FirstOrDefault() * model.InvestmentAmount) / 100

                            });

                            //If Commission Added Successfully in Database Then agent balance will be updated  with addition commission amount
                            agent.Balance += (installmentCommission * model.InstallmentAmount) / 100;
                            await this._userManager.UpdateAsync(agent);

                            //If all installments are done then policy claim will be true thwn customer is able to claim the policy
                            if (model.InstallmentNumber == insuranceAccount.NumberOfInstallments)
                            {
                                insuranceAccount.IsPolicyClaimed = "True";
                                await this._insuranceAccountManager.Update(insuranceAccount);
                            }
                                scope.Complete();
                                return this.Ok(new Response { Message = "Payment Done Successfully", Status = "Success" });
                        }
                    }                   
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Transaction Fail " + ex.Message });
                    }              
                }        
           return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong Transaction Fail" });
          
        }
        [HttpPut]
        [Route("{customerId}/PolicyClaimRequest")]
        [Authorize(Roles =UserRoles.Customer)]
        public async Task<IActionResult> PolicyClaimRequest(string customerId,PolicyClaimRequestDto model)
        {
            //If all the installments are done then customer can claim the policy request
            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }
            var account = this._insuranceAccountManager.GetAll().Find(x => x.AccountNumber == model.InsuranceAccountNumber);
            if (account == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Insurance Account Not Found", Status = "Error" });
            }

            account.PolicyStatus = PolicyStatus.Requested;
            await this._insuranceAccountManager.Update(account);
            return this.Ok(new Response { Message = "Your Insurance Policy Claimed Is Requested Successfully You Will Receive A Response In Next 24 Hours", Status = "Success" });
        }
    }
}
