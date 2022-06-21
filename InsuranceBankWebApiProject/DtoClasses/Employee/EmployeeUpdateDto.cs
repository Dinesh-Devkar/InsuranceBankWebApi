using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Employee
{
    public class EmployeeUpdateDto
    {
        [Required(ErrorMessage ="Id Is Required")]
        public string Id { get; set; }
        [Required(ErrorMessage = "Name Is Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "LoginId Is Required")]
        public string LoginId { get; set; }
        [Required(ErrorMessage = "Status Is Required")]
        public string UserStatus { get; set; }
        [Required(ErrorMessage = "Email Is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "UserRoll Is Required")]
        public string UserRoll { get; set; }
    }
}
