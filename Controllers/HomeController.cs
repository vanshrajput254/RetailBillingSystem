using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;
using RetailBillingSystem.Models;

namespace RetailBillingSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var invoices = await _db.Invoices.Include(i => i.Customer).ToListAsync();
            var products = await _db.Products.Where(p => p.IsActive).ToListAsync();
            var customers = await _db.Customers.Where(c => c.IsActive).ToListAsync();

            ViewBag.TotalRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.GrandTotal);
            ViewBag.TotalInvoices = invoices.Count;
            ViewBag.GSTCollected = invoices.Sum(i => i.TotalGST);
            ViewBag.ActiveCustomers = customers.Count;
            ViewBag.LowStockCount = products.Count(p => p.StockQuantity <= p.LowStockThreshold);
            ViewBag.PendingAmount = invoices.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.GrandTotal);
            ViewBag.RecentInvoices = invoices.OrderByDescending(i => i.InvoiceDate).Take(5).ToList();

            // ?? Chart Data ????????????????????????????????????????????????????

            // Monthly Revenue — last 6 months
            var now = DateTime.Now;
            var monthly = new List<object>();
            for (int m = 5; m >= 0; m--)
            {
                var d = now.AddMonths(-m);
                var start = new DateTime(d.Year, d.Month, 1);
                var end = start.AddMonths(1);
                monthly.Add(new
                {
                    month = d.ToString("MMM yy"),
                    revenue = invoices.Where(i => i.InvoiceDate >= start && i.InvoiceDate < end)
                                      .Sum(i => i.GrandTotal),
                    gst = invoices.Where(i => i.InvoiceDate >= start && i.InvoiceDate < end)
                                      .Sum(i => i.TotalGST)
                });
            }
            ViewBag.MonthlyChartData = System.Text.Json.JsonSerializer.Serialize(monthly);

            // Invoice Status breakdown
            var statusData = new[]
            {
                new { label = "Paid",      value = invoices.Count(i => i.Status == InvoiceStatus.Paid),      color = "#22c55e" },
                new { label = "Pending",   value = invoices.Count(i => i.Status == InvoiceStatus.Pending),   color = "#f59e0b" },
                new { label = "Overdue",   value = invoices.Count(i => i.Status == InvoiceStatus.Overdue),   color = "#ef4444" },
                new { label = "Cancelled", value = invoices.Count(i => i.Status == InvoiceStatus.Cancelled), color = "#94a3b8" }
            };
            ViewBag.StatusChartData = System.Text.Json.JsonSerializer.Serialize(statusData);

            return View();
        }
    }
}
