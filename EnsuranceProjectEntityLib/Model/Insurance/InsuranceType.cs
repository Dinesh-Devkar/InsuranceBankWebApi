using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Insurance
{
    public class InsuranceType
    {
        public int Id { get; set; }
        public string InsuranceName { get; set; }
        public string Image { get; set; }
        public string Status { get; set; }
    }
}
