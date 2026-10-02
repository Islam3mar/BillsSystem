using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientMaxCreditLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxCreditLimit",
                table: "Clients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxCreditLimit",
                table: "Clients");
        }
    }
}
