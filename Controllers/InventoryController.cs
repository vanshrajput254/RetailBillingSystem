using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;

namespace RetailBillingSystem.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _db;
        public InventoryController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search = null)
        {
            var q = _db.Products.Where(p => p.IsActive).AsQueryable();
            if (!string.IsNullOrEmpty(search))
                q = q.Where(p => p.ProductName.Contains(search) || p.SKU.Contains(search));

            ViewBag.Search = search;
            return View(await q.OrderBy(p => p.ProductName).ToListAsync());
        }
    }
}
