using Cocoon.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class ResendVcodeModel
    {
        [Required(ErrorMessageResourceName = "PhoneNumberFieldIsRequired", ErrorMessageResourceType = typeof(Website))]
        public string Provider { get; set; }
    }
}