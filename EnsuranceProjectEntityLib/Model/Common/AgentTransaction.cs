using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Common
{
    public class AgentTransaction
    {
        public int Id { get; set; }
        public double  Amount { get; set; }
        public string AgentId { get; set; }

        public string TransactionDate { get; set; }
        public string TransactionType { get; set; }
    }
}
