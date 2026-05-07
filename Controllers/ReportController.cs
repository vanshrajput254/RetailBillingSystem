using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;
using RetailBillingSystem.Models;

namespace RetailBillingSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _db;
        public ReportController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? month, int? year)
        {
            int y = year  ?? DateTime.Now.Year;
            int m = month ?? DateTime.Now.Month;
            var start = new DateTime(y, m, 1);
            var end   = start.AddMonths(1);

            var invoices = await _db.Invoices
                .Where(i => i.InvoiceDate >= start &&
                             i.InvoiceDate <  end  &&
                             i.Status      != InvoiceStatus.Cancelled)
                .ToListAsync();

            ViewBag.Month         = m;
            ViewBag.Year          = y;
            ViewBag.TotalSales    = invoices.Sum(i => i.Subtotal);
            ViewBag.TotalDiscount = invoices.Sum(i => i.TotalDiscount);
            ViewBag.TaxableAmount = invoices.Sum(i => i.TaxableAmount);
            ViewBag.TotalCGST     = invoices.Sum(i => i.CGSTAmount);
            ViewBag.TotalSGST     = invoices.Sum(i => i.SGSTAmount);
            ViewBag.TotalIGST     = invoices.Sum(i => i.IGSTAmount);
            ViewBag.TotalGST      = invoices.Sum(i => i.TotalGST);
            return View(invoices);
        }
    }
}
