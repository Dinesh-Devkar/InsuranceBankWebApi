using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceAccountController : ControllerBase
    {
        private readonly IAllRepository<InsuranceAccount> _insuranceAccountManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public InsuranceAccountController(UserManager<ApplicationUser> userManager, BankInsuranceDbContext bankInsuranceDb)
        {
            this._insuranceAccountManager = new AllRepository<InsuranceAccount>(bankInsuranceDb);
            this._userManager = userManager;
        }
        [HttpGet]
        [Route("GetAllInsuranceAccounts")]
        [Authorize(Roles = UserRoles.Employee+","+UserRoles.Admin)]
        public async Task<IActionResult> GetAllInsuranceAccounts()
        {
            //Return a list of all the insurance accounts  (purchased insurance plans)
            List<InsuranceAccountShortDto> insuranceAccountsList = new List<InsuranceAccountShortDto>();
            var insuranceAccounts = this._insuranceAccountManager.GetAll();
            foreach (var insuranceAccount in insuranceAccounts)
            {
                insuranceAccountsList.Add(new InsuranceAccountShortDto()
                {
                    DateCreated = insuranceAccount.DateCreated,
                    InsuranceScheme = insuranceAccount.InsuranceScheme,
                    InsuranceType = insuranceAccount.InsuranceType,
                    InvestmentAmount = insuranceAccount.InvestmentAmount,
                    MaturityDate = insuranceAccount.MaturityDate,
                    PremiumType = insuranceAccount.PremiumType,
                    ProfitRatio = insuranceAccount.ProfitRatio,
                    TotalAmount = insuranceAccount.TotalAmount,
                    AccountNumber = insuranceAccount.AccountNumber,
                    CustomerName=insuranceAccount.CustomerName
                });
            }
            return this.Ok(insuranceAccountsList);
        }

        [HttpGet]
        [Route("{customerId}/GetInsuranceAccountsByCustomerId")]
        [Authorize(Roles = UserRoles.Employee + "," + UserRoles.Admin+","+UserRoles.Customer)]
        public async Task<IActionResult> GetInsuranceAccountsByCustomerId(string customerId)
        {
            //Return a list insurance plan purchased based on customer id
            List<InsuranceAccountShortDto> insuranceAccounts = new List<InsuranceAccountShortDto>();
            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }
            var accounts = this._insuranceAccountManager.GetAll().Where(x => x.CustomerId == customerId).ToList();
            foreach (var account in accounts)
            {
                insuranceAccounts.Add(new InsuranceAccountShortDto()
                {
                    DateCreated = account.DateCreated,
                    InsuranceScheme = account.InsuranceScheme,
                    InsuranceType = account.InsuranceType,
                    InvestmentAmount = account.InvestmentAmount,
                    MaturityDate = account.MaturityDate,
                    PremiumType = account.PremiumType,
                    ProfitRatio = account.ProfitRatio,
                    TotalAmount = account.TotalAmount,
                    AccountNumber=account.AccountNumber
                });
            }
            return this.Ok(insuranceAccounts);
        }

        [HttpGet]
        [Route("{accountId}/GetInsuranceAccountByAccountId")
             [Authorize(Roles = UserRoles.Employee + "," + UserRoles.Admin + "," + UserRoles.Customer+","+UserRoles.Agent)]
        public async Task<IActionResult> GetInsuranceAccountByAccountId(string accountId)
        {
            //method will return the particular insurance account detail based on customer and account ID
            //will return one single insurance plan purchase details
            List<InsuranceAccountShortDto> insuranceAccounts = new List<InsuranceAccountShortDto>();

            var account = this._insuranceAccountManager.GetAll().Find(x => x.AccountNumber == accountId);
            if (account == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Insurance Account Not Found", Status = "Error" });
            }
            return this.Ok(new InsuranceAccountShortDto()
            {
                DateCreated = account.DateCreated,
                InsuranceScheme = account.InsuranceScheme,
                InsuranceType = account.InsuranceType,
                InvestmentAmount = account.InvestmentAmount,
                MaturityDate = account.MaturityDate,
                PremiumType = account.PremiumType,
                ProfitRatio = account.ProfitRatio,
                TotalAmount = account.TotalAmount,
                AccountNumber = account.AccountNumber,
                CustomerName=account.CustomerName,
                CustomerId=account.CustomerId,
                NumberOfInstallments=account.NumberOfInstallments,
                PendingInstallments=account.PendingInstallments,
                PolicyStatus=account.PolicyStatus
            });
        }

        [HttpGet]
        [Route("GetAllRequestedPolicyClaims")]
        [Authorize(Roles =UserRoles.Admin+","+UserRoles.Employee)]
        public async Task<IActionResult> GetAllRequestedPolicyClaims()
        {
            // return a list of policies which are requested for claim
            List<InsuranceAccountShortDto> policyClaimRequestAccounts = new List<InsuranceAccountShortDto>();
            var insuranceAccounts = this._insuranceAccountManager.GetAll().Where(x => x.PolicyStatus == PolicyStatus.Requested).ToList();
            foreach(var account in insuranceAccounts)
            {
                policyClaimRequestAccounts.Add(new InsuranceAccountShortDto
                {
                    AccountNumber = account.AccountNumber,
                    InvestmentAmount = account.InvestmentAmount,
                    PolicyStatus = account.PolicyStatus,
                    CustomerId = account.CustomerId,
                    CustomerName = account.CustomerName,
                    DateCreated = account.DateCreated,
                    InsuranceScheme = account.InsuranceScheme,
                    InsuranceType = account.InsuranceType,
                    MaturityDate = account.MaturityDate,
                    NumberOfInstallments = account.NumberOfInstallments,
                    PendingInstallments = account.PendingInstallments,
                    PremiumType = account.PremiumType,
                    ProfitRatio = account.ProfitRatio,
                    TotalAmount = account.TotalAmount
                });
            }
            return this.Ok(policyClaimRequestAccounts);
        }

        [HttpPut]
        [Route("ClaimRequestApprove")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> ClaimRequestApprove(InsuranceAccountShortDto model)
        {
            //Approve a claim request of a customer insurance policy by admin or employee
            var account = this._insuranceAccountManager.GetAll().Find(x => x.AccountNumber == model.AccountNumber);
            if (account == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Insurance Account Not Found", Status = "Error" });
            }
            account.PolicyStatus = PolicyStatus.Claimed;
            await this._insuranceAccountManager.Update(account);
            return this.Ok(new Response { Message = "Policy Approve Successfully", Status = "Success" });
        }

        [HttpGet]
        [Route("GetAllClaimedPolicies")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> GetAllClaimedPolicies()
        {
            // return a list of policies which claimes
            List<InsuranceAccountShortDto> policyClaimAccounts = new List<InsuranceAccountShortDto>();
            var insuranceAccounts = this._insuranceAccountManager.GetAll().Where(x => x.PolicyStatus == PolicyStatus.Claimed).ToList();
            foreach (var account in insuranceAccounts)
            {
                policyClaimAccounts.Add(new InsuranceAccountShortDto
                {
                    AccountNumber = account.AccountNumber,
                    InvestmentAmount = account.InvestmentAmount,
                    PolicyStatus = account.PolicyStatus,
                    CustomerId = account.CustomerId,
                    CustomerName = account.CustomerName,
                    DateCreated = account.DateCreated,
                    InsuranceScheme = account.InsuranceScheme,
                    InsuranceType = account.InsuranceType,
                    MaturityDate = account.MaturityDate,
                    NumberOfInstallments = account.NumberOfInstallments,
                    PendingInstallments = account.PendingInstallments,
                    PremiumType = account.PremiumType,
                    ProfitRatio = account.ProfitRatio,
                    TotalAmount = account.TotalAmount
                });
            }
            return this.Ok(policyClaimAccounts);
        }
        [HttpGet]
        [Route("{customerId}/GetAllClaimedPoliciesByCustomerId")]
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> GetAllClaimedPoliciesByCustomerId(string customerId)
        {
            // return a list of policies which claimes by customer Id
            var customer = await this._userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            }
            List<InsuranceAccountShortDto> policyClaimAccounts = new List<InsuranceAccountShortDto>();
            var insuranceAccounts = this._insuranceAccountManager.GetAll().Where(x => x.PolicyStatus == PolicyStatus.Claimed && x.CustomerId==customerId).ToList();
            foreach (var account in insuranceAccounts)
            {
                policyClaimAccounts.Add(new InsuranceAccountShortDto
                {
                    AccountNumber = account.AccountNumber,
                    InvestmentAmount = account.InvestmentAmount,
                    PolicyStatus = account.PolicyStatus,
                    CustomerId = account.CustomerId,
                    CustomerName = account.CustomerName,
                    DateCreated = account.DateCreated,
                    InsuranceScheme = account.InsuranceScheme,
                    InsuranceType = account.InsuranceType,
                    MaturityDate = account.MaturityDate,
                    NumberOfInstallments = account.NumberOfInstallments,
                    PendingInstallments = account.PendingInstallments,
                    PremiumType = account.PremiumType,
                    ProfitRatio = account.ProfitRatio,
                    TotalAmount = account.TotalAmount
                });
            }
            return this.Ok(policyClaimAccounts);
        }
    }
}
