using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsuranceTypeAddDto
    {
        [Required(ErrorMessage ="InsuranceType Is Required")]
        public string InsuranceName { get; set; }
        public IFormFile Image { get; set; }
        [Required(ErrorMessage = "Status Is Required")]
        public string Status { get; set; }
    }
}
