using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDefintion.ManagerUser)]

    [Area("Admin")]
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        public SubCategoriesController(ApplicationDbContext db)
        {
            this.db = db;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subcategories = await db.SubCategories.Include(e => e.Category).ToListAsync();

            return View(subcategories);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoriesList = await db.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory(),
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doessubcategoryexisit = db.SubCategories.Include(m => m.Category).Where(m => m.Category.Id == model.SubCategory.CategoryId && m.Name == model.SubCategory.Name);

                if (doessubcategoryexisit.Count() > 0)
                {

                }
                else
                {
                    db.SubCategories.Add(model.SubCategory);
                    await db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }




            SubCategoryAndCategoryViewModel modelvm = new SubCategoryAndCategoryViewModel()
            {
                CategoriesList = await db.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory()
            };

            return View(modelvm);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await db.SubCategories.Where(e => e.CategoryId == id).ToListAsync();


            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategory = await db.SubCategories.FindAsync(id);
            if (subcategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoriesList = await db.Categories.ToListAsync(),
                SubCategory = subcategory
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doessubcategoryexisit = db.SubCategories.Include(m => m.Category).Where(m => m.Category.Id == model.SubCategory.CategoryId && m.Name == model.SubCategory.Name && m.Id != model.SubCategory.Id);

                if (doessubcategoryexisit.Count() > 0)
                {
                }
                else
                {
                    db.SubCategories.Update(model.SubCategory);
                    await db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelvm = new SubCategoryAndCategoryViewModel()
            {
                CategoriesList = await db.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory()
            };

            return View(modelvm);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategory = db.SubCategories.Include(e => e.Category).Where(m => m.Id == id).SingleOrDefault();
            if (subcategory == null)
            {
                return NotFound();
            }

            return View(subcategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(SubCategory subCategory)
        {
            db.SubCategories.Remove(subCategory);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategory = db.SubCategories.Include(e => e.Category).Where(m => m.Id == id).SingleOrDefault();
            if (subcategory == null)
            {
                return NotFound();
            }

            return View(subcategory);
        }
        }
    }
