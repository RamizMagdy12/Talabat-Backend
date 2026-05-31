using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedById",
                table: "products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByName",
                table: "products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDateTime",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "productCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "productCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                table: "productCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "productCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "productCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedById",
                table: "productCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByName",
                table: "productCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDateTime",
                table: "productCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "productBrands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "productBrands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                table: "productBrands",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "productBrands",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "productBrands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedById",
                table: "productBrands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByName",
                table: "productBrands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDateTime",
                table: "productBrands",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "products");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "products");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "products");

            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "products");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "products");

            migrationBuilder.DropColumn(
                name: "LastModifiedByName",
                table: "products");

            migrationBuilder.DropColumn(
                name: "LastModifiedDateTime",
                table: "products");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "LastModifiedByName",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "LastModifiedDateTime",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "productBrands");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "productBrands");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "productBrands");

            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "productBrands");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "productBrands");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "productBrands");

            migrationBuilder.DropColumn(
                name: "LastModifiedByName",
                table: "productBrands");

            migrationBuilder.DropColumn(
                name: "LastModifiedDateTime",
                table: "productBrands");
        }
    }
}
