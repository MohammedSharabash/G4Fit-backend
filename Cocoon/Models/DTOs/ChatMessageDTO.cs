using Matgarkom.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class ChatMessageDTO
    {
        public bool IsMyMessage { get; set; }
        public string Message { get; set; }
        public bool IsLocation { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public bool IsAttachment { get; set; }
        public string AttachmentFileUrl { get; set; }
        public string AttachmentFileName { get; set; }
        public MediaType? AttachmentFileType { get; set; }
        public string ClientImageUrl { get; set; }
        public string DriverImageUrl { get; set; }
        public string Date { get; set; }
       
    }
}