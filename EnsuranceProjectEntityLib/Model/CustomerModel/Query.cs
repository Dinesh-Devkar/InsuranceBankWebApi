using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.CustomerModel
{
    public class Query
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Reply { get; set; }
        public string ContactDate { get; set; }
        public string Status { get; set; }
    }
}
