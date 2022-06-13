using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class CustomerAddDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string LoginId { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public int PinCode { get; set; }
        [Required]
        public int MobileNumber { get; set; }
        [Required]
        public string NomineeName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string NomineeRelation { get; set; }
    }
}
