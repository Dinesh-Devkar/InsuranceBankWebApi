using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IAllRepository<InsuranceAccount> _insuranceAccountManager;
        private readonly IAllRepository<Payment> _paymentManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public PaymentController(BankInsuranceDbContext insuranceDbContext,UserManager<ApplicationUser> userManager)
        {
            this._paymentManager=new AllRepository<Payment>(insuranceDbContext);
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(insuranceDbContext);
            this._userManager = userManager;
        }

        [HttpGet]
        [Route("{accountNumber}/GetPaymentDetails")]
        public async Task<IActionResult> GetPaymentDetails(string accountNumber)
        {
            var account=this._insuranceAccountManager.GetAll().Find(x=>x.AccountNumber==accountNumber);
            if (account == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Insurance Account Found With Given Id", Status = "Error" });
            }
            List<PaymentGetDto> paymentsList=new List<PaymentGetDto>();
            var payments=this._paymentManager.GetAll().Where(x=>x.InsuranceAccountNumber==accountNumber).ToList();
            foreach(var payment in payments)
            {
                paymentsList.Add(new PaymentGetDto()
                {
                    InstallmentAmount = payment.InstallmentAmount,
                    InstallmentDate = payment.InstallmentDate,
                    InstallmentNumber = payment.InstallmentNumber,
                    PaidDate = payment.PaidDate,
                    PaymentStatus = payment.PaymentStatus
                });
            }
            return this.Ok(paymentsList);
        }

        [HttpGet]
        [Route("GetAllPaymentDetails")]
        public async Task<IActionResult> GetAllPaymentDetails()
        {
           

            List<PaymentGetAllDto> paymentsList = new List<PaymentGetAllDto>();
            var payments = this._paymentManager.GetAll();
            foreach (var payment in payments)
            {
                var agentCode = await this._userManager.Users.Where(x => x.Id == payment.CustomerId).Select(x => x.AgentCode).FirstOrDefaultAsync();
                var agentName = await this._userManager.Users.Where(x => x.AgentCode == agentCode && x.UserRoll==UserRoles.Agent).Select(x => x.UserName).FirstOrDefaultAsync();
                paymentsList.Add(new PaymentGetAllDto()
                {
                    InstallmentAmount = payment.InstallmentAmount,
                    InstallmentDate = payment.InstallmentDate,
                    InstallmentNumber = payment.InstallmentNumber,
                    PaidDate = payment.PaidDate,
                    PaymentStatus = payment.PaymentStatus,
                    AccountNumber=payment.InsuranceAccountNumber,
                    AgentName=agentName,
                    CustomerName=await this._userManager.Users.Where(x=>x.Id==payment.CustomerId).Select(x=>x.UserName).FirstOrDefaultAsync(),
                    InsuranceSchemeName=this._insuranceAccountManager.GetAll().Where(x=>x.AccountNumber==payment.InsuranceAccountNumber).Select(x=>x.InsuranceScheme).FirstOrDefault()
                    
                }) ;
            }
            return this.Ok(paymentsList);
        }
    }
}
