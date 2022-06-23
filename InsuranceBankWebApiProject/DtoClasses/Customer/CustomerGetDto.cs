using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class CustomerGetDto
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }

        public string State { get; set; }
        
        public string City { get; set; }
        
        public string PinCode { get; set; }
        public string DateOfBirth { get; set; }
        
        public string LoginId { get; set; }
        
        public string Address { get; set; }
        
        public string Email { get; set; }
        
        public string MobileNumber { get; set; }
        
        public string NomineeName { get; set; }
        
        public string NomineeRelation { get; set; }
        public string Status { get; set; }
    }
}
