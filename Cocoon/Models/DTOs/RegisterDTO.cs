using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class RegisterDTO
    {
        public string ImageBase64 { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public long CountryId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string IDNumber { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}