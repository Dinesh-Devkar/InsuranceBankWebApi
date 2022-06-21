namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class InsuranceAccountGetDto
    {
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
        public string AccountNumber { get; set; }
    }
}
