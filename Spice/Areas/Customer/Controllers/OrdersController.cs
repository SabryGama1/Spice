using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spice.Models;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db;

        public OrdersController(ApplicationDbContext db)
        {
            this.db = db;
        }
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentitry = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentitry.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
            {
                OrderHeader = await db.OrderHeaders.Include(r => r.ApplicationUser).FirstOrDefaultAsync(e => e.UserId == claim.Value && e.Id == id),
                OrderDetails = await db.OrderDetails.Where(e => e.OrderId == id).ToListAsync()
            };
            return View(orderDetailsVM);
        }

        private int pageSize = 2;
        [Authorize]
        public async Task<IActionResult> OrderHistory(int pageNumber = 1)
        {
            var claimsIdentitry = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentitry.FindFirst(ClaimTypes.NameIdentifier);
            //List<OrderDetailsViewModel> orderDetailsVMlist = new List<OrderDetailsViewModel>();

            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            List<OrderHeader> orderHeaderslist = await db.OrderHeaders.Include(e => e.ApplicationUser).Where(m => m.UserId == claim.Value).ToListAsync();
            foreach (var orderHeader in orderHeaderslist)
            {
                OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
                {
                    OrderHeader = orderHeader,
                    OrderDetails = await db.OrderDetails.Where(m => m.OrderId == orderHeader.Id).ToListAsync()
                };
                orderListVM.Orders.Add(orderDetailsVM);
            }
            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(e => e.OrderHeader.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo()
            {
                CurrentPage = pageNumber,
                RecordsPerPage = pageSize,
                TotalRecords = count,
                urlParam = "/Customer/Orders/OrderHistory?pageNumber=:"
            };

            return View(orderListVM);
        }

        public async Task<IActionResult> GetOrderDatails(int id)
        {

            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
            {
                OrderHeader = await db.OrderHeaders.Include(r => r.ApplicationUser).FirstOrDefaultAsync(e => e.Id == id),
                OrderDetails = await db.OrderDetails.Where(e => e.OrderId == id).ToListAsync()
            };
            return PartialView("_IndividualOrderDetails", orderDetailsVM);
        }
        public async Task<IActionResult> GetOrderStutas(int id)
        {
            OrderHeader orderHeader = await db.OrderHeaders.FindAsync(id);

            return PartialView("_OrderStutas", orderHeader.Status);
        }

        [Authorize(Roles = StaticDefintion.ManagerUser + "," + StaticDefintion.KitchenUser)]
        public async Task<IActionResult> ManageOrder()
        {
            List<OrderDetailsViewModel> orderDetailsVMlist = new List<OrderDetailsViewModel>();

            List<OrderHeader> orderHeaderslist = await db.OrderHeaders.Where(m => m.Status == StaticDefintion.StatusInProccess || m.Status == StaticDefintion.StatusSubmitted).ToListAsync();
            foreach (var orderHeader in orderHeaderslist)
            {
                OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
                {
                    OrderHeader = orderHeader,
                    OrderDetails = await db.OrderDetails.Where(m => m.OrderId == orderHeader.Id).ToListAsync()
                };
                orderDetailsVMlist.Add(orderDetailsVM);
            }

            return View(orderDetailsVMlist.OrderBy(e => e.OrderHeader.PickUpTime).ToList());
        }

        [Authorize(Roles = StaticDefintion.ManagerUser + "," + StaticDefintion.KitchenUser)]
        public async Task<IActionResult> OrderPrepare(int orderId)
        {
            var orderHeder = await db.OrderHeaders.FindAsync(orderId);
            orderHeder.Status = StaticDefintion.StatusInProccess;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageOrder));
        }

        [Authorize(Roles = StaticDefintion.ManagerUser + "," + StaticDefintion.KitchenUser)]
        public async Task<IActionResult> OrderReady(int orderId)
        {
            var orderHeder = await db.OrderHeaders.FindAsync(orderId);
            orderHeder.Status = StaticDefintion.StatusReady;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageOrder));
        }

        [Authorize(Roles = StaticDefintion.ManagerUser + "," + StaticDefintion.KitchenUser)]
        public async Task<IActionResult> OrderCancel(int orderId)
        {
            var orderHeder = await db.OrderHeaders.FindAsync(orderId);
            orderHeder.Status = StaticDefintion.StatusCancalled;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageOrder));
        }

        [Authorize(Roles = StaticDefintion.ManagerUser + "," + StaticDefintion.FrontDeskUser)]
        public async Task<IActionResult> OrderPickup(int pageNumber = 1, string searchName = null, string searchPhone = null, string searchEmail = null)
        {
            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Orders/OrderPickup?pageNumber=:");
            param.Append("&searchName");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            else
            {
                searchName = "";
            }
            param.Append("&searchPhone");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            else
            {
                searchPhone = "";
            }
            param.Append("&searchEmail");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            else
            {
                searchEmail = "";
            }


            List<OrderHeader> orderHeaderslist = await db.OrderHeaders.Include(e => e.ApplicationUser).OrderByDescending(e => e.OrderDate)
                .Where(m => m.Status == StaticDefintion.StatusReady && m.PickUpName.Contains(searchName) && m.PhoneNumber.Contains(searchPhone) &&
                m.ApplicationUser.Email.Contains(searchEmail)).ToListAsync();
            foreach (var orderHeader in orderHeaderslist)
            {
                OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
                {
                    OrderHeader = orderHeader,
                    OrderDetails = await db.OrderDetails.Where(m => m.OrderId == orderHeader.Id).ToListAsync()
                };
                orderListVM.Orders.Add(orderDetailsVM);
            }
            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(e => e.OrderHeader.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo()
            {
                CurrentPage = pageNumber,
                RecordsPerPage = pageSize,
                TotalRecords = count,
                urlParam = param.ToString()

            };

            return View(orderListVM);
        }


        [Authorize(Roles = StaticDefintion.ManagerUser + "," + StaticDefintion.FrontDeskUser)]
        [HttpPost]
        public async Task<IActionResult> OrderPickup(int orderId)
        {
            var orderHeder = await db.OrderHeaders.FindAsync(orderId);
            orderHeder.Status = StaticDefintion.StatusCompleted;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(OrderPickup));
        }
    }
}
