using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class UploadImageDTO
    {
        public string OrderDescription { get; set; }
        public string ImageBase64 { get; set; }
    }
}