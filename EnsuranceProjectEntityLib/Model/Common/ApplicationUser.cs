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
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public int? PinCode { get; set; }
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }
        public int? AgentCode { get; set; }
        public string? UserRoll { get; set; }
        public string? Qualification { get; set; }
        public double? Balance { get; set; }

        public List<CustomerDocument> Documents { get; set; }
    }
}
