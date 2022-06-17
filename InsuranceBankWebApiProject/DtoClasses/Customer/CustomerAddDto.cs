using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class CustomerAddDto
    {
        [Required(ErrorMessage = "Name Is Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date Of Birth Is Required")]
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "LoginId Is Required")]
        
        public string LoginId { get; set; }
        [Required(ErrorMessage = "Address Is Required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "State Is Required")]
        public string State { get; set; }
        [Required(ErrorMessage = "City Is Required")]
        public string City { get; set; }
        [Required(ErrorMessage = "Pin Code Is Required")]
        public int PinCode { get; set; }
        [Required(ErrorMessage = "Mobile Number Is Required")]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = "Nominee Is Required")]
        public string NomineeName { get; set; }
        [Required(ErrorMessage = "Email Is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password Is Required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "ConfirmPassword Is Required")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Nominee Relation Is Required")]
        public string NomineeRelation { get; set; }
        [Required(ErrorMessage ="Agent Code Is Required")]
        public string AgentCode { get; set; }
    }
}
