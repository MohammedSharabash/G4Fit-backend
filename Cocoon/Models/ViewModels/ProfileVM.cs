using Cocoon.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class ProfileVM
    {
        public HttpPostedFileBase Image { get; set; }
        [Required(ErrorMessageResourceName = "NameFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "PhoneNumberFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string PhoneNumber { get; set; }
        //[Required(ErrorMessageResourceName = "IDNumberFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        // public string IDNumber { get; set; }
        [Required(ErrorMessageResourceName = "AddressFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string Address { get; set; }
        [Required(ErrorMessageResourceName = "EmailFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        [EmailAddress]
        public string Email { get; set; }

        public long? CountryId { get; set; }
        public string ImageUrl { get; set; }
        public double? weight { get; set; }
        public double? length { get; set; }
    }
}