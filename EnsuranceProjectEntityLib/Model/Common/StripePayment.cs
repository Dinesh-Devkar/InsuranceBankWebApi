using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Common
{
    public class StripePayment
    {
        public int Id { get; set; }
        public string StripeCustomerId { get; set; }
        public string StripePaymentId { get; set; }
    }
}
