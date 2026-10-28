using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Talabat.Repository.Data;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
	[DbContext(typeof(StoreContext))]
	[Migration("20261028114500_AddOrderNumber")]
	public partial class AddOrderNumber : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Separate batches via dynamic SQL so the new column is visible to UPDATE.
			migrationBuilder.Sql(@"
IF COL_LENGTH('orders', 'OrderNumber') IS NULL
    ALTER TABLE orders ADD OrderNumber nvarchar(32) NULL;

EXEC(N'
UPDATE orders
SET OrderNumber = CONCAT(''TLB-'', CONVERT(varchar(6), OrderDate, 12), ''-'', RIGHT(''0000'' + CAST(Id AS varchar(10)), 4))
WHERE OrderNumber IS NULL OR OrderNumber = '''';
');

EXEC(N'ALTER TABLE orders ALTER COLUMN OrderNumber nvarchar(32) NOT NULL');

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_OrderNumber' AND object_id = OBJECT_ID('orders')
)
    CREATE UNIQUE INDEX IX_Orders_OrderNumber ON orders(OrderNumber);
");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_OrderNumber' AND object_id = OBJECT_ID('orders')
)
    DROP INDEX IX_Orders_OrderNumber ON orders;

IF COL_LENGTH('orders', 'OrderNumber') IS NOT NULL
    ALTER TABLE orders DROP COLUMN OrderNumber;
");
		}
	}
}
