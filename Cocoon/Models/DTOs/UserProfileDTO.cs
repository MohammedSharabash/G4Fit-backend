using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class UserProfileDTO
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberCode { get; set; }
        public long? CountryId { get; set; }
    }
}