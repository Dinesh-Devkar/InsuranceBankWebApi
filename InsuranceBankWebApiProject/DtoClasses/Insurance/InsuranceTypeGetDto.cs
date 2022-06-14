using System.Drawing;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsuranceTypeGetDto
    {
        
        public string InsuranceName { get; set; }
        public Bitmap Image { get; set; }
        
        public string Status { get; set; }
    }
}
