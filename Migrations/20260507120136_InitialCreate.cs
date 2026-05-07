using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RetailBillingSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GSTIN = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SKU = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GSTRate = table.Column<decimal>(type: "numeric", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GSTType = table.Column<int>(type: "integer", nullable: false),
                    BuyerGSTIN = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalGST = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    InvoiceItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    GSTRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GSTAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.InvoiceItemId);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "UserId", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3720), "Store Admin", true, "GHVQ3kffbwHsr+Nln/uxAZ75bXZpTR06zEi8m96TMBg=", "Admin", "admin" });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Address", "City", "CreatedAt", "Email", "GSTIN", "IsActive", "Name", "Phone", "PinCode", "State" },
                values: new object[,]
                {
                    { 1, null, "Bengaluru", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3680), "ravi@example.com", "29ASHSS1234R1Z1", true, "Ravi Kumar", "9876543210", null, "Karnataka" },
                    { 2, null, "Mysuru", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3688), "meena@traders.com", "29BBBCA1557C1Z1", true, "Meena Traders", "9123456789", null, "Karnataka" },
                    { 3, null, "Hubli", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3690), null, null, true, "Sunita Retail", "9000000001", null, "Karnataka" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Category", "CreatedAt", "GSTRate", "IsActive", "LowStockThreshold", "ProductName", "Rate", "SKU", "StockQuantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Groceries", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3248), 5m, true, 10, "Basmati Rice 5kg", 350m, "SKU001", 150, new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3251) },
                    { 2, "Groceries", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3260), 5m, true, 10, "Tata Salt 1kg", 22m, "SKU002", 8, new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3261) },
                    { 3, "Household", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3264), 18m, true, 10, "Surf Excel 1kg", 185m, "SKU003", 60, new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3264) },
                    { 4, "Personal Care", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3266), 12m, true, 10, "Colgate Toothpaste", 95m, "SKU004", 45, new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3266) },
                    { 5, "Household", new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3269), 18m, true, 10, "Ariel Detergent 2kg", 310m, "SKU005", 5, new DateTime(2026, 5, 7, 12, 1, 36, 218, DateTimeKind.Utc).AddTicks(3270) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ProductId",
                table: "InvoiceItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
