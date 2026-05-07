namespace RetailBillingSystem.DTOs
{
    public class InvoiceItemCalculation
    {
        public decimal GrossAmount    { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount  { get; set; }
        public decimal GSTAmount      { get; set; }
        public decimal CGSTAmount     { get; set; }
        public decimal SGSTAmount     { get; set; }
        public decimal IGSTAmount     { get; set; }
        public decimal LineTotal      { get; set; }
    }

    public class InvoiceTotals
    {
        public decimal Subtotal      { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CGSTAmount    { get; set; }
        public decimal SGSTAmount    { get; set; }
        public decimal IGSTAmount    { get; set; }
        public decimal TotalGST      { get; set; }
        public decimal GrandTotal    { get; set; }
    }

    // ── API Request / Response DTOs ──────────────────────────────────────────

    public class CreateInvoiceRequest
    {
        public int CustomerId          { get; set; }
        public DateTime InvoiceDate    { get; set; } = DateTime.Today;
        public DateTime? DueDate       { get; set; }
        public string GSTType          { get; set; } = "CGST_SGST";
        public string? BuyerGSTIN      { get; set; }
        public string? Notes           { get; set; }
        public List<InvoiceItemRequest> Items { get; set; } = new();
    }

    public class InvoiceItemRequest
    {
        public int    ProductId       { get; set; }
        public int    Quantity        { get; set; }
        public decimal Rate           { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal GSTRate        { get; set; }
    }

    public class UpdateInvoiceStatusRequest
    {
        public string Status { get; set; } = "Paid";
    }

    public class CreateProductRequest
    {
        public string SKU           { get; set; } = string.Empty;
        public string ProductName   { get; set; } = string.Empty;
        public string? Category     { get; set; }
        public decimal Rate         { get; set; }
        public decimal GSTRate      { get; set; }
        public int StockQuantity    { get; set; }
        public int LowStockThreshold { get; set; } = 10;
    }

    public class CreateCustomerRequest
    {
        public string Name   { get; set; } = string.Empty;
        public string? GSTIN { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City    { get; set; }
        public string? State   { get; set; }
        public string? PinCode { get; set; }
    }

    public class GSTCalculateRequest
    {
        public decimal Rate            { get; set; }
        public int    Quantity         { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal GSTRate         { get; set; }
        public string GSTType          { get; set; } = "CGST_SGST";
    }

    public class DashboardStats
    {
        public decimal TotalRevenue     { get; set; }
        public int     TotalInvoices    { get; set; }
        public decimal GSTCollected     { get; set; }
        public int     ActiveCustomers  { get; set; }
        public int     LowStockCount    { get; set; }
        public decimal PendingAmount    { get; set; }
        public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
    }

    public class RecentInvoiceDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName  { get; set; } = string.Empty;
        public decimal GrandTotal   { get; set; }
        public decimal TotalGST     { get; set; }
        public string Status        { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
    }

    public class GSTReportDto
    {
        public decimal TotalSales      { get; set; }
        public decimal TotalDiscount   { get; set; }
        public decimal TaxableAmount   { get; set; }
        public decimal TotalCGST       { get; set; }
        public decimal TotalSGST       { get; set; }
        public decimal TotalIGST       { get; set; }
        public decimal TotalGSTPayable { get; set; }
    }
}
