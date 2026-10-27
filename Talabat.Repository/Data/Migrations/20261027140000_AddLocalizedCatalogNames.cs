using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Talabat.Repository.Data;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
    [DbContext(typeof(StoreContext))]
    [Migration("20261027140000_AddLocalizedCatalogNames")]
    public partial class AddLocalizedCatalogNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('products', 'DescriptionAr') IS NOT NULL
    ALTER TABLE products DROP COLUMN DescriptionAr;
IF COL_LENGTH('products', 'NameAr') IS NOT NULL
    ALTER TABLE products DROP COLUMN NameAr;
IF COL_LENGTH('productBrands', 'NameAr') IS NOT NULL
    ALTER TABLE productBrands DROP COLUMN NameAr;
IF COL_LENGTH('productCategories', 'NameAr') IS NOT NULL
    ALTER TABLE productCategories DROP COLUMN NameAr;
");
        }
    }
}
