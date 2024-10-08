using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class CategoryStoreDTO
    {
        public string CategoryName { get; set; }
        public long CategoryId { get; set; }
        public List<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
    }
}