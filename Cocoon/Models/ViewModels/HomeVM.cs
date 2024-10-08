using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class HomeVM
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Slider> Sliders { get; set; } = new List<Slider>();
        public List<Service> LatestServices { get; set; } = new List<Service>();
        public List<Service> MostSoldServices { get; set; } = new List<Service>();
        public List<Service> OffersServices { get; set; } = new List<Service>();
        public CompanyData About { get; set; }
    }
}