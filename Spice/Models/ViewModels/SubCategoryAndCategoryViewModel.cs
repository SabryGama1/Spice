using System.Collections.Generic;

namespace Spice.Models.ViewModels
{
    public class SubCategoryAndCategoryViewModel
    {
        public IEnumerable<Category> CategoriesList { get; set; }
        public SubCategory SubCategory { get; set; }
        public string StutsMassage { get; set; }
    }
}
