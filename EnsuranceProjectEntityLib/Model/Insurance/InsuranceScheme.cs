using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Insurance
{
    public class InsuranceScheme
    {
        public int Id { get; set; }
        public string InsuranceSchemeName { get; set; }
        public string InsuranceTypeName { get; set; }
        public string Status { get; set; }
        public Byte[] Image { get; set; }
        public int NewRegComission { get; set; }
        public int InstallmentComission { get; set; }
    }
}
