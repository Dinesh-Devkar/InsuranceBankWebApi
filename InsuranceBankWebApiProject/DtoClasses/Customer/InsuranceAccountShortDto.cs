namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class InsuranceAccountShortDto
    {
        public string AccountNumber { get; set; }
        public string InsuranceType { get; set; }
        public string InsuranceScheme { get; set; }       
        public int ProfitRatio { get; set; }
        public double InvestmentAmount { get; set; }
        public string PremiumType { get; set; }      
        public double TotalAmount { get; set; }
        public string DateCreated { get; set; }
        public string MaturityDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public int NumberOfInstallments { get; set; }
        public int PendingInstallments { get; set; }
        public string PolicyStatus { get; set; }
    }
}
