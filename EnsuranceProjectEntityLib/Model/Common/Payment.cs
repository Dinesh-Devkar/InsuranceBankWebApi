using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Common
{
    public class Payment
    {
        public int Id { get; set; }
        public int InstallmentNumber { get; set; }
        public string InstallmentDate { get; set; }
        public double InstallmentAmount { get; set; }
        public string PaidDate { get; set; }
        public string PaymentStatus { get; set; }
        public string CustomerId { get; set; }
        public string InsuranceAccountNumber { get; set; }
    }
}
