using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class BookAddDto
    {
        [Required(ErrorMessage = "InsuranceType Is Required")]
        public string InsuranceName { get; set; }
        public object Image { get; set; }
    }
}
