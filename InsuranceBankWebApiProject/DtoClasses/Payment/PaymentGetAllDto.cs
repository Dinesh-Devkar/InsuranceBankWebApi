namespace InsuranceBankWebApiProject.DtoClasses.Payment
{
    public class PaymentGetAllDto
    {
        public int InstallmentNumber { get; set; }
        public string InstallmentDate { get; set; }
        public string PaidDate { get; set; }
        public string PaymentStatus { get; set; }
        public double InstallmentAmount { get; set; }
        public string CustomerName { get; set; }
        public string AgentName { get; set; }
        public string InsuranceSchemeName { get; set; }
        public string AccountNumber { get; set; }
    }
}
