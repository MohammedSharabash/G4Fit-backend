using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class UserAddressDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Details { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDefault { get; set; }
    }
}