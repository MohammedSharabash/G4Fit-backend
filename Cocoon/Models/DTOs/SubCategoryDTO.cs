using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class SubCategoryDTO
    {
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}