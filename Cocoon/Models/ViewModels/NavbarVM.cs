using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class NavbarVM
    {
        public int NotificationsCount { get; set; }
        public string UserImage { get; set; }
        public int ShoppingCartCount { get; set; }
        public string Username { get; set; }
        public string UserPhoneNumber { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}