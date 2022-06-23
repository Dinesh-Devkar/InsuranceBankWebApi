using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.CustomerModel
{
    public class CustomerDocument
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentFile { get; set; }
        public string CustomerId { get; set; }
    }
}
