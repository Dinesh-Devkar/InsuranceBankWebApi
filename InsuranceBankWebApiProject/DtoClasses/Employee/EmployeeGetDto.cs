using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Employee
{
    public class EmployeeGetDto
    {
        public string Id { get; set; }

        public string Name { get; set; }
   
        public string LoginId { get; set; }

        public string UserStatus { get; set; }
 
        public string Email { get; set; }

        public string UserRoll { get; set; }


    }
}
