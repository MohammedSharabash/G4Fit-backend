using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.ViewModels
{
    public class ChangePasswordVM
    {
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمه السر يجب ان لا تقل عن 6 أحرف")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمه السر يجب ان لا تقل عن 6 أحرف")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمة السر غير متطابقة")]
        public string ConfirmPassword { get; set; }
    }
}