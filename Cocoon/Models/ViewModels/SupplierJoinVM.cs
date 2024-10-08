using G4Fit.Models.Enums;
using Cocoon.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class SupplierJoinVM
    {
        public SupplierType SupplierType { get; set; }
        [Required(ErrorMessageResourceName = "FirstNameFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string FirstName { get; set; }
        [Required(ErrorMessageResourceName = "LastNameFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string LastName { get; set; }
        [Required(ErrorMessageResourceName = "EmailFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        [EmailAddress(ErrorMessageResourceName = "EmailFieldIsInvalid", ErrorMessageResourceType = typeof(Website))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "PhoneNumberFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessageResourceName = "PasswordFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        [StringLength(6, MinimumLength = 6, ErrorMessageResourceType = typeof(Website), ErrorMessageResourceName = "PasswordFieldIMinimumLength")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "ConfirmPasswordFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceName = "PasswordDoesNotMatch", ErrorMessageResourceType = typeof(Website))]
        public string ConfirmPassword { get; set; }

        public string StoreName { get; set; }

        public long CountryId { get; set; }
    }
}