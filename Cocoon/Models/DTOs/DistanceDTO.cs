using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class DistanceMatrixDTO
    {
        [JsonProperty("destination_addresses")]
        public List<string> destination_addresses { get; set; }

        [JsonProperty("origin_addresses")]
        public List<string> origin_addresses { get; set; }

        [JsonProperty("rows")]
        public List<Row> rows { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }
    }
    public class DistanceObject
    {

        [JsonProperty("text")]
        public string text { get; set; }

        [JsonProperty("value")]
        public int value { get; set; }
    }

    public class Duration
    {

        [JsonProperty("text")]
        public string text { get; set; }

        [JsonProperty("value")]
        public int value { get; set; }
    }

    public class Element
    {

        [JsonProperty("distance")]
        public DistanceObject distance { get; set; }

        [JsonProperty("duration")]
        public Duration duration { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }
    }

    public class Row
    {
        [JsonProperty("elements")]
        public List<Element> elements { get; set; }
    }
}