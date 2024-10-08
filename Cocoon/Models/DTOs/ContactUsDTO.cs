using Matgarkom.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class ContactUsDTO
    {
        public long? OrderId { get; set; }
        public OrderType? OrderType { get; set; }
        public string Message { get; set; }
    }
}