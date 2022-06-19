namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class CommissionRecordGetDto
    {
        public int InsuranceAccountId { get; set; }
        public string InsuranceScheme { get; set; }
        public string CustomerName { get; set; }
        public string AgentName { get; set; }
        public string PurchasedDate { get; set; }
        public double CommissionAmount { get; set; }
    }
}
