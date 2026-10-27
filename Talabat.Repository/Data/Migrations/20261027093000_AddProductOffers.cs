using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Talabat.Repository.Data;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
    [DbContext(typeof(StoreContext))]
    [Migration("20261027093000_AddProductOffers")]
    public partial class AddProductOffers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OfferPrice",
                table: "products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferStartUtc",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferEndUtc",
                table: "products",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "OfferPrice", table: "products");
            migrationBuilder.DropColumn(name: "OfferStartUtc", table: "products");
            migrationBuilder.DropColumn(name: "OfferEndUtc", table: "products");
        }
    }
}
