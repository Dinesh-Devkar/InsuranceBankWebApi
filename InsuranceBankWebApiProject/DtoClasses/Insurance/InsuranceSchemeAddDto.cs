using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsuranceSchemeAddDto
    {
        [Required(ErrorMessage ="InsuranceScheme Is Required")]
        public string InsuranceSchemeName { get; set; }
        [Required(ErrorMessage = "InsuranceType Is Required")]
        public string InsuranceTypeName { get; set; }
        [Required(ErrorMessage = "Status Is Required")]
        public string Status { get; set; }
        public IFormFile Image { get; set; }
        [Required(ErrorMessage = "Comission For New Registration Is Required")]
        public int NewRegComission { get; set; }
        [Required(ErrorMessage = "Comission For Installment Payment Is Required")]
        public int InstallmentComission { get; set; }
    }
}
