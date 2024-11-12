using Cocoon.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "Cash", ResourceType = typeof(Website))]
        Cash,
        [Display(Name = "Online", ResourceType = typeof(Website))]
        UrWay,
        [Display(Name = "Tabby", ResourceType = typeof(Website))]
        Tabby
    }
}