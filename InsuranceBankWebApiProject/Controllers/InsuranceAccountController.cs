using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Customer;
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
        public async Task<IActionResult> GetAllInsuranceAccounts()
        {
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
        public async Task<IActionResult> GetInsuranceAccountsByCustomerId(string customerId)
        {
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
                    //AccountNumber = account.Id,
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

        //[HttpGet]
        //[Route("{customerId}/GetInsuranceAccountByAccountId/{accountId}")]
        //public async Task<IActionResult> GetInsuranceAccountByAccountId(string customerId, string accountId)
        //{
        //    //method will return the particular insurance account detail baded in customer and account ID
        //    //will return one single insurance plan purchase details
        //    List<InsuranceAccountShortDto> insuranceAccounts = new List<InsuranceAccountShortDto>();
        //    var customer = await this._userManager.FindByIdAsync(customerId);
        //    if (customer == null)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
        //    }
        //    var account = this._insuranceAccountManager.GetAll().Find(x => x.CustomerId == customerId && x.AccountNumber == accountId);
        //    if (account == null)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Insurance Account Not Found", Status = "Error" });
        //    }
        //    return this.Ok(new InsuranceAccountShortDto()
        //    {
        //        // AccountNumber = account.Id,
        //        DateCreated = account.DateCreated,
        //        InsuranceScheme = account.InsuranceScheme,
        //        InsuranceType = account.InsuranceType,
        //        InvestmentAmount = account.InvestmentAmount,
        //        MaturityDate = account.MaturityDate,
        //        PremiumType = account.PremiumType,
        //        ProfitRatio = account.ProfitRatio,
        //        TotalAmount = account.TotalAmount,
        //        AccountNumber=account.AccountNumber
        //    });
        //}
        [HttpGet]
        [Route("{accountId}/GetInsuranceAccountByAccountId")]
        public async Task<IActionResult> GetInsuranceAccountByAccountId(string accountId)
        {
            //method will return the particular insurance account detail baded in customer and account ID
            //will return one single insurance plan purchase details
            List<InsuranceAccountShortDto> insuranceAccounts = new List<InsuranceAccountShortDto>();
            //var customer = await this._userManager.FindByIdAsync(customerId);
            //if (customer == null)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Customer Not Found", Status = "Error" });
            //}

            var account = this._insuranceAccountManager.GetAll().Find(x => x.AccountNumber == accountId);
            if (account == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Insurance Account Not Found", Status = "Error" });
            }
            return this.Ok(new InsuranceAccountShortDto()
            {
                // AccountNumber = account.Id,
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
                CustomerId=account.CustomerId
            });
        }
    }
}
