using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BillsSystem.Infrastructure.Pdf
{
    public class QuestPdfBillPdfService : IBillPdfService
    {
        static QuestPdfBillPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] Generate(Bill bill, byte[]? logo = null)
        {
            var gross = bill.Items.Sum(i => i.Total);                 // الإجمالي قبل أي خصم
            var itemsDiscount = bill.Items.Sum(i => i.DiscountAmount); // خصومات الأصناف

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(t => t.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, bill, logo));
                    page.Content().PaddingVertical(15).Element(c => ComposeContent(c, bill, gross, itemsDiscount));

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1));
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, Bill bill, byte[]? logo)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("SALES INVOICE").FontSize(20).Bold();
                    col.Item().Text($"Invoice #{bill.Id}").FontSize(12).SemiBold();
                    col.Item().Text($"Date: {bill.BillDate:yyyy/MM/dd}");
                    if (bill.DueDate.HasValue)
                        col.Item().Text($"Due date: {bill.DueDate.Value:yyyy/MM/dd}");
                });

                if (logo != null)
                    row.ConstantItem(90).Image(logo);
            });
        }

        private static void ComposeContent(IContainer container, Bill bill, decimal gross, decimal itemsDiscount)
        {
            container.Column(col =>
            {
                col.Spacing(12);

                // ---------- بيانات العميل ----------
                col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(c =>
                {
                    c.Item().Text("Bill to").FontSize(9).FontColor(Colors.Grey.Darken1);
                    c.Item().Text(bill.Client.Name).Bold().FontSize(12);
                    c.Item().Text($"Phone: {bill.Client.Phone}");
                    if (!string.IsNullOrWhiteSpace(bill.Client.Email))
                        c.Item().Text($"Email: {bill.Client.Email}");
                    if (!string.IsNullOrWhiteSpace(bill.Client.Address))
                        c.Item().Text($"Address: {bill.Client.Address}");
                });

                // ---------- جدول الأصناف ----------
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(35);    // Code
                        cols.RelativeColumn(4);     // Name
                        cols.ConstantColumn(45);    // Unit
                        cols.ConstantColumn(30);    // Qty
                        cols.ConstantColumn(55);    // Price
                        cols.ConstantColumn(65);    // Discount
                        cols.ConstantColumn(60);    // Total
                        cols.ConstantColumn(60);    // Balance
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Code");
                        h.Cell().Element(HeaderCell).Text("Item");
                        h.Cell().Element(HeaderCell).Text("Unit");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Price");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Discount");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Total");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Balance");
                    });

                    foreach (var line in bill.Items)
                    {
                        var discountText = line.DiscountAmount <= 0
                            ? "-"
                            : line.DiscountType == DiscountType.Percentage
                                ? $"{line.Discount:0.##}% ({line.DiscountAmount:0.00})"
                                : line.DiscountAmount.ToString("0.00");

                        table.Cell().Element(BodyCell).Text(line.ItemId.ToString());
                        table.Cell().Element(BodyCell).Text(line.ItemName);
                        table.Cell().Element(BodyCell).Text(line.UnitName);
                        table.Cell().Element(BodyCell).AlignRight().Text(line.Quantity.ToString());
                        table.Cell().Element(BodyCell).AlignRight().Text(line.SellingPrice.ToString("0.00"));
                        table.Cell().Element(BodyCell).AlignRight().Text(discountText);
                        table.Cell().Element(BodyCell).AlignRight().Text(line.Total.ToString("0.00"));
                        table.Cell().Element(BodyCell).AlignRight().Text(line.Balance.ToString("0.00"));
                    }
                });

                // ---------- الملخص ----------
                col.Item().AlignRight().Width(240).Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.ConstantColumn(90);
                    });

                    SummaryRow(t, "Total before discount", gross.ToString("0.00"));
                    if (itemsDiscount > 0)
                        SummaryRow(t, "Items discount", "- " + itemsDiscount.ToString("0.00"));
                    if (bill.ValueDiscount > 0)
                        SummaryRow(t, $"Invoice discount ({bill.PercentageDiscount:0.##}%)", "- " + bill.ValueDiscount.ToString("0.00"));
                    SummaryRow(t, "Net total", bill.TheNet.ToString("0.00"), bold: true);
                    SummaryRow(t, "Paid", bill.PaidUp.ToString("0.00"));
                    SummaryRow(t, "Remaining", bill.TheRest.ToString("0.00"), bold: true);
                });
            });
        }

        private static IContainer HeaderCell(IContainer c) =>
            c.Background(Colors.Grey.Lighten3)
             .BorderBottom(1).BorderColor(Colors.Grey.Medium)
             .Padding(4)
             .DefaultTextStyle(x => x.SemiBold().FontSize(9));

        private static IContainer BodyCell(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
             .Padding(4)
             .DefaultTextStyle(x => x.FontSize(9));

        private static void SummaryRow(TableDescriptor t, string label, string value, bool bold = false)
        {
            t.Cell().Padding(3).Text(label).Style(bold ? TextStyle.Default.Bold() : TextStyle.Default);
            t.Cell().Padding(3).AlignRight().Text(value).Style(bold ? TextStyle.Default.Bold() : TextStyle.Default);
        }
    }
}
