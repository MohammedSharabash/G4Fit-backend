using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class ServiceDetailsDTO
    {
        public long Id { get; set; }
        public string ServiceName { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool HasDiscount { get; set; }
        public bool IsTimeBoundService { get; set; }
        public int ServiceDays { get; set; }
        public string Price { get; set; }
        public string PriceAfter { get; set; }
        public List<ServiceColorDTO> Trainers { get; set; } = new List<ServiceColorDTO>();
        public List<ServiceSizeDTO> Sizes { get; set; } = new List<ServiceSizeDTO>();
        public List<string> Images { get; set; } = new List<string>();
    }
}