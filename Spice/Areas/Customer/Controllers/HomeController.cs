using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;

        public HomeController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IActionResult> Index()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                List<ShoppingCart> shoppingCarts = await db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value).ToListAsync();
                HttpContext.Session.SetInt32(StaticDefintion.ShoppingCartCount, shoppingCarts.Count);

            }

            IndexViewModel indexVM = new IndexViewModel()
            {
                Categories = await db.Categories.ToListAsync(),
                Coupons = await db.Coupons.Where(m => m.IsActive == true).ToListAsync(),
                MenuItems = await db.MenuItems.Include(e => e.Category).Include(e => e.SubCategory).ToListAsync(),

            };
            return View(indexVM);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int itemid)
        {
            var menuitem = await db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(e => e.Id == itemid).FirstOrDefaultAsync();


            ShoppingCart shoppingCart = new ShoppingCart()
            {
                MenuItem = menuitem,
                MenuItemId = menuitem.Id,

            };
            return View(shoppingCart);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            if (ModelState.IsValid)
            {
                //shoppingCart.Id = 0;
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = claim.Value;
                var shoppingCartFromDb = await db.ShoppingCarts.Where(m => m.ApplicationUserId == shoppingCart.ApplicationUserId && m.MenuItemId == shoppingCart.MenuItemId).FirstOrDefaultAsync();
                if (shoppingCartFromDb == null)
                {
                    db.ShoppingCarts.Add(shoppingCart);
                }
                else
                {
                    shoppingCartFromDb.Count += shoppingCart.Count;
                }
                await db.SaveChangesAsync();
                var count = db.ShoppingCarts.Where(e => e.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(StaticDefintion.ShoppingCartCount, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var menuitem = await db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(e => e.Id == shoppingCart.MenuItemId).FirstOrDefaultAsync();


                ShoppingCart shoppingCartObj = new ShoppingCart()
                {
                    MenuItem = menuitem,
                    MenuItemId = menuitem.Id

                };
                return View(shoppingCartObj);
            }
        }
    }
}
