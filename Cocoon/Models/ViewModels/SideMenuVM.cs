using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class SideMenuVM
    {
        public string Name { get; set; }
        public string UserImage { get; set; }
        public string TagName { get; set; }
        public SubAdminRole Role { get; set; }
    }
}