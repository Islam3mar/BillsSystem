using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BillsHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_ItemTypes_ItemTypeId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemTypes_Companies_CompanyId",
                table: "ItemTypes");

            migrationBuilder.AddColumn<int>(
                name: "QuantityInStock",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Items",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<decimal>(
                name: "PercentageDiscount",
                table: "Bills",
                type: "decimal(7,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BillDate",
                table: "Bills",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Bills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Bills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Bills",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionId",
                table: "Bills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "BillItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BuyingPrice",
                table: "BillItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "BillItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "BillItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "BillItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "BillItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TypeName",
                table: "BillItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "BillItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.Sql(@"
   UPDATE BillItems SET
       Total = ROUND(SellingPrice * Quantity, 2),
       DiscountAmount = ROUND(CASE WHEN DiscountType = 1
                                   THEN SellingPrice * Quantity * Discount / 100.0
                                   ELSE Discount END, 2);");
            migrationBuilder.Sql("UPDATE BillItems SET Balance = Total - DiscountAmount;");

            // Snapshot للفواتير القديمة (اسم الصنف/النوع/الشركة/الوحدة + سعر الشراء الحالي)
            migrationBuilder.Sql(@"
   UPDATE bi SET bi.ItemName = i.Name, bi.TypeName = t.Name, bi.CompanyName = c.Name,
                 bi.UnitName = u.Name, bi.BuyingPrice = i.BuyingPrice
   FROM BillItems bi
   JOIN Items i ON i.Id = bi.ItemId
   JOIN ItemTypes t ON t.Id = i.ItemTypeId
   JOIN Companies c ON c.Id = t.CompanyId
   JOIN Units u ON u.Id = i.UnitId;");

            // أول دفعة لكل فاتورة قديمة مدفوع منها حاجة
            migrationBuilder.Sql(@"
   INSERT INTO Payments (BillId, Amount, PaymentDate, Notes, CreatedAt)
   SELECT Id, PaidUp, BillDate, N'Initial payment', CreatedAt FROM Bills WHERE PaidUp > 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_BillDate",
                table: "Bills",
                column: "BillDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_SubmissionId",
                table: "Bills",
                column: "SubmissionId",
                unique: true,
                filter: "[SubmissionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BillId",
                table: "Payments",
                column: "BillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_ItemTypes_ItemTypeId",
                table: "Categories",
                column: "ItemTypeId",
                principalTable: "ItemTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemTypes_Companies_CompanyId",
                table: "ItemTypes",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_ItemTypes_ItemTypeId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemTypes_Companies_CompanyId",
                table: "ItemTypes");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Bills_BillDate",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_SubmissionId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "QuantityInStock",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "SubmissionId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "BuyingPrice",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "TypeName",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "BillItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "PercentageDiscount",
                table: "Bills",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,4)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BillDate",
                table: "Bills",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_ItemTypes_ItemTypeId",
                table: "Categories",
                column: "ItemTypeId",
                principalTable: "ItemTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemTypes_Companies_CompanyId",
                table: "ItemTypes",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
