using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Payment
{
    public class PaymentAddDto
    {
        [Required(ErrorMessage = "Installment Number Is Required")]
        public int InstallmentNumber { get; set; }
        [Required(ErrorMessage = "Installment Date Is Required")]
        public string InstallmentDate { get; set; }
        [Required(ErrorMessage = "Paid Date  Is Required")]
        public string PaidDate { get; set; }
        [Required(ErrorMessage = "Installment Amount Is Required")]
        public double InstallmentAmount { get; set; }
        [Required(ErrorMessage = "Customer  Id Number Is Required")]
        public string CustomerId { get; set; }
        [Required(ErrorMessage ="Insurance Account Number Is Required")]
        public string InsuranceAccountNumber { get; set; }
        [Required(ErrorMessage = "Customer Name Is Required")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Insurance Scheme Is Required")]
        public string InsuranceScheme { get; set; }
    }
}
