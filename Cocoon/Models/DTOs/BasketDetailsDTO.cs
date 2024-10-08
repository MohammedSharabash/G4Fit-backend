using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class BasketDetailsDTO
    {
        public string SubTotal { get; set; }
        public string Total { get; set; }
        public int UserStatus { get; set; }
        public List<BasketItemDTO> BasketItems { get; set; } = new List<BasketItemDTO>();
    }
}