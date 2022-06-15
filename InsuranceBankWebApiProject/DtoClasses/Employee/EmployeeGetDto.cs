using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Employee
{
    public class EmployeeGetDto
    {
        [Required(ErrorMessage = "Name Is Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "LoginId Is Required")]
        public string LoginId { get; set; }
        [Required(ErrorMessage = "User Status Is Required")]
        public string UserStatus { get; set; }
        [Required(ErrorMessage = "Email Is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "User Roll Is Required")]
        public string UserRoll { get; set; }

    }
}
