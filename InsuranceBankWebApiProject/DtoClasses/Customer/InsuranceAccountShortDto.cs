namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class InsuranceAccountShortDto
    {
        public int AccountNumber { get; set; }
        public string InsuranceType { get; set; }
        public string InsuranceScheme { get; set; }       
        public int ProfitRatio { get; set; }
        public double InvestmentAmount { get; set; }
        public string PremiumType { get; set; }      
        public double TotalAmount { get; set; }
        public string DateCreated { get; set; }
        public string MaturityDate { get; set; }
    }
}
