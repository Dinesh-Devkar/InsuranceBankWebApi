using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.AdminModel
{
    public class State
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "State is Required")]
        public string StateName { get; set; }
        [Required(ErrorMessage = "Status is Required")]
        public string Status { get; set; }
    }
}
