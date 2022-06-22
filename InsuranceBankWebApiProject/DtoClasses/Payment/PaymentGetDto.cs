namespace InsuranceBankWebApiProject.DtoClasses.Payment
{
    public class PaymentGetDto
    {
        public int InstallmentNumber { get; set; }
        public string InstallmentDate { get; set; }
        public string PaidDate { get; set; }
        public string PaymentStatus { get; set; }
        public double InstallmentAmount { get; set; }
    }
}
