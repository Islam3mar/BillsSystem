using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBillsAndBillItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountType",
                table: "Bills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Bills");
        }
    }
}
