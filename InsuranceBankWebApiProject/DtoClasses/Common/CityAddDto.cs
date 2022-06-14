using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Common
{
    public class CityAddDto
    {
        [Required(ErrorMessage = "City is Required")]
        public string CityName { get; set; }
        [Required(ErrorMessage = "Status is Required")]
        public string Status { get; set; }
    }
}
