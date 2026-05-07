using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;
using RetailBillingSystem.Models;
using RetailBillingSystem.Services;

namespace RetailBillingSystem.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;

        public InvoiceController(AppDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _db.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
            return View(invoices);
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items).ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();
            return View(invoice);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _db.Customers.Where(c => c.IsActive).ToListAsync();
            ViewBag.Products = await _db.Products.Where(p => p.IsActive).ToListAsync();
            return View();
        }

        // ?? Print view (browser print / Ctrl+P) ??????????????????????????????
        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items).ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // ?? Send Invoice via Email ????????????????????????????????????????????
        [HttpPost]
        public async Task<IActionResult> SendEmail(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return Json(new { success = false, message = "Invoice not found" });

            var email = invoice.Customer?.Email;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Customer ka email nahi hai" });

            var sent = await _email.SendInvoiceEmailAsync(invoice, email);

            return Json(sent
                ? new { success = true, message = $"Invoice email bheja gaya: {email}" }
                : new { success = false, message = "Email bhejne mein error. appsettings.json mein EmailSettings check karo." });
        }
    }
}
