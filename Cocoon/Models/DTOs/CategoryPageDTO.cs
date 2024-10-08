using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class CategoryPageDTO
    {
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public List<SubCategoryDTO> SubCategories { get; set; } = new List<SubCategoryDTO>();
    }
}