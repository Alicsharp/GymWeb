using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gtm.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class addsomechangeinmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PostTitle",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "OrderSellers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PostTitle",
                table: "OrderSellers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostId",
                table: "OrderSellers");

            migrationBuilder.DropColumn(
                name: "PostTitle",
                table: "OrderSellers");

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PostTitle",
                table: "Orders",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);
        }
    }
}
