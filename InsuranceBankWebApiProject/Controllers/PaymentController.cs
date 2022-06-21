using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IAllRepository<InsuranceAccount> _insuranceAccountManager;
        private readonly IAllRepository<Payment> _paymentManager;
        public PaymentController(BankInsuranceDbContext insuranceDbContext)
        {
            this._paymentManager=new AllRepository<Payment>(insuranceDbContext);
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(insuranceDbContext);
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
    }
}
