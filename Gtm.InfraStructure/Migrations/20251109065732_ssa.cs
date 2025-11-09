using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gtm.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class ssa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderAddressId",
                table: "Orders",
                column: "OrderAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderAddresses_OrderAddressId",
                table: "Orders",
                column: "OrderAddressId",
                principalTable: "OrderAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderAddresses_OrderAddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderAddressId",
                table: "Orders");
        }
    }
}
