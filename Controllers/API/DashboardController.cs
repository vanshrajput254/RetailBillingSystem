using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;
using RetailBillingSystem.DTOs;
using RetailBillingSystem.Models;
using RetailBillingSystem.Services;

namespace RetailBillingSystem.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IGSTService  _gst;

        public DashboardController(AppDbContext db, IGSTService gst)
        {
            _db  = db;
            _gst = gst;
        }

        // GET api/dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var invoices  = await _db.Invoices.Include(i => i.Customer).ToListAsync();
            var products  = await _db.Products.Where(p => p.IsActive).ToListAsync();
            var customers = await _db.Customers.Where(c => c.IsActive).ToListAsync();

            var stats = new DashboardStats
            {
                TotalRevenue    = invoices.Where(i => i.Status == InvoiceStatus.Paid)
                                          .Sum(i => i.GrandTotal),
                TotalInvoices   = invoices.Count,
                GSTCollected    = invoices.Sum(i => i.TotalGST),
                ActiveCustomers = customers.Count,
                LowStockCount   = products.Count(p => p.StockQuantity <= p.LowStockThreshold),
                PendingAmount   = invoices.Where(i => i.Status == InvoiceStatus.Pending ||
                                                        i.Status == InvoiceStatus.Overdue)
                                          .Sum(i => i.GrandTotal),
                RecentInvoices  = invoices
                    .OrderByDescending(i => i.InvoiceDate)
                    .Take(5)
                    .Select(i => new RecentInvoiceDto
                    {
                        InvoiceNumber = i.InvoiceNumber,
                        CustomerName  = i.Customer?.Name ?? "—",
                        GrandTotal    = i.GrandTotal,
                        TotalGST      = i.TotalGST,
                        Status        = i.Status.ToString(),
                        InvoiceDate   = i.InvoiceDate
                    }).ToList()
            };

            return Ok(stats);
        }

        // GET api/dashboard/gst-report?month=4&year=2025
        [HttpGet("gst-report")]
        public async Task<IActionResult> GetGSTReport(
            [FromQuery] int? month = null,
            [FromQuery] int? year  = null)
        {
            int  y = year  ?? DateTime.Now.Year;
            int  m = month ?? DateTime.Now.Month;
            var start = new DateTime(y, m, 1);
            var end   = start.AddMonths(1);

            var invoices = await _db.Invoices
                .Where(i => i.InvoiceDate >= start  &&
                             i.InvoiceDate <  end   &&
                             i.Status      != InvoiceStatus.Cancelled)
                .ToListAsync();

            var report = new GSTReportDto
            {
                TotalSales      = invoices.Sum(i => i.Subtotal),
                TotalDiscount   = invoices.Sum(i => i.TotalDiscount),
                TaxableAmount   = invoices.Sum(i => i.TaxableAmount),
                TotalCGST       = invoices.Sum(i => i.CGSTAmount),
                TotalSGST       = invoices.Sum(i => i.SGSTAmount),
                TotalIGST       = invoices.Sum(i => i.IGSTAmount),
                TotalGSTPayable = invoices.Sum(i => i.TotalGST)
            };

            return Ok(report);
        }

        // POST api/dashboard/calculate-gst   (live preview)
        [HttpPost("calculate-gst")]
        public IActionResult CalculateGST([FromBody] GSTCalculateRequest req)
        {
            if (!Enum.TryParse<GSTType>(req.GSTType, true, out var gstType))
                gstType = GSTType.CGST_SGST;

            var calc = _gst.CalculateLineItem(
                req.Rate, req.Quantity, req.DiscountPercent, req.GSTRate, gstType);

            return Ok(calc);
        }
    }
}
