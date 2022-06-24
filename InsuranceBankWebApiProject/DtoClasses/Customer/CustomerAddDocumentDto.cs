using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Customer
{
    public class CustomerAddDocumentDto
    {
        [Required(ErrorMessage ="Document Name Is Required")]
        public string DocumentName { get; set; }
        [Required(ErrorMessage = "Document File Is Required")]
        public string DocumentFile { get; set; }
    }
}
