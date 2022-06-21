using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Common
{
    public class SolveQueryDto
    {
        public string CustomerName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        [Required(ErrorMessage ="Reply Is Required")]
        public string Reply { get; set; }
        public string ContactDate { get; set; }
        public string CustomerId { get; set; }
        public int QueryId { get; set; }
    }
}
