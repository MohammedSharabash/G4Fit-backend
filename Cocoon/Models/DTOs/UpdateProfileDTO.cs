using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class UpdateProfileDTO
    {
        public string ImageBase64 { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public long CountryId { get; set; }
    }
}