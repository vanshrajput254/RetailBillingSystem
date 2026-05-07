using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;
using RetailBillingSystem.Models;

namespace RetailBillingSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _db;
        public CustomerController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search = null)
        {
            var q = _db.Customers
                .Include(c => c.Invoices)
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q = q.Where(c => c.Name.Contains(search) ||
                                  (c.Phone != null && c.Phone.Contains(search)));

            ViewBag.Search = search;
            return View(await q.OrderBy(c => c.Name).ToListAsync());
        }
    }
}
