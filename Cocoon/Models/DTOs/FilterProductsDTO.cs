using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class FilterProductsDTO
    {
        public long[] FilterValueIds { get; set; }
        public long StoreId { get; set; }
        public long CategoryId { get; set; }
    }
}