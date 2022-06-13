using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.CustomerModel
{
    public class Customer:IdentityUser
    {

        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string LoginId { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public int PinCode { get; set; }
        [Required]
        public string NomineeName { get; set; }
        [Required]
        public string NomineeRelation { get; set; }
        [Required]
        public string Password { get; set; }
        public string AgentId { get; set; }

        public List<CustomerDocument> Documents { get; set; }
    }
}
