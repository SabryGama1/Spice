using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spice.Models;
using Stripe;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext db;

        public CartsController(ApplicationDbContext db)
        {
            this.db = db;
        }
        [BindProperty]
        public OrderDetailsCartViewModel OrderDetailsCartVM { get; set; }
        public IActionResult Index()
        {
            OrderDetailsCartVM = new OrderDetailsCartViewModel()
            {
                OrderHeader = new OrderHeader()
            };
            OrderDetailsCartVM.OrderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var shoppingCarts = db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value);
            if (shoppingCarts != null)
            {
                OrderDetailsCartVM.ShoppingCartList = shoppingCarts.ToList();
            }
            foreach (var item in OrderDetailsCartVM.ShoppingCartList)
            {
                item.MenuItem = db.MenuItems.FirstOrDefault(m => m.Id == item.MenuItemId);
                OrderDetailsCartVM.OrderHeader.OrderTotal += item.MenuItem.Price * item.Count;
                OrderDetailsCartVM.OrderHeader.OrderTotal = Math.Round(OrderDetailsCartVM.OrderHeader.OrderTotal, 2);
                item.MenuItem.Description = StaticDefintion.ConvertToRawHtml(item.MenuItem.Description);
                if (item.MenuItem.Description.Length > 75)
                {
                    item.MenuItem.Description = item.MenuItem.Description.Substring(0, 74);
                }
            }
            OrderDetailsCartVM.OrderHeader.OrderTotalOrginal = OrderDetailsCartVM.OrderHeader.OrderTotal;
            if (HttpContext.Session.GetString(StaticDefintion.ssCouponCode) != null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDefintion.ssCouponCode);
                var couponCodeFromDb = db.Coupons.Where(E => E.Name == OrderDetailsCartVM.OrderHeader.CouponCode).FirstOrDefault();
                OrderDetailsCartVM.OrderHeader.OrderTotal = StaticDefintion.DiscountPrice(couponCodeFromDb, OrderDetailsCartVM.OrderHeader.OrderTotalOrginal);
            }

            return View(OrderDetailsCartVM);
        }

        public IActionResult Summary()
        {
            OrderDetailsCartVM = new OrderDetailsCartViewModel()
            {
                OrderHeader = new Models.OrderHeader()
            };
            OrderDetailsCartVM.OrderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var AppUser = db.ApplicationUsers.Find(claim.Value);
            OrderDetailsCartVM.OrderHeader.PickUpName = AppUser.Name;
            OrderDetailsCartVM.OrderHeader.PhoneNumber = AppUser.PhoneNumber;
            OrderDetailsCartVM.OrderHeader.PickUpDate = DateTime.Now;


            var shoppingCarts = db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value);
            if (shoppingCarts != null)
            {
                OrderDetailsCartVM.ShoppingCartList = shoppingCarts.ToList();
            }
            foreach (var item in OrderDetailsCartVM.ShoppingCartList)
            {
                item.MenuItem = db.MenuItems.FirstOrDefault(m => m.Id == item.MenuItemId);
                OrderDetailsCartVM.OrderHeader.OrderTotal += item.MenuItem.Price * item.Count;
                OrderDetailsCartVM.OrderHeader.OrderTotal = Math.Round(OrderDetailsCartVM.OrderHeader.OrderTotal, 2);

            }
            OrderDetailsCartVM.OrderHeader.OrderTotalOrginal = OrderDetailsCartVM.OrderHeader.OrderTotal;
            if (HttpContext.Session.GetString(StaticDefintion.ssCouponCode) != null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDefintion.ssCouponCode);
                var couponCodeFromDb = db.Coupons.Where(E => E.Name.ToLower() == OrderDetailsCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefault();
                OrderDetailsCartVM.OrderHeader.OrderTotal = StaticDefintion.DiscountPrice(couponCodeFromDb, OrderDetailsCartVM.OrderHeader.OrderTotalOrginal);
            }

            return View(OrderDetailsCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            OrderDetailsCartVM.ShoppingCartList = await db.ShoppingCarts.Where(e => e.ApplicationUserId == claim.Value).ToListAsync();

            OrderDetailsCartVM.OrderHeader.PaymentStatus = StaticDefintion.PaymentStatusPending;
            OrderDetailsCartVM.OrderHeader.OrderDate = DateTime.Now;
            OrderDetailsCartVM.OrderHeader.UserId = claim.Value;
            OrderDetailsCartVM.OrderHeader.Status = StaticDefintion.PaymentStatusPending;
            OrderDetailsCartVM.OrderHeader.PickUpTime = Convert.ToDateTime(OrderDetailsCartVM.OrderHeader.PickUpDate.ToShortDateString() + " " +
            OrderDetailsCartVM.OrderHeader.PickUpTime.ToShortTimeString());
            OrderDetailsCartVM.OrderHeader.OrderTotalOrginal = 0;
            db.OrderHeaders.Add(OrderDetailsCartVM.OrderHeader);
            await db.SaveChangesAsync();


            foreach (var item in OrderDetailsCartVM.ShoppingCartList)
            {
                item.MenuItem = db.MenuItems.FirstOrDefault(m => m.Id == item.MenuItemId);

                OrderDetail orderDetail = new OrderDetail()
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = OrderDetailsCartVM.OrderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };

                OrderDetailsCartVM.OrderHeader.OrderTotal += item.MenuItem.Price * item.Count;
                db.OrderDetails.Add(orderDetail);
            }


            if (HttpContext.Session.GetString(StaticDefintion.ssCouponCode) != null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDefintion.ssCouponCode);
                var couponCodeFromDb = db.Coupons.Where(e => e.Name.ToLower() == OrderDetailsCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefault();
                OrderDetailsCartVM.OrderHeader.OrderTotal = StaticDefintion.DiscountPrice(couponCodeFromDb, OrderDetailsCartVM.OrderHeader.OrderTotalOrginal);
            }
            else
            {
                OrderDetailsCartVM.OrderHeader.OrderTotal = Math.Round(OrderDetailsCartVM.OrderHeader.OrderTotalOrginal, 2);

            }
            OrderDetailsCartVM.OrderHeader.CouponCodeDiscount = OrderDetailsCartVM.OrderHeader.OrderTotalOrginal - OrderDetailsCartVM.OrderHeader.OrderTotal;
            db.ShoppingCarts.RemoveRange(OrderDetailsCartVM.ShoppingCartList);
            HttpContext.Session.SetInt32(StaticDefintion.ShoppingCartCount, 0);
            await db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(OrderDetailsCartVM.OrderHeader.OrderTotal * 100),
                Currency = "usd",
                Description = "Order Id : " + OrderDetailsCartVM.OrderHeader.Id,
                Source = stripeToken
            };

            var service = new ChargeService();

            Charge charge = service.Create(options);

            if (charge.BalanceTransactionId == null)
            {
                OrderDetailsCartVM.OrderHeader.PaymentStatus = StaticDefintion.PaymentStatusRejected;
            }
            else
            {
                OrderDetailsCartVM.OrderHeader.TrasactionId = charge.BalanceTransactionId;
            }
            if (charge.Status.ToLower() == "succeeded")
            {
                OrderDetailsCartVM.OrderHeader.PaymentStatus = StaticDefintion.PaymentStatusApproved;
                OrderDetailsCartVM.OrderHeader.Status = StaticDefintion.StatusSubmitted;

            }
            else
            {
                OrderDetailsCartVM.OrderHeader.PaymentStatus = StaticDefintion.PaymentStatusRejected;

            }
            await db.SaveChangesAsync();

            //return RedirectToAction("Index", "Home");
            return RedirectToAction("Confirm", "Orders", new { OrderDetailsCartVM.OrderHeader.Id });
        }


        public ActionResult ApplyCoupon()
        {
            if (OrderDetailsCartVM.OrderHeader.CouponCode == null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(StaticDefintion.ssCouponCode, OrderDetailsCartVM.OrderHeader.CouponCode);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult RemoveCoupon()
        {

            HttpContext.Session.SetString(StaticDefintion.ssCouponCode, string.Empty);
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> Plus(int cartid)
        {
            var shoppingCart = await db.ShoppingCarts.FindAsync(cartid);
            shoppingCart.Count += 1;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<ActionResult> Minus(int cartid)
        {
            var shoppingCart = await db.ShoppingCarts.FindAsync(cartid);
            if (shoppingCart.Count > 1)
            {
                shoppingCart.Count -= 1;
                await db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));

        }
        public async Task<ActionResult> Remove(int cartId)
        {
            var shoppingCart = await db.ShoppingCarts.FindAsync(cartId);
            db.ShoppingCarts.Remove(shoppingCart);
            await db.SaveChangesAsync();
            var count = db.ShoppingCarts.Where(e => e.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(StaticDefintion.ShoppingCartCount, count);
            return RedirectToAction(nameof(Index));

        }
    }
}