using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Data;
using RetailBillingSystem.DTOs;
using RetailBillingSystem.Models;

namespace RetailBillingSystem.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductsController(AppDbContext db) => _db = db;

        // GET api/products?search=rice&lowStock=true
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search   = null,
            [FromQuery] bool?   lowStock = null)
        {
            var q = _db.Products.Where(p => p.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q = q.Where(p => p.ProductName.Contains(search) ||
                                  p.SKU.Contains(search)         ||
                                  p.Category.Contains(search));

            if (lowStock == true)
                q = q.Where(p => p.StockQuantity <= p.LowStockThreshold);

            var products = await q
                .OrderBy(p => p.ProductName)
                .Select(p => new
                {
                    p.ProductId,
                    p.SKU,
                    p.ProductName,
                    p.Category,
                    p.Rate,
                    p.GSTRate,
                    p.StockQuantity,
                    p.LowStockThreshold,
                    StockStatus = p.StockQuantity <= p.LowStockThreshold ? "Low" : "OK"
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET api/products/3
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null || !p.IsActive) return NotFound();
            return Ok(p);
        }

        // POST api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool skuExists = await _db.Products.AnyAsync(p => p.SKU == req.SKU);
            if (skuExists)
                return Conflict(new { message = "SKU already exists" });

            var product = new Product
            {
                SKU               = req.SKU,
                ProductName       = req.ProductName,
                Category          = req.Category ?? string.Empty,
                Rate              = req.Rate,
                GSTRate           = req.GSTRate,
                StockQuantity     = req.StockQuantity,
                LowStockThreshold = req.LowStockThreshold
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
        }

        // PUT api/products/3
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProductRequest req)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();

            bool skuConflict = await _db.Products
                .AnyAsync(x => x.SKU == req.SKU && x.ProductId != id);
            if (skuConflict)
                return Conflict(new { message = "SKU already used by another product" });

            p.SKU               = req.SKU;
            p.ProductName       = req.ProductName;
            p.Category          = req.Category ?? string.Empty;
            p.Rate              = req.Rate;
            p.GSTRate           = req.GSTRate;
            p.StockQuantity     = req.StockQuantity;
            p.LowStockThreshold = req.LowStockThreshold;
            p.UpdatedAt         = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(p);
        }

        // DELETE api/products/3 (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();

            bool hasInvoiceItems = await _db.InvoiceItems.AnyAsync(ii => ii.ProductId == id);
            if (hasInvoiceItems)
            {
                p.IsActive  = false;
                p.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Product deactivated (has linked invoice items)" });
            }

            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
