using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderAttemptAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderAttemptAt",
                table: "Bills",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReminderAttemptAt",
                table: "Bills");
        }
    }
}
