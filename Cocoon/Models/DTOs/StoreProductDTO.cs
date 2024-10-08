using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class StoreProductDTO
    {
        public string StoreName { get; set; }
        public long StoreId { get; set; }
        public long CategoryId { get; set; }
        public List<ProductDTO> Products { get; set; } = new List<ProductDTO>();
    }
}