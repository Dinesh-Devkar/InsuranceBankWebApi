using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Common
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage ="Old Passwrod Required")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "New Passwrod Required")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm Passwrod Required")]
        public string ConfirmNewPassword { get; set; }
    }
}
