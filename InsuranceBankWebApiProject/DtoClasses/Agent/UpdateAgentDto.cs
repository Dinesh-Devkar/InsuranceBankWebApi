using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Agent
{
    public class UpdateAgentDto
    {
        [Required(ErrorMessage ="Name Is Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "LoginId Is Required")]
        public string LoginId { get; set; }
        [Required(ErrorMessage = "Qualification Is Required")]
        public string Qualification { get; set; }
        [Required(ErrorMessage = "Address Is Required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "AgentCode Is Required")]
        public int AgentCode { get; set; }
        [Required(ErrorMessage = "Email Is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Status Is Required")]
        public string Status { get; set; }
    }
}
