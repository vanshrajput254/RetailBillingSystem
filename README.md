# 🧾 Retail Billing & Invoice Management System
### ASP.NET Core 8 MVC + RESTful API + SQL Server + GST (India)

---

## 📁 Project Structure

```
RetailBillingSystem/
├── Controllers/
│   ├── API/
│   │   ├── InvoicesController.cs      ← Full CRUD, GST calc, stock deduction
│   │   ├── ProductsController.cs      ← Inventory management
│   │   ├── CustomersController.cs     ← Customer management
│   │   └── DashboardController.cs     ← Stats, GST report, live calc
│   ├── HomeController.cs              ← Dashboard MVC
│   ├── InvoiceController.cs           ← Invoices MVC (list, create, print)
│   ├── InventoryController.cs         ← Inventory MVC
│   ├── CustomerController.cs          ← Customers MVC
│   └── ReportController.cs            ← GST Reports MVC
├── Models/
│   ├── Product.cs
│   ├── Customer.cs
│   └── Invoice.cs                     ← Invoice + InvoiceItem + enums
├── DTOs/
│   └── Dtos.cs                        ← All request/response DTOs
├── Services/
│   └── GSTService.cs                  ← Core GST business logic
├── Data/
│   └── AppDbContext.cs                ← EF Core DbContext + seed data
├── Views/
│   ├── Shared/_Layout.cshtml          ← Sidebar layout
│   ├── Home/Index.cshtml              ← Dashboard
│   ├── Invoice/
│   │   ├── Index.cshtml               ← Invoice list
│   │   ├── Create.cshtml              ← New invoice + live GST preview
│   │   ├── Details.cshtml             ← Invoice detail + mark paid
│   │   └── Print.cshtml               ← Print-ready tax invoice
│   ├── Inventory/Index.cshtml
│   ├── Customer/Index.cshtml
│   └── Report/Index.cshtml            ← GST monthly report + CSV export
├── wwwroot/css/site.css
├── Database/
│   └── schema_and_seed.sql            ← Raw SQL script (no EF needed)
├── Program.cs
├── appsettings.json
└── RetailBillingSystem.csproj
```

---

## ⚙️ Setup & Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019+ / Azure SQL / **SQL Server LocalDB** (included with Visual Studio)

### 1. Configure connection string

Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RetailBillingDB;Trusted_Connection=True;"
}
```

For a full SQL Server instance:
```
Server=YOUR_SERVER;Database=RetailBillingDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

### 2. Apply EF Core migrations

