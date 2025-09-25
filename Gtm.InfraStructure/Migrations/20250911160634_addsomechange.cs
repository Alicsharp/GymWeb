using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gtm.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class addsomechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Stores");
        }
    }
}
