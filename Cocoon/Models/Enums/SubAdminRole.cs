using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Enums
{
    public enum SubAdminRole
    {
        [Display(Name = "الطلبات")]
        Orders,

        [Display(Name = "كوبونات الخصم")]
        PromoCodes,

        [Display(Name = "مستخدمي التطبيق")]
        Users,

        [Display(Name = "الاشعارات")]
        Notifications,
        [Display(Name = "جميع ما سبق")]
        All,

    }
}