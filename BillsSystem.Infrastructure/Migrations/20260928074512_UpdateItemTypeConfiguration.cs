using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemTypeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemTypes_Companies_CompanyId1",
                table: "ItemTypes");

            migrationBuilder.DropIndex(
                name: "IX_ItemTypes_CompanyId1",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "ItemTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId1",
                table: "ItemTypes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemTypes_CompanyId1",
                table: "ItemTypes",
                column: "CompanyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemTypes_Companies_CompanyId1",
                table: "ItemTypes",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "Id");
        }
    }
}
