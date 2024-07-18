using System.Collections.Generic;

namespace Spice.Models.ViewModels
{
    public class MenuItemeViewModel
    {
        public MenuItem MenuItem { get; set; }
        public IEnumerable<Category> CategoriesList { get; set; }
        public IEnumerable<SubCategory> SubCategoriesList { get; set; }
    }
}
