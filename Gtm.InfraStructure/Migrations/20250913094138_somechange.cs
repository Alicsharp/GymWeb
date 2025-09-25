using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gtm.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class somechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscountTitle",
                table: "OrderSellers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "OrderItems",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "OrderDiscounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(355)",
                oldMaxLength: 355);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "OrderDiscounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountTitle",
                table: "OrderSellers");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "OrderDiscounts",
                type: "nvarchar(355)",
                maxLength: 355,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "OrderDiscounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
