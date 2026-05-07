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
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IGSTService _gst;

        public InvoicesController(AppDbContext db, IGSTService gst)
        {
            _db = db;
            _gst = gst;
        }

        // GET api/invoices?status=Paid&customerId=1
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null,
            [FromQuery] string? search = null)
        {
            var q = _db.Invoices
                        .Include(i => i.Customer)
                        .Include(i => i.Items)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<InvoiceStatus>(status, true, out var s))
                q = q.Where(i => i.Status == s);

            if (customerId.HasValue)
                q = q.Where(i => i.CustomerId == customerId.Value);

            if (!string.IsNullOrEmpty(search))
                q = q.Where(i => i.InvoiceNumber.Contains(search) ||
                                  i.Customer.Name.Contains(search));

            var invoices = await q
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new
                {
                    i.InvoiceId,
                    i.InvoiceNumber,
                    CustomerName = i.Customer.Name,
                    i.InvoiceDate,
                    i.DueDate,
                    i.Subtotal,
                    i.TotalDiscount,
                    i.TaxableAmount,
                    i.CGSTAmount,
                    i.SGSTAmount,
                    i.IGSTAmount,
                    i.TotalGST,
                    i.GrandTotal,
                    Status = i.Status.ToString(),
                    GSTType = i.GSTType.ToString(),
                    ItemCount = i.Items.Count
                })
                .ToListAsync();

            return Ok(invoices);
        }

        // GET api/invoices/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items).ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return NotFound(new { message = "Invoice not found" });

            // Return flat DTO to avoid circular reference
            return Ok(new
            {
                invoice.InvoiceId,
                invoice.InvoiceNumber,
                invoice.InvoiceDate,
                invoice.DueDate,
                invoice.BuyerGSTIN,
                invoice.Notes,
                GSTType = invoice.GSTType.ToString(),
                Status = invoice.Status.ToString(),
                invoice.Subtotal,
                invoice.TotalDiscount,
                invoice.TaxableAmount,
                invoice.CGSTAmount,
                invoice.SGSTAmount,
                invoice.IGSTAmount,
                invoice.TotalGST,
                invoice.GrandTotal,
                Customer = invoice.Customer == null ? null : new
                {
                    invoice.Customer.CustomerId,
                    invoice.Customer.Name,
                    invoice.Customer.GSTIN,
                    invoice.Customer.Phone,
                    invoice.Customer.Email,
                    invoice.Customer.Address,
                    invoice.Customer.City,
                    invoice.Customer.State
                },
                Items = invoice.Items.Select(ii => new {
                    ii.InvoiceItemId,
                    ii.ProductId,
                    ii.ProductName,
                    ii.Rate,
                    ii.Quantity,
                    ii.DiscountPercent,
                    ii.GSTRate,
                    ii.DiscountAmount,
                    ii.TaxableAmount,
                    ii.GSTAmount,
                    ii.LineTotal
                })
            });
        }

        // POST api/invoices
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var customer = await _db.Customers.FindAsync(req.CustomerId);
            if (customer == null) return BadRequest(new { message = "Customer not found" });

            // Parse GST type
            if (!Enum.TryParse<GSTType>(req.GSTType, true, out var gstType))
                gstType = GSTType.CGST_SGST;

            // Build line items with calculations
            var itemCalcs = new List<(InvoiceItem item, InvoiceItemCalculation calc)>();
            foreach (var r in req.Items)
            {
                var product = await _db.Products.FindAsync(r.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Product {r.ProductId} not found" });
                if (product.StockQuantity < r.Quantity)
                    return BadRequest(new { message = $"Insufficient stock for {product.ProductName}" });

                var calc = _gst.CalculateLineItem(r.Rate, r.Quantity, r.DiscountPercent, r.GSTRate, gstType);
                var item = new InvoiceItem
                {
                    ProductId = r.ProductId,
                    ProductName = product.ProductName,
                    Rate = r.Rate,
                    Quantity = r.Quantity,
                    DiscountPercent = r.DiscountPercent,
                    GSTRate = r.GSTRate,
                    DiscountAmount = calc.DiscountAmount,
                    TaxableAmount = calc.TaxableAmount,
                    GSTAmount = calc.GSTAmount,
                    LineTotal = calc.LineTotal
                };
                itemCalcs.Add((item, calc));

                // Deduct stock
                product.StockQuantity -= r.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            var totals = _gst.CalculateInvoiceTotals(itemCalcs.Select(x => x.calc), gstType);

            // Generate invoice number
            int nextSeq = await _db.Invoices.CountAsync() + 1;
            string invNum = _gst.GenerateInvoiceNumber(nextSeq);

            var invoice = new Invoice
            {
                InvoiceNumber = invNum,
                CustomerId = req.CustomerId,
                InvoiceDate = req.InvoiceDate,
                DueDate = req.DueDate,
                GSTType = gstType,
                BuyerGSTIN = req.BuyerGSTIN,
                Notes = req.Notes,
                Subtotal = totals.Subtotal,
                TotalDiscount = totals.TotalDiscount,
                TaxableAmount = totals.TaxableAmount,
                CGSTAmount = totals.CGSTAmount,
                SGSTAmount = totals.SGSTAmount,
                IGSTAmount = totals.IGSTAmount,
                TotalGST = totals.TotalGST,
                GrandTotal = totals.GrandTotal,
                Status = InvoiceStatus.Pending,
                Items = itemCalcs.Select(x => x.item).ToList()
            };

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = invoice.InvoiceId }, new
            {
                invoice.InvoiceId,
                invoice.InvoiceNumber,
                invoice.GrandTotal,
                invoice.Status
            });
        }

        // PATCH api/invoices/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusRequest req)
        {
            var invoice = await _db.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            if (!Enum.TryParse<InvoiceStatus>(req.Status, true, out var status))
                return BadRequest(new { message = "Invalid status value" });

            invoice.Status = status;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Status updated", status = status.ToString() });
        }

        // DELETE api/invoices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _db.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();
            if (invoice.Status == InvoiceStatus.Paid)
                return BadRequest(new { message = "Cannot delete a paid invoice" });

            _db.Invoices.Remove(invoice);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
