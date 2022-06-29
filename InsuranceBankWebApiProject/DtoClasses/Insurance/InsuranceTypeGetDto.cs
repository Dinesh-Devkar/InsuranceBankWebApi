using System.Drawing;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsuranceTypeGetDto
    {

        public int Id { get; set; }
        public string InsuranceName { get; set; }
        public string Image { get; set; }
        
        public string Status { get; set; }
    }
}
