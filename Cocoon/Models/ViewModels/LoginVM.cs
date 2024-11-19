using Cocoon.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace G4Fit.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessageResourceName = "PhoneNumberFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string Provider { get; set; } //Email or Phone Number

        [Required(ErrorMessageResourceName = "PasswordFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        //[StringLength(6, MinimumLength = 6, ErrorMessageResourceType = typeof(Website), ErrorMessageResourceName = "PasswordFieldIMinimumLength")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
