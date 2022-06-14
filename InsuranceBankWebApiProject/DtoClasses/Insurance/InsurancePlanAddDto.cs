using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsurancePlanAddDto
    {
        [Required(ErrorMessage ="InsuranceType Is Required")]
        public string InsuranceType { get; set; }
        [Required(ErrorMessage = "InsuranceScheme Is Required")]
        public string InsuranceScheme { get; set; }
        [Required(ErrorMessage = "InsurancePlan Is Required")]
        public string InsurancePlanName { get; set; }
        [Required(ErrorMessage = "Minimum Year Is Required")]
        public int MinimumYears { get; set; }
        [Required(ErrorMessage = "Maximun Is Required")]
        public int MaximumYears { get; set; }
        [Required(ErrorMessage = "Minimum Age Is Requireds")]
        public int MinimumAge { get; set; }
        [Required(ErrorMessage = "Maximum Year Is Required")]
        public int MaximumAge { get; set; }
        [Required(ErrorMessage = "Minimum Investment Amount Is Required")]
        public int MinimumInvestAmt { get; set; }
        [Required(ErrorMessage = "Maximum Investment Amount Is Required")]
        public int MaximumInvestAmt { get; set; }
        [Required(ErrorMessage = "Profit Ratio Is Required")]
        public int ProfitRatio { get; set; }
        [Required(ErrorMessage = "Status Is Required")]
        public string Status { get; set; }
    }
}
