using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;
using RetailBillingSystem.DTOs;
using RetailBillingSystem.Models;

namespace RetailBillingSystem.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CustomersController(AppDbContext db) => _db = db;

        // GET api/customers?search=ravi
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var q = _db.Customers
                .Include(c => c.Invoices)
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q = q.Where(c => c.Name.Contains(search) ||
                                  (c.Phone != null && c.Phone.Contains(search)) ||
                                  (c.GSTIN != null && c.GSTIN.Contains(search)));

            var customers = await q
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.CustomerId,
                    c.Name,
                    c.GSTIN,
                    c.Phone,
                    c.Email,
                    c.City,
                    c.State,
                    TotalInvoices = c.Invoices.Count,
                    TotalBilled   = c.Invoices.Sum(i => i.GrandTotal),
                    BalanceDue    = c.Invoices
                                     .Where(i => i.Status != InvoiceStatus.Paid &&
                                                  i.Status != InvoiceStatus.Cancelled)
                                     .Sum(i => i.GrandTotal)
                })
                .ToListAsync();

            return Ok(customers);
        }

        // GET api/customers/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _db.Customers
                .Include(x => x.Invoices)
                .FirstOrDefaultAsync(x => x.CustomerId == id && x.IsActive);

            if (c == null) return NotFound();
            return Ok(c);
        }

        // POST api/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var c = new Customer
            {
                Name    = req.Name,
                GSTIN   = req.GSTIN,
                Phone   = req.Phone,
                Email   = req.Email,
                Address = req.Address,
                City    = req.City,
                State   = req.State,
                PinCode = req.PinCode
            };

            _db.Customers.Add(c);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = c.CustomerId }, c);
        }

        // PUT api/customers/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCustomerRequest req)
        {
            var c = await _db.Customers.FindAsync(id);
            if (c == null || !c.IsActive) return NotFound();

            c.Name    = req.Name;
            c.GSTIN   = req.GSTIN;
            c.Phone   = req.Phone;
            c.Email   = req.Email;
            c.Address = req.Address;
            c.City    = req.City;
            c.State   = req.State;
            c.PinCode = req.PinCode;

            await _db.SaveChangesAsync();
            return Ok(c);
        }

        // DELETE api/customers/1 (soft)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Customers.FindAsync(id);
            if (c == null) return NotFound();

            bool hasInvoices = await _db.Invoices.AnyAsync(i => i.CustomerId == id);
            if (hasInvoices)
            {
                c.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Customer deactivated (has existing invoices)" });
            }

            _db.Customers.Remove(c);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