```bash
cd RetailBillingSystem
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> ⚠️ The app also calls `db.Database.Migrate()` on startup, so the DB is created automatically.

### 3. OR use the raw SQL script

```sql
-- Run in SQL Server Management Studio (SSMS) or Azure Data Studio:
-- Database/schema_and_seed.sql
```

### 4. Run the application

```bash
dotnet run
```

Open browser: **https://localhost:5001**  
Swagger UI: **https://localhost:5001/swagger**

---

## 🗄️ Database Schema

| Table         | Key Columns                                                                 |
|---------------|-----------------------------------------------------------------------------|
| `Products`    | SKU (unique), ProductName, Rate, GSTRate (0/5/12/18/28), StockQuantity     |
| `Customers`   | Name, GSTIN, Phone, Email, City, State                                      |
| `Invoices`    | InvoiceNumber (unique), CustomerId, GSTType (CGST+SGST / IGST), totals    |
| `InvoiceItems`| InvoiceId, ProductId, Qty, Rate, DiscountPercent, GSTRate, calculated amts |

### Views
| View                    | Purpose                                      |
|-------------------------|----------------------------------------------|
| `vw_InvoiceSummary`     | Invoice + customer joined with status label  |
| `vw_MonthlyGSTSummary`  | Month-wise CGST/SGST/IGST aggregation        |
| `vw_LowStockProducts`   | Products at or below low-stock threshold     |

### Stored Procedures
| Procedure                | Purpose                                      |
|--------------------------|----------------------------------------------|
| `sp_MarkOverdueInvoices` | Auto-marks pending invoices past due date    |
| `sp_CustomerLedger`      | All invoices for a given customer            |

---

## 🔌 REST API Endpoints

### Invoices
| Method | Endpoint                          | Description                        |
|--------|-----------------------------------|------------------------------------|
| GET    | `/api/invoices`                   | List all (filter: status, customer, search) |
| GET    | `/api/invoices/{id}`              | Get invoice detail                 |
| POST   | `/api/invoices`                   | Create invoice (auto-calculates GST, deducts stock) |
| PATCH  | `/api/invoices/{id}/status`       | Update status (Pending/Paid/Overdue/Cancelled) |
| DELETE | `/api/invoices/{id}`              | Delete unpaid invoice              |

### Products
| Method | Endpoint              | Description                        |
|--------|-----------------------|------------------------------------|
| GET    | `/api/products`       | List all (filter: search, lowStock)|
| GET    | `/api/products/{id}`  | Get product                        |
| POST   | `/api/products`       | Create product                     |
| PUT    | `/api/products/{id}`  | Update product                     |
| DELETE | `/api/products/{id}`  | Delete / soft-deactivate           |

### Customers
| Method | Endpoint               | Description                       |
|--------|------------------------|-----------------------------------|
| GET    | `/api/customers`       | List all (filter: search)         |
| GET    | `/api/customers/{id}`  | Get customer                      |
| POST   | `/api/customers`       | Create customer                   |
| PUT    | `/api/customers/{id}`  | Update customer                   |
| DELETE | `/api/customers/{id}`  | Delete / soft-deactivate          |

### Dashboard & Reports
| Method | Endpoint                             | Description                      |
|--------|--------------------------------------|----------------------------------|
| GET    | `/api/dashboard/stats`               | Dashboard KPIs                   |
| GET    | `/api/dashboard/gst-report?month=4&year=2025` | Monthly GST report       |
| POST   | `/api/dashboard/calculate-gst`       | Live GST calculation             |

---

## 🧮 GST Calculation Logic (`GSTService.cs`)

```
Gross Amount    = Rate × Quantity
Discount Amount = Gross × DiscountPercent / 100
Taxable Amount  = Gross - Discount
GST Amount      = Taxable × GSTRate / 100

Intra-state (CGST + SGST):
  CGST = GST / 2
  SGST = GST / 2

Inter-state (IGST):
  IGST = GST

Line Total = Taxable + GST
Grand Total = Σ(Line Totals)
```

Supported GST slabs: **0%, 5%, 12%, 18%, 28%**

---

## ✨ Features

- ✅ Dashboard with revenue, GST, customer, stock KPIs
- ✅ Create GST-compliant invoices with live preview
- ✅ CGST + SGST (intra-state) and IGST (inter-state) support
- ✅ Discount per line item
- ✅ Automatic stock deduction on invoice creation
- ✅ Print-ready Tax Invoice (browser print / PDF)
- ✅ Mark invoices as Paid / Overdue / Cancelled
- ✅ Customer GSTIN management
- ✅ Inventory with low-stock alerts
- ✅ Monthly GST report with GSTR-1 CSV export
- ✅ Full REST API with Swagger documentation
- ✅ Seed data for immediate testing

---

## 🛠️ Tech Stack

| Layer         | Technology                              |
|---------------|-----------------------------------------|
| Backend       | C# 12, ASP.NET Core 8                  |
| Pattern       | MVC + RESTful API                       |
| ORM           | Entity Framework Core 8                 |
| Database      | SQL Server 2019+ / LocalDB              |
| API Docs      | Swagger / Swashbuckle                   |
| Frontend      | Razor Views, Vanilla JS                 |

---

## 📞 Support

For questions or customization, contact your development team.
