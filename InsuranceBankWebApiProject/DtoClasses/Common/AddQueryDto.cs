using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Common
{
    public class AddQueryDto
    {
        [Required(ErrorMessage ="CustomerID Is Required")]
        public string CustomerId { get; set; }
        [Required(ErrorMessage = "Title Is Required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Message Is Required")]
        public string Message { get; set; }
    }
}
