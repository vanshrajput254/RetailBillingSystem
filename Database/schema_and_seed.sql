-- ============================================================
--  Retail Billing & Invoice Management System
--  SQL Server Schema + Seed Data
--  Compatible: SQL Server 2019+ / Azure SQL / LocalDB
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RetailBillingDB')
    CREATE DATABASE RetailBillingDB;
GO

USE RetailBillingDB;
GO

-- ─────────────────────────────────────────────────────────────
--  1. Products
-- ─────────────────────────────────────────────────────────────
IF OBJECT_ID('InvoiceItems', 'U') IS NOT NULL DROP TABLE InvoiceItems;
IF OBJECT_ID('Invoices',     'U') IS NOT NULL DROP TABLE Invoices;
IF OBJECT_ID('Customers',    'U') IS NOT NULL DROP TABLE Customers;
IF OBJECT_ID('Products',     'U') IS NOT NULL DROP TABLE Products;
GO

CREATE TABLE Products (
    ProductId          INT            IDENTITY(1,1) PRIMARY KEY,
    SKU                NVARCHAR(20)   NOT NULL,
    ProductName        NVARCHAR(200)  NOT NULL,
    Category           NVARCHAR(100)  NOT NULL DEFAULT '',
    Rate               DECIMAL(18,2)  NOT NULL,
    GSTRate            DECIMAL(5,2)   NOT NULL DEFAULT 18,  -- 0,5,12,18,28
    StockQuantity      INT            NOT NULL DEFAULT 0,
    LowStockThreshold  INT            NOT NULL DEFAULT 10,
    IsActive           BIT            NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt          DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Products_SKU UNIQUE (SKU)
);
GO

-- ─────────────────────────────────────────────────────────────
--  2. Customers
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Customers (
    CustomerId  INT            IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(200)  NOT NULL,
    GSTIN       NVARCHAR(15)   NULL,
    Phone       NVARCHAR(15)   NULL,
    Email       NVARCHAR(200)  NULL,
    Address     NVARCHAR(500)  NULL,
    City        NVARCHAR(100)  NULL,
    State       NVARCHAR(100)  NULL,
    PinCode     NVARCHAR(10)   NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ─────────────────────────────────────────────────────────────
--  3. Invoices
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Invoices (
    InvoiceId      INT            IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber  NVARCHAR(20)   NOT NULL,
    CustomerId     INT            NOT NULL REFERENCES Customers(CustomerId),
    InvoiceDate    DATE           NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    DueDate        DATE           NULL,
    -- 0 = CGST_SGST  |  1 = IGST
    GSTType        INT            NOT NULL DEFAULT 0,
    BuyerGSTIN     NVARCHAR(15)   NULL,
    -- Amounts
    Subtotal       DECIMAL(18,2)  NOT NULL DEFAULT 0,
    TotalDiscount  DECIMAL(18,2)  NOT NULL DEFAULT 0,
    TaxableAmount  DECIMAL(18,2)  NOT NULL DEFAULT 0,
    CGSTAmount     DECIMAL(18,2)  NOT NULL DEFAULT 0,
    SGSTAmount     DECIMAL(18,2)  NOT NULL DEFAULT 0,
    IGSTAmount     DECIMAL(18,2)  NOT NULL DEFAULT 0,
    TotalGST       DECIMAL(18,2)  NOT NULL DEFAULT 0,
    GrandTotal     DECIMAL(18,2)  NOT NULL DEFAULT 0,
    -- 0=Pending 1=Paid 2=Overdue 3=Cancelled
    Status         INT            NOT NULL DEFAULT 0,
    Notes          NVARCHAR(500)  NULL,
    CreatedAt      DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt      DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Invoices_Number UNIQUE (InvoiceNumber)
);
GO

-- ─────────────────────────────────────────────────────────────
--  4. InvoiceItems
-- ─────────────────────────────────────────────────────────────
CREATE TABLE InvoiceItems (
    InvoiceItemId   INT            IDENTITY(1,1) PRIMARY KEY,
    InvoiceId       INT            NOT NULL REFERENCES Invoices(InvoiceId) ON DELETE CASCADE,
    ProductId       INT            NOT NULL REFERENCES Products(ProductId),
    ProductName     NVARCHAR(200)  NOT NULL,
    Rate            DECIMAL(18,2)  NOT NULL,
    Quantity        INT            NOT NULL,
    DiscountPercent DECIMAL(5,2)   NOT NULL DEFAULT 0,
    GSTRate         DECIMAL(5,2)   NOT NULL DEFAULT 18,
    DiscountAmount  DECIMAL(18,2)  NOT NULL DEFAULT 0,
    TaxableAmount   DECIMAL(18,2)  NOT NULL DEFAULT 0,
    GSTAmount       DECIMAL(18,2)  NOT NULL DEFAULT 0,
    LineTotal       DECIMAL(18,2)  NOT NULL DEFAULT 0
);
GO

-- ─────────────────────────────────────────────────────────────
--  5. Indexes for common queries
-- ─────────────────────────────────────────────────────────────
CREATE INDEX IX_Invoices_CustomerId   ON Invoices(CustomerId);
CREATE INDEX IX_Invoices_Status       ON Invoices(Status);
CREATE INDEX IX_Invoices_InvoiceDate  ON Invoices(InvoiceDate);
CREATE INDEX IX_InvoiceItems_Invoice  ON InvoiceItems(InvoiceId);
CREATE INDEX IX_InvoiceItems_Product  ON InvoiceItems(ProductId);
GO

-- ─────────────────────────────────────────────────────────────
--  6. Useful Views
-- ─────────────────────────────────────────────────────────────
CREATE OR ALTER VIEW vw_InvoiceSummary AS
SELECT
    i.InvoiceId,
    i.InvoiceNumber,
    c.Name          AS CustomerName,
    c.GSTIN         AS CustomerGSTIN,
    i.InvoiceDate,
    i.DueDate,
    CASE i.GSTType WHEN 0 THEN 'CGST+SGST' ELSE 'IGST' END AS GSTType,
    i.Subtotal,
    i.TotalDiscount,
    i.TaxableAmount,
    i.CGSTAmount,
    i.SGSTAmount,
    i.IGSTAmount,
    i.TotalGST,
    i.GrandTotal,
    CASE i.Status
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Paid'
        WHEN 2 THEN 'Overdue'
        WHEN 3 THEN 'Cancelled'
    END AS Status
FROM Invoices i
JOIN Customers c ON c.CustomerId = i.CustomerId;
GO

CREATE OR ALTER VIEW vw_MonthlyGSTSummary AS
SELECT
    YEAR(InvoiceDate)  AS [Year],
    MONTH(InvoiceDate) AS [Month],
    COUNT(*)           AS TotalInvoices,
    SUM(Subtotal)      AS TotalSales,
    SUM(TotalDiscount) AS TotalDiscount,
    SUM(TaxableAmount) AS TaxableAmount,
    SUM(CGSTAmount)    AS TotalCGST,
    SUM(SGSTAmount)    AS TotalSGST,
    SUM(IGSTAmount)    AS TotalIGST,
    SUM(TotalGST)      AS TotalGSTPayable,
    SUM(GrandTotal)    AS GrandTotal
FROM Invoices
WHERE Status <> 3   -- exclude Cancelled
GROUP BY YEAR(InvoiceDate), MONTH(InvoiceDate);
GO

CREATE OR ALTER VIEW vw_LowStockProducts AS
SELECT
    ProductId,
    SKU,
    ProductName,
    Category,
    Rate,
    GSTRate,
    StockQuantity,
    LowStockThreshold
FROM Products
WHERE IsActive = 1
  AND StockQuantity <= LowStockThreshold;
GO

-- ─────────────────────────────────────────────────────────────
--  7. Stored Procedures
-- ─────────────────────────────────────────────────────────────

-- Mark overdue invoices automatically
CREATE OR ALTER PROCEDURE sp_MarkOverdueInvoices
AS
BEGIN
    UPDATE Invoices
    SET    Status    = 2,  -- Overdue
           UpdatedAt = SYSUTCDATETIME()
    WHERE  Status   = 0   -- Pending
      AND  DueDate  < CAST(SYSUTCDATETIME() AS DATE);

    SELECT @@ROWCOUNT AS InvoicesMarkedOverdue;
END;
GO

-- Customer ledger summary
CREATE OR ALTER PROCEDURE sp_CustomerLedger
    @CustomerId INT
AS
BEGIN
    SELECT
        i.InvoiceNumber,
        i.InvoiceDate,
        i.GrandTotal,
        CASE i.Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Paid'
                      WHEN 2 THEN 'Overdue' ELSE 'Cancelled' END AS Status,
        i.TotalGST
    FROM Invoices i
    WHERE i.CustomerId = @CustomerId
    ORDER BY i.InvoiceDate DESC;
END;
GO

-- ─────────────────────────────────────────────────────────────
--  8. Seed Data
-- ─────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Products ON;
INSERT INTO Products (ProductId, SKU, ProductName, Category, Rate, GSTRate, StockQuantity, LowStockThreshold)
VALUES
    (1, 'SKU001', 'Basmati Rice 5kg',       'Groceries',     350.00,  5, 150, 20),
    (2, 'SKU002', 'Tata Salt 1kg',           'Groceries',      22.00,  5,   8, 20),
    (3, 'SKU003', 'Surf Excel 1kg',          'Household',     185.00, 18,  60, 10),
    (4, 'SKU004', 'Colgate Toothpaste 200g', 'Personal Care',  95.00, 12,  45, 10),
    (5, 'SKU005', 'Ariel Detergent 2kg',     'Household',     310.00, 18,   5, 10),
    (6, 'SKU006', 'Maggi Noodles 12pk',      'Groceries',     144.00,  5,  80, 15),
    (7, 'SKU007', 'Dove Soap 4pk',           'Personal Care',  96.00, 12,  33, 10),
    (8, 'SKU008', 'Bisleri 1L x12',          'Beverages',      96.00, 18,  25, 10);
SET IDENTITY_INSERT Products OFF;
GO

SET IDENTITY_INSERT Customers ON;
INSERT INTO Customers (CustomerId, Name, GSTIN, Phone, Email, City, State)
VALUES
    (1, 'Ravi Kumar',     '29ASHSS1234R1Z1', '9876543210', 'ravi@example.com',   'Bengaluru', 'Karnataka'),
    (2, 'Meena Traders',  '29BBBCA1557C1Z1', '9123456789', 'meena@traders.com',  'Mysuru',    'Karnataka'),
    (3, 'Sunita Retail',  NULL,              '9000000001', NULL,                 'Hubli',     'Karnataka'),
    (4, 'Arjun Stores',   '29CCCDE2345F1Z5', '9111222333', 'arjun@stores.in',    'Mangaluru', 'Karnataka');
SET IDENTITY_INSERT Customers OFF;
GO

PRINT 'RetailBillingDB schema + seed data created successfully.';
GO
