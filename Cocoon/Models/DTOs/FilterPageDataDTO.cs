using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class FilterPageDataDTO
    {
        public List<FilterDTO> Filters = new List<FilterDTO>();
        public decimal MinimumPrice { get; set; }
        public decimal MaximumPrice { get; set; }
    }

    public class FilterDTO
    {
        public string FilterName { get; set; }
        public List<FilterValueDTO> FilterValues { get; set; } = new List<FilterValueDTO>();
    }
    public class FilterValueDTO
    {
        public long ValueId { get; set; }
        public string ValueName { get; set; }
    }
}