using EnsuranceProjectEntityLib.Model.CustomerModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.Common
{
    public class ApplicationUser : IdentityUser
    {
        public string? LoginId { get; set; }
        public string? UserStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public int? PinCode { get; set; }
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }
        public string? AgentId { get; set; }

        public List<CustomerDocument> Documents { get; set; }
    }
}
