using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Common
{
    public class Subscriber
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public string  CurrentEndDate { get; set; }
    }
}
