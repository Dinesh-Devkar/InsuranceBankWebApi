﻿using System.ComponentModel.DataAnnotations;

namespace InsuranceBankWebApiProject.DtoClasses.Payment
{
    public class CustomerPortalRequest
    {
        [Required]
        public string ReturnUrl { get; set; }
    }
}
