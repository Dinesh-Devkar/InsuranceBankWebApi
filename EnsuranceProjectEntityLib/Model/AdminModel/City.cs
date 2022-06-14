using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectEntityLib.Model.AdminModel
{
    public class City
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="City is Required")]
        public string CityName { get; set; }
        [Required(ErrorMessage = "Status is Required")]
        public string Status { get; set; }
        [Required(ErrorMessage = "State is Required")]
        public string State { get; set; }
    }
}
