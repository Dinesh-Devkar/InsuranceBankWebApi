using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IAllRepository<InsuranceAccount> _insuranceAccountManager;
        private readonly IAllRepository<Payment> _paymentManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StripeSettings _stripeSettings;
        private readonly IAllRepository<Subscriber> _subscriberManager;
        private readonly IAllRepository<StripePayment> _stripePaymentManager;
        public PaymentController(BankInsuranceDbContext insuranceDbContext,UserManager<ApplicationUser> userManager, IOptions<StripeSettings> stripeSettings)
        {
            this._paymentManager=new AllRepository<Payment>(insuranceDbContext);
            this._insuranceAccountManager=new AllRepository<InsuranceAccount>(insuranceDbContext);
            this._userManager = userManager;
            _stripeSettings = stripeSettings.Value;
            this._subscriberManager=new AllRepository<Subscriber>(insuranceDbContext);
            this._stripePaymentManager = new AllRepository<StripePayment>(insuranceDbContext);
            //StripeConfiguration.ApiKey = "sk_test_51LHTG0SBgKTZYyeaxtvjqK7VCYUkHhXom6lm5mHCzF2Hd6CbZqO2uHDXcRlnt2ruBxFNjKSvt0HnRN0nAAGtcSpB00gVhmWqrV";
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

        [HttpGet]
        [Route("GetProducts")]
        public async Task<IActionResult> GetProducts()
        {
            //StripeConfiguration.ApiKey = "sk_test_51LHTG0SBgKTZYyeaxtvjqK7VCYUkHhXom6lm5mHCzF2Hd6CbZqO2uHDXcRlnt2ruBxFNjKSvt0HnRN0nAAGtcSpB00gVhmWqrV";

            var options = new ProductListOptions
            {
                Limit = 3,
            };
            var service = new ProductService();
            StripeList<Product> products = service.List(
              options);

            return this.Ok(products);
        }
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest req)
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = req.SuccessUrl, // "http://localhost:4200/paymentsuccess",
                CancelUrl = req.FailureUrl, //  "http://localhost:4200/paymentfailure",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = req.PriceId,
                        Quantity = 1,
                    },
                },
            };

            var service = new SessionService();
            service.Create(options);
            try
            {
                var session = await service.CreateAsync(options);
                return Ok(new CreateCheckoutSessionResponse
                {
                    SessionId = session.Id,
                    PublicKey = _stripeSettings.PublicKey
                });
            }
            catch (StripeException e)
            {
                Console.WriteLine(e.StripeError.Message);
                return BadRequest(new ErrorResponse
                {
                    ErrorMessage = new ErrorMessage
                    {
                        Message = e.StripeError.Message,
                    }
                });
            }
        }

        //**WARNING** You want to protect this api and you want to get the customerId from the database.
        //we will take care of this in the next video
        [Authorize]
        [HttpPost("customer-portal")]
        public async Task<IActionResult> CustomerPortal([FromBody] CustomerPortalRequest req)
        {
           // ClaimsPrincipal principal = HttpContext.User as ClaimsPrincipal;
            //ApplicationUser principal = HttpContext.User. as ApplicationUser;
            //var claim = principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
            var userFromDb = await _userManager.FindByIdAsync(req.CustomerId);

            if (userFromDb == null)
            {
                return BadRequest();
            }

            try
            {
                var options = new Stripe.BillingPortal.SessionCreateOptions
                {
                    //Customer = "cus_LzkBvUpcsuAApY",
                    Customer = userFromDb.CustomerStripeId,
                    ReturnUrl = req.ReturnUrl,
                };
                var service = new Stripe.BillingPortal.SessionService();
                var session = await service.CreateAsync(options);

                return Ok(new
                {
                    url = session.Url
                });
            }
            catch (StripeException e)
            {
                Console.WriteLine(e.StripeError.Message);
                return BadRequest(new ErrorResponse
                {
                    ErrorMessage = new ErrorMessage
                    {
                        Message = e.StripeError.Message,
                    }
                });
            }

        }
        [HttpPost]
        [Route("Webhook")]
        public async Task<IActionResult> WebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                 json,
                 Request.Headers["Stripe-Signature"],
                 _stripeSettings.WHSecret
               );

                // Handle the event
                if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
                {
                    var subscription = stripeEvent.Data.Object as Subscription;
                    //Do stuff
                    
                    await AddSubscriptionToDb(subscription);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
                {
                    var session = stripeEvent.Data.Object as Stripe.Subscription;

                    // Update Subsription
                    await UpdateSubscription(session);
                }
                else if (stripeEvent.Type == Events.CustomerCreated)
                {
                    var customer = stripeEvent.Data.Object as Stripe.Customer;
                    //Do Stuff
                    await AddCustomerIdToUser(customer);
                }
               
                else if (stripeEvent.Type == Events.PaymentIntentCreated)
                {
                    var payment = stripeEvent.Data.Object as Stripe.PaymentIntent;
                    await AddPaymentToDb(payment);
                }
                else
                {
                    // Unexpected event type
                    Debug.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                Debug.WriteLine(e.StripeError.Message);
                return BadRequest();
            }
        }

        private async Task AddCustomerIdToUser(Stripe.Customer customer)
        {
            try
            {
                var userFromDb = await _userManager.FindByEmailAsync(customer.Email);

                if (userFromDb != null)
                {
                    
                    userFromDb.CustomerStripeId = customer.Id;
                    await _userManager.UpdateAsync(userFromDb);
                    Debug.WriteLine("Customer Id added to user ");
                }

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Unable to add customer id to user");
                Debug.WriteLine(ex);
            }
        }

        private async Task AddSubscriptionToDb(Subscription subscription)
        {
           try
            {
                var subscriber = new Subscriber
                {
                    SubscriptionId=subscription.Id,
                    CustomerId = subscription.CustomerId,
                    Status = "Active",
                    CurrentEndDate = subscription.CurrentPeriodEnd.ToShortDateString()
                };
                await this._subscriberManager.Add(subscriber);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Unable to add new subscriber to Database");
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task AddPaymentToDb(PaymentIntent payment)
        {
            try
            {
                var stripePayment = new StripePayment()
                {
                    StripeCustomerId = payment.CustomerId,
                    StripePaymentId = payment.Id
                };
                await this._stripePaymentManager.Add(stripePayment);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Unable to add new subscriber to Database");
                Debug.WriteLine(ex.Message);
            }
        }
        private async Task UpdateSubscription(Subscription subscription)
        {
           
            try
            {
                var subscriptionFromDb =  this._subscriberManager.GetAll().Where(x=>x.SubscriptionId==subscription.Id).FirstOrDefault();
                if (subscriptionFromDb != null)
                {
                    subscriptionFromDb.Status = subscription.Status;
                    subscriptionFromDb.CurrentEndDate = subscription.CurrentPeriodEnd.ToShortDateString();
                    await this._subscriberManager.Update(subscriptionFromDb);
                   // Console.WriteLine("Subscription Updated");
                }

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);

                Debug.WriteLine("Unable to update subscription");

            }

        }

    }
}
