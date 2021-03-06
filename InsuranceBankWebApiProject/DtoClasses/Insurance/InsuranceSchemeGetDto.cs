using System.Drawing;

namespace InsuranceBankWebApiProject.DtoClasses.Insurance
{
    public class InsuranceSchemeGetDto
    {

        public string InsuranceSchemeName { get; set; }
        public string InsuranceTypeName { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public int NewRegComission { get; set; }
        public int InstallmentComission { get; set; }
        public string Note { get; set; }
        public int Id { get; set; }
    }
}
