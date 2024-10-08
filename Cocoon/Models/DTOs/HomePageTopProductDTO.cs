using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class HomePageTopProductDTO
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string AdvertisementImage { get; set; }
        public string AdvertisementLink { get; set; }
        public List<ProductDTO> Products { get; set; } = new List<ProductDTO>();
    }
}