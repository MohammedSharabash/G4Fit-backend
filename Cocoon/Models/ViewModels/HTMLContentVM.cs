using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Models.ViewModels
{
    public class HTMLContentVM
    {
        [AllowHtml]
        public string ContentAr { get; set; }
        [AllowHtml]
        public string ContentEn { get; set; }
    }
}