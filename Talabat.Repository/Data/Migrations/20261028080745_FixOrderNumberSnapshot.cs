using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
	/// <inheritdoc />
	public partial class FixOrderNumberSnapshot : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Snapshot-only sync for localized catalog columns already applied via earlier SQL migrations.
			migrationBuilder.Sql(@"
IF COL_LENGTH('products', 'NameAr') IS NULL
    ALTER TABLE products ADD NameAr nvarchar(100) NULL;
IF COL_LENGTH('products', 'DescriptionAr') IS NULL
    ALTER TABLE products ADD DescriptionAr nvarchar(max) NULL;
IF COL_LENGTH('productBrands', 'NameAr') IS NULL
    ALTER TABLE productBrands ADD NameAr nvarchar(100) NULL;
IF COL_LENGTH('productCategories', 'NameAr') IS NULL
    ALTER TABLE productCategories ADD NameAr nvarchar(100) NULL;
");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
		}
	}
}
