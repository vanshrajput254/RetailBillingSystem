using RetailBillingSystem.Models;
using RetailBillingSystem.DTOs;

namespace RetailBillingSystem.Services
{
    /// <summary>
    /// Core GST business logic for Indian retail billing.
    /// Supports CGST+SGST (intra-state) and IGST (inter-state).
    /// </summary>
    public class GSTService : IGSTService
    {
        /// <summary>
        /// Calculate all tax values for a single invoice line item.
        /// </summary>
        public InvoiceItemCalculation CalculateLineItem(
            decimal rate, int quantity, decimal discountPercent, decimal gstRate, GSTType gstType)
        {
            decimal grossAmount    = rate * quantity;
            decimal discountAmount = Math.Round(grossAmount * discountPercent / 100, 2);
            decimal taxableAmount  = grossAmount - discountAmount;
            decimal gstAmount      = Math.Round(taxableAmount * gstRate / 100, 2);
            decimal lineTotal      = taxableAmount + gstAmount;

            decimal cgst = 0, sgst = 0, igst = 0;
            if (gstType == GSTType.CGST_SGST)
            {
                cgst = Math.Round(gstAmount / 2, 2);
                sgst = gstAmount - cgst;            // remainder avoids rounding gap
            }
            else
            {
                igst = gstAmount;
            }

            return new InvoiceItemCalculation
            {
                GrossAmount    = grossAmount,
                DiscountAmount = discountAmount,
                TaxableAmount  = taxableAmount,
                GSTAmount      = gstAmount,
                CGSTAmount     = cgst,
                SGSTAmount     = sgst,
                IGSTAmount     = igst,
                LineTotal      = lineTotal
            };
        }

        /// <summary>
        /// Aggregate totals across all line items to produce invoice-level figures.
        /// </summary>
        public InvoiceTotals CalculateInvoiceTotals(
            IEnumerable<InvoiceItemCalculation> items, GSTType gstType)
        {
            var list      = items.ToList();
            decimal sub   = list.Sum(i => i.GrossAmount);
            decimal disc  = list.Sum(i => i.DiscountAmount);
            decimal taxable = list.Sum(i => i.TaxableAmount);
            decimal cgst  = list.Sum(i => i.CGSTAmount);
            decimal sgst  = list.Sum(i => i.SGSTAmount);
            decimal igst  = list.Sum(i => i.IGSTAmount);
            decimal total = list.Sum(i => i.LineTotal);

            return new InvoiceTotals
            {
                Subtotal       = Math.Round(sub,     2),
                TotalDiscount  = Math.Round(disc,    2),
                TaxableAmount  = Math.Round(taxable, 2),
                CGSTAmount     = Math.Round(cgst,    2),
                SGSTAmount     = Math.Round(sgst,    2),
                IGSTAmount     = Math.Round(igst,    2),
                TotalGST       = Math.Round(cgst + sgst + igst, 2),
                GrandTotal     = Math.Round(total,   2)
            };
        }

        /// <summary>
        /// Generate a unique invoice number: INV-YYYYMM-XXXX
        /// </summary>
        public string GenerateInvoiceNumber(int sequenceNumber)
        {
            string ym  = DateTime.Now.ToString("yyyyMM");
            string seq = sequenceNumber.ToString("D4");
            return $"INV-{ym}-{seq}";
        }
    }

    public interface IGSTService
    {
        InvoiceItemCalculation CalculateLineItem(
            decimal rate, int quantity, decimal discountPercent, decimal gstRate, GSTType gstType);
        InvoiceTotals CalculateInvoiceTotals(
            IEnumerable<InvoiceItemCalculation> items, GSTType gstType);
        string GenerateInvoiceNumber(int sequenceNumber);
    }
}
