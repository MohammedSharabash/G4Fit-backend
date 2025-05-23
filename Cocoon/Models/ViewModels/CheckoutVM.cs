﻿using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class CheckoutVM
    {
        public PaymentMethod PaymentMethod { get; set; }
        public long CityId { get; set; } = 1;
        public string Address { get; set; }
    }
}