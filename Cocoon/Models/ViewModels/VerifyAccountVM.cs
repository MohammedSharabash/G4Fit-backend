using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class VerifyAccountVM
    {
        public int Vcode1 { get; set; }
        public int Vcode2 { get; set; }
        public int Vcode3 { get; set; }
        public int Vcode4 { get; set; }
        public string Provider { get; set; }
        public string Password { get; set; }
    }
}