using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsuranceSchemeAddDto
    {
        [Required(ErrorMessage ="Id Is Required")]
        public int Id { get; set; }
        [Required(ErrorMessage ="InsuranceScheme Is Required")]
        public string InsuranceSchemeName { get; set; }
        [Required(ErrorMessage = "InsuranceType Is Required")]
        public string InsuranceTypeName { get; set; }
        [Required(ErrorMessage = "Status Is Required")]
        public string Status { get; set; }
        public string Image { get; set; }
        [Required(ErrorMessage = "Comission For New Registration Is Required")]
        public int NewRegComission { get; set; }
        [Required(ErrorMessage = "Comission For Installment Payment Is Required")]
        public int InstallmentComission { get; set; }
        [Required(ErrorMessage = "Note Is Required")]
        public string Note { get; set; }
    }
}
