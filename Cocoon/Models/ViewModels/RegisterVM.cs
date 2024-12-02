using Cocoon.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class RegisterVM
    {
        public HttpPostedFileBase Image { get; set; }
        [Required(ErrorMessageResourceName = "NameFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "PhoneNumberFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string PhoneNumber { get; set; }
        //[Required(ErrorMessageResourceName = "IDNumberFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string IDNumber { get; set; }
        [Required(ErrorMessageResourceName = "AddressFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string Address { get; set; }
        [Required(ErrorMessageResourceName = "EmailFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        [EmailAddress]
        public string Email { get; set; }

        //[Required(ErrorMessageResourceName = "PasswordFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        //[StringLength(6, MinimumLength = 6, ErrorMessageResourceType = typeof(Website), ErrorMessageResourceName = "PasswordFieldIMinimumLength")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "ConfirmPasswordFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceName = "PasswordDoesNotMatch", ErrorMessageResourceType = typeof(Website))]
        public string ConfirmPassword { get; set; }

        public long CountryId { get; set; } = 1;
    }
}