using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDefintion.ManagerUser)]

    [Area("Admin")]
    public class MenuItemesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment webHostEnvironment;

        [BindProperty]
        public MenuItemeViewModel MenuItemeVM { get; set; }

        public MenuItemesController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            this.db = db;
            this.webHostEnvironment = webHostEnvironment;
            MenuItemeVM = new MenuItemeViewModel()
            {
                MenuItem = new Models.MenuItem(),
                CategoriesList = db.Categories.ToList()
            };
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var menuItems = await db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            return View(menuItems);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(MenuItemeVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            if (ModelState.IsValid)
            {
                string imagepath = @"\images\1.jpg";
                var files = HttpContext.Request.Form.Files;
                if (files.Count() > 0)
                {
                    string webRootPath = webHostEnvironment.WebRootPath;
                    string imageName = DateTime.Now.ToFileTime().ToString() + Path.GetExtension(files[0].FileName);
                    FileStream fileStream = new FileStream(Path.Combine(webRootPath, "images", imageName), FileMode.Create);
                    files[0].CopyTo(fileStream);
                    imagepath = @"\images\" + imageName;
                }
                MenuItemeVM.MenuItem.Image = imagepath;
                db.MenuItems.Add(MenuItemeVM.MenuItem);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(MenuItemeVM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var menuIteme = db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).FirstOrDefault(e => e.Id == id);
            if (menuIteme == null)
            {
                return NotFound();
            }
            MenuItemeVM.MenuItem = menuIteme;
            MenuItemeVM.SubCategoriesList = await db.SubCategories.Where(e => e.CategoryId == MenuItemeVM.MenuItem.CategoryId).ToListAsync();
            return View(MenuItemeVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost()
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count() > 0)
                {
                    string webRootPath = webHostEnvironment.WebRootPath;
                    string imageName = DateTime.Now.ToFileTime().ToString() + Path.GetExtension(files[0].FileName);
                    FileStream fileStream = new FileStream(Path.Combine(webRootPath, "images", imageName), FileMode.Create);
                    files[0].CopyTo(fileStream);
                    string imagepath = @"\images\" + imageName;
                    MenuItemeVM.MenuItem.Image = imagepath;
                }
                db.MenuItems.Update(MenuItemeVM.MenuItem);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(MenuItemeVM);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var menuIteme = db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).FirstOrDefault(e => e.Id == id);
            if (menuIteme == null)
            {
                return NotFound();
            }
            MenuItemeVM.MenuItem = menuIteme;
            MenuItemeVM.SubCategoriesList = await db.SubCategories.Where(e => e.CategoryId == MenuItemeVM.MenuItem.CategoryId).ToListAsync();
            return View(MenuItemeVM);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var menuIteme = db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).FirstOrDefault(e => e.Id == id);
            if (menuIteme == null)
            {
                return NotFound();
            }
            MenuItemeVM.MenuItem = menuIteme;
            MenuItemeVM.SubCategoriesList = await db.SubCategories.Where(e => e.CategoryId == MenuItemeVM.MenuItem.CategoryId).ToListAsync();
            return View(MenuItemeVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost()
        {
            /*
                var files = HttpContext.Request.Form.Files;
                if (files.Count() > 0)
                {
                    string webRootPath = webHostEnvironment.WebRootPath;
                    string imageName = DateTime.Now.ToFileTime().ToString() + Path.GetExtension(files[0].FileName);
                    FileStream fileStream = new FileStream(Path.Combine(webRootPath, "images", imageName), FileMode.Create);
                    files[0].CopyTo(fileStream);
                    string imagepath = @"\images\" + imageName;
                    MenuItemeVM.MenuItem.Image = imagepath;
                */
            db.MenuItems.Remove(MenuItemeVM.MenuItem);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

    }
}
