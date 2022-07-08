using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsurancePlanGetDto
    {
        public int Id { get; set; }
        public string InsuranceType { get; set; }
       
        public string InsuranceScheme { get; set; }
        
        public string InsurancePlanName { get; set; }
        
        public int MinimumYears { get; set; }
        
        public int MaximumYears { get; set; }
        
        public int MinimumAge { get; set; }
        
        public int MaximumAge { get; set; }
        
        public int MinimumInvestAmt { get; set; }
        
        public int MaximumInvestAmt { get; set; }
        
        public int ProfitRatio { get; set; }
        
        public string Status { get; set; }
        public string ProductId { get; set; }
        public string PriceId { get; set; }
    }
}
