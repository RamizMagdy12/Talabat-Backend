using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Talabat.Repository.Data;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
    [DbContext(typeof(StoreContext))]
    [Migration("20261027130000_AddStockMovements")]
    public partial class AddStockMovements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StockAfter = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_OccurredAtUtc",
                table: "StockMovements",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");

            // Historical sales from existing orders
            migrationBuilder.Sql(@"
INSERT INTO StockMovements (ProductId, Type, Quantity, StockAfter, OccurredAtUtc, Note, OrderId, IsDeleted, CreatedDateTime)
SELECT
    oi.Product_ProductId,
    1,
    oi.Quantity,
    0,
    CONVERT(datetime2, o.OrderDate),
    CONCAT(N'Sold on order #', CAST(o.Id AS nvarchar(20))),
    o.Id,
    0,
    SYSUTCDATETIME()
FROM orderItem oi
INNER JOIN orders o ON o.Id = oi.OrderId
WHERE oi.IsDeleted = 0
  AND o.IsDeleted = 0
  AND oi.Product_ProductId IS NOT NULL;
");

            // Opening balance dated before first sale so the ledger reads chronologically
            migrationBuilder.Sql(@"
INSERT INTO StockMovements (ProductId, Type, Quantity, StockAfter, OccurredAtUtc, Note, IsDeleted, CreatedDateTime)
SELECT
    p.Id,
    0,
    p.Stock + ISNULL(sold.SoldQty, 0),
    p.Stock + ISNULL(sold.SoldQty, 0),
    DATEADD(DAY, -1, ISNULL(sold.FirstSoldAt, SYSUTCDATETIME())),
    N'Opening balance',
    0,
    SYSUTCDATETIME()
FROM products p
LEFT JOIN (
    SELECT
        oi.Product_ProductId AS ProductId,
        SUM(oi.Quantity) AS SoldQty,
        MIN(CONVERT(datetime2, o.OrderDate)) AS FirstSoldAt
    FROM orderItem oi
    INNER JOIN orders o ON o.Id = oi.OrderId
    WHERE oi.IsDeleted = 0 AND o.IsDeleted = 0 AND oi.Product_ProductId IS NOT NULL
    GROUP BY oi.Product_ProductId
) sold ON sold.ProductId = p.Id
WHERE p.IsDeleted = 0
  AND (p.Stock + ISNULL(sold.SoldQty, 0)) > 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "StockMovements");
        }
    }
}
