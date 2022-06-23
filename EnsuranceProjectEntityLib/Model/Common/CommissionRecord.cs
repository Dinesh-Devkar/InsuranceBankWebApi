using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Common
{
    public class CommissionRecord
    {
        public int Id { get; set; }
        public string InsuranceAccountId { get; set; }
        public string InsuranceScheme { get; set; }
        public string   CustomerName { get; set; }
        public string? AgentName { get; set; }
        public string PurchasedDate { get; set; }
        public double CommissionAmount { get; set; }
        public string AgentCode { get; set; }
        public string CustomerId { get; set; }
    }
}
