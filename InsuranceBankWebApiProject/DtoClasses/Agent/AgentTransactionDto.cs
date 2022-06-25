using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Agent
{
    public class AgentTransactionDto
    {
        [Required(ErrorMessage ="Agent Id Is Required")]
        public string AgentId { get; set; }
        [Required(ErrorMessage = "Amount Is Required")]
        public double Amount { get; set; }
    }
}
