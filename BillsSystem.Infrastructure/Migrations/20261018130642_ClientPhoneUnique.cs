using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClientPhoneUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clients_Phone",
                table: "Clients",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Phone",
                table: "Clients");
        }
    }
}
