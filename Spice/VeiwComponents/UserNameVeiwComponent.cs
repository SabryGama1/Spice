using Microsoft.AspNetCore.Mvc;
using Spice.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.VeiwComponents
{
    public class UserNameVeiwComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public UserNameVeiwComponent(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userFromDb = await db.ApplicationUsers.FindAsync(claim.Value);

            return View(userFromDb);
        }
    }
}
