using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.CustomerModel
{
    public class InsuranceAccount
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public string InsuranceType { get; set; }
        public string InsuranceScheme { get; set; }
        public int NumberOfYears { get; set; }          
        public int ProfitRatio { get; set; }
        public double InvestmentAmount { get; set; }
        public string PremiumType { get; set; }
        public double InstallmentAmount { get; set; }
        public double InterestAmount { get; set; }
        public double TotalAmount { get; set; }
        public string DateCreated { get; set; }
        public string MaturityDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public string AgentCode { get; set; }
        public int NumberOfInstallments { get; set; }
        public int PendingInstallments { get; set; }
        public string IsPolicyClaimed { get; set; }
        public string PolicyStatus { get; set; }
    }
}
