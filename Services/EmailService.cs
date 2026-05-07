using System.Net;
using System.Net.Mail;
using RetailBillingSystem.Models;

namespace RetailBillingSystem.Services
{
    public interface IEmailService
    {
        Task<bool> SendInvoiceEmailAsync(Invoice invoice, string toEmail);
        Task<bool> SendTestEmailAsync(string toEmail);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task<bool> SendInvoiceEmailAsync(Invoice invoice, string toEmail)
        {
            try
            {
                var subject = $"Invoice {invoice.InvoiceNumber} from RetailBill Pro";
                var body = BuildInvoiceEmailBody(invoice);
                return await SendEmailAsync(toEmail, subject, body);
            }
            catch { return false; }
        }

        public async Task<bool> SendTestEmailAsync(string toEmail)
        {
            return await SendEmailAsync(toEmail, "RetailBill Pro — Test Email",
                "<h2>✅ Email configuration is working!</h2><p>RetailBill Pro email service is configured correctly.</p>");
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
            var smtpUser = _config["EmailSettings:SmtpUser"] ?? "";
            var smtpPass = _config["EmailSettings:SmtpPass"] ?? "";
            var fromEmail = _config["EmailSettings:FromEmail"] ?? smtpUser;
            var fromName = _config["EmailSettings:FromName"] ?? "RetailBill Pro";

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                return false;

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            var msg = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);

            await client.SendMailAsync(msg);
            return true;
        }

        private string BuildInvoiceEmailBody(Invoice invoice)
        {
            var itemRows = string.Join("", invoice.Items?.Select(i =>
                $@"<tr>
                    <td style='padding:8px;border-bottom:1px solid #eee;'>{i.ProductName}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:center;'>{i.Quantity}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>₹{i.Rate:N2}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>{i.GSTRate}%</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>₹{i.LineTotal:N2}</td>
                   </tr>") ?? Enumerable.Empty<string>());

            return $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif;color:#222;max-width:600px;margin:auto;'>
  <div style='background:#185FA5;padding:24px;border-radius:8px 8px 0 0;'>
    <h1 style='color:#fff;margin:0;font-size:22px;'>🧾 RetailBill Pro</h1>
    <p style='color:rgba(255,255,255,.8);margin:4px 0 0;'>Tax Invoice</p>
  </div>
  <div style='background:#fff;padding:24px;border:1px solid #e0e0e0;'>
    <table style='width:100%;margin-bottom:20px;'>
      <tr>
        <td><strong>Invoice #:</strong> {invoice.InvoiceNumber}</td>
        <td style='text-align:right;'><strong>Date:</strong> {invoice.InvoiceDate:dd MMM yyyy}</td>
      </tr>
      <tr>
        <td><strong>Customer:</strong> {invoice.Customer?.Name}</td>
        <td style='text-align:right;'><strong>Status:</strong> {invoice.Status}</td>
      </tr>
    </table>

    <table style='width:100%;border-collapse:collapse;'>
      <thead>
        <tr style='background:#f5f7fa;'>
          <th style='padding:8px;text-align:left;'>Product</th>
          <th style='padding:8px;text-align:center;'>Qty</th>
          <th style='padding:8px;text-align:right;'>Rate</th>
          <th style='padding:8px;text-align:right;'>GST</th>
          <th style='padding:8px;text-align:right;'>Total</th>
        </tr>
      </thead>
      <tbody>{itemRows}</tbody>
    </table>

    <div style='text-align:right;margin-top:16px;'>
      <table style='display:inline-table;min-width:220px;'>
        <tr><td>Subtotal</td><td style='text-align:right;'>₹{invoice.Subtotal:N2}</td></tr>
        <tr><td>Discount</td><td style='text-align:right;'>−₹{invoice.TotalDiscount:N2}</td></tr>
        <tr><td>GST</td><td style='text-align:right;'>₹{invoice.TotalGST:N2}</td></tr>
        <tr style='font-weight:bold;font-size:16px;border-top:2px solid #333;'>
          <td>Grand Total</td><td style='text-align:right;'>₹{invoice.GrandTotal:N2}</td>
        </tr>
      </table>
    </div>
  </div>
  <div style='background:#f5f5f5;padding:12px 24px;border-radius:0 0 8px 8px;font-size:11px;color:#888;text-align:center;'>
    This is a computer-generated invoice. | RetailBill Pro
  </div>
</body>
</html>";
        }
    }
}
