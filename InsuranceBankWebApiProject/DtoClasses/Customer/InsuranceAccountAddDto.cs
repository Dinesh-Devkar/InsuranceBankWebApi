using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class InsuranceAccountAddDto
    {
        [Required(ErrorMessage ="InsuranceType Is Required")]
        public string InsuranceType { get; set; }
        [Required(ErrorMessage = "InsuranceScheme Is Required")]
        public string InsuranceScheme { get; set; }
        [Required(ErrorMessage = "Number Of Years Is Required")]
        public int NumberOfYears { get; set; }
        [Required(ErrorMessage = "Profit Ratio Is Required")]
        public int ProfitRatio { get; set; }
        [Required(ErrorMessage = "Investment Amount Is Required")]
        public double InvestmentAmount { get; set; }
        [Required(ErrorMessage = "PremiumType Is Required")]
        public string PremiumType { get; set; }
        [Required(ErrorMessage = "Installment Amount Is Required")]
        public double InstallmentAmount { get; set; }
        [Required(ErrorMessage = "Interest Amount Is Required")]
        public double InterestAmount { get; set; }
        [Required(ErrorMessage = "Total Amount Is Required")]
        public double TotalAmount { get; set; }
        [Required(ErrorMessage = "Date Created Is Required Is Required")]
        public string DateCreated { get; set; }
        [Required(ErrorMessage = "Maturity Date Is Required")]
        public string MaturityDate { get; set; }
       
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "CustomerId Is Required")]
        public string CustomerId { get; set; }
        
        public int AgentCode { get; set; }
    }
}
