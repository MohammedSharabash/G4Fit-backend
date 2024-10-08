using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class ServicesVM
    {
        [Required(ErrorMessage = "ملف ألخدمات الجداد مطلوب")]
        public HttpPostedFileBase file { get; set; }
    }
}