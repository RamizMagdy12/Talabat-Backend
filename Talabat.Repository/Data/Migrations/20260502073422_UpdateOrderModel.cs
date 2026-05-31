using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_DeliveryMethod_DeliveryMethodId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Order",
                table: "Order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryMethod",
                table: "DeliveryMethod");

            migrationBuilder.RenameTable(
                name: "OrderItem",
                newName: "orderItem");

            migrationBuilder.RenameTable(
                name: "Order",
                newName: "orders");

            migrationBuilder.RenameTable(
                name: "DeliveryMethod",
                newName: "deliveryMethods");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_OrderId",
                table: "orderItem",
                newName: "IX_orderItem_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_DeliveryMethodId",
                table: "orders",
                newName: "IX_orders_DeliveryMethodId");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryMethodId",
                table: "orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_orderItem",
                table: "orderItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orders",
                table: "orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_deliveryMethods",
                table: "deliveryMethods",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_orderItem_orders_OrderId",
                table: "orderItem",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_deliveryMethods_DeliveryMethodId",
                table: "orders",
                column: "DeliveryMethodId",
                principalTable: "deliveryMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orderItem_orders_OrderId",
                table: "orderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_deliveryMethods_DeliveryMethodId",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orderItem",
                table: "orderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orders",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_deliveryMethods",
                table: "deliveryMethods");

            migrationBuilder.RenameTable(
                name: "orderItem",
                newName: "OrderItem");

            migrationBuilder.RenameTable(
                name: "orders",
                newName: "Order");

            migrationBuilder.RenameTable(
                name: "deliveryMethods",
                newName: "DeliveryMethod");

            migrationBuilder.RenameIndex(
                name: "IX_orderItem_OrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_orders_DeliveryMethodId",
                table: "Order",
                newName: "IX_Order_DeliveryMethodId");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryMethodId",
                table: "Order",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order",
                table: "Order",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryMethod",
                table: "DeliveryMethod",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_DeliveryMethod_DeliveryMethodId",
                table: "Order",
                column: "DeliveryMethodId",
                principalTable: "DeliveryMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id");
        }
    }
}
