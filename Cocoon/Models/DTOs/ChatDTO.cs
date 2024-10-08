using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class ChatDTO
    {
        public long ChatId { get; set; }
        public string OrderCode { get; set; }
        public string DriverPhoneNumber { get; set; }
        public long? DriverId { get; set; }
        public double? DriverLatitude { get; set; }
        public double? DriverLongitude { get; set; }
        public string ClientPhoneNumber { get; set; }
        public List<ChatMessageDTO> Messages { get; set; } = new List<ChatMessageDTO>();
        public bool CanChat { get; set; }
        public bool CanCall { get; set; }
        public bool CanChangeDriver { get;  set; }
        public bool CanCancelOrder { get;  set; }
        public bool CanDriverCancelOrder { get; set; }
        public bool CanLiveTrack { get;  set; }
        public long OfferId { get; set; }
        public long OrderId { get; set; }
    }
}