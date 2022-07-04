//using Newtonsoft.Json;

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Payment
{
    public class CreateCheckoutSessionRequest
    {
        [JsonProperty("priceId")]      
        [Required]
        public string PriceId { get; set; }
        [Required]
        public string SuccessUrl { get; set; }
        [Required]
        public string FailureUrl { get; set; }
    }
}
