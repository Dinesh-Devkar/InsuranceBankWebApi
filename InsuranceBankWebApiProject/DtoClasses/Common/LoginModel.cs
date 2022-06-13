using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Common
{
    public class LoginModel
    {
        
        public string Email { get; set; }
        
        public string Password { get; set; }
    }
}
