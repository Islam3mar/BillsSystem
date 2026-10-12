using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BillsSystem.Infrastructure.Pdf
{
    public class QuestPdfBillPdfService : IBillPdfService
    {
        // ---------- بيانات الشركة (اللي فاضي مش بيظهر) ----------
        private static readonly string CompanyName = "Pioneers Solutions";
        private static readonly string CompanyAddress = "";
        private static readonly string CompanyPhone = "";
        private static readonly string CompanyEmail = "";

        // ---------- الألوان ----------
        private static readonly Color Brand = Color.FromHex("#1F5F7A");
        private static readonly Color BrandLight = Color.FromHex("#EAF2F6");
        private static readonly Color Green = Color.FromHex("#2E7D32");
        private static readonly Color Red = Color.FromHex("#C62828");
        private static readonly Color Orange = Color.FromHex("#EF6C00");
        private static readonly Color Muted = Colors.Grey.Darken1;
        private static readonly Color BoxBorder = Colors.Grey.Lighten1;

        static QuestPdfBillPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            RegisterEmbeddedFonts();
        }

        // بيسجّل خطوط Cairo المدمجة في الـ DLL عشان أي نص عربي يتعرض صح
        private static void RegisterEmbeddedFonts()
        {
            var assembly = typeof(QuestPdfBillPdfService).Assembly;
            foreach (var name in assembly.GetManifestResourceNames()
                         .Where(n => n.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)))
            {
                var stream = assembly.GetManifestResourceStream(name);
                if (stream != null)
                    QuestPDF.Drawing.FontManager.RegisterFont(stream);
            }
        }

        public byte[] Generate(Bill bill, byte[]? logo = null)
        {
            var gross = bill.Items.Sum(i => i.Total);                  // الإجمالي قبل أي خصم
            var itemsDiscount = bill.Items.Sum(i => i.DiscountAmount); // خصومات الأصناف

            // الدفعات الشغالة بس (الملغية مش بتظهر)
            var payments = bill.Payments
                .Where(x => !x.IsVoided)
                .OrderBy(x => x.PaymentDate)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    // Lato للإنجليزي، وأي حرف مش موجود فيه (عربي) بيتعرض بخط Cairo
                    page.DefaultTextStyle(t => t.FontSize(10)
                        .FontFamily("Lato")
                        .Fallback(f => f.FontFamily("Cairo")));

                    page.Header().Element(c => ComposeHeader(c, logo));
                    page.Content().PaddingVertical(15).Element(c => ComposeContent(c, bill, gross, itemsDiscount, payments));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // =====================================================================
        //  Header
        // =====================================================================
        private static void ComposeHeader(IContainer container, byte[]? logo)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (logo != null)
                        row.ConstantItem(80).Image(logo);

                    row.RelativeItem().PaddingLeft(logo != null ? 12 : 0).Column(info =>
                    {
                        info.Spacing(1);
                        info.Item().Text(CompanyName).FontSize(16).Bold().FontColor(Brand);
                        if (!string.IsNullOrWhiteSpace(CompanyAddress))
                            info.Item().Text(CompanyAddress).FontSize(9).FontColor(Muted);
                        if (!string.IsNullOrWhiteSpace(CompanyPhone))
                            info.Item().Text($"Tel: {CompanyPhone}").FontSize(9).FontColor(Muted);
                        if (!string.IsNullOrWhiteSpace(CompanyEmail))
                            info.Item().Text(CompanyEmail).FontSize(9).FontColor(Muted);
                    });

                    row.ConstantItem(200).AlignRight().Text("SALES INVOICE")
                        .FontSize(22).Bold().FontColor(Brand);
                });

                col.Item().PaddingTop(8).LineHorizontal(2).LineColor(Brand);
            });
        }

        // =====================================================================
        //  Content
        // =====================================================================
        private static void ComposeContent(IContainer container, Bill bill, decimal gross,
            decimal itemsDiscount, List<Payment> payments)
        {
            var (statusText, statusColor) = GetStatus(bill);

            container.Column(col =>
            {
                col.Spacing(14);

                // ---------- بيانات العميل + تفاصيل الفاتورة ----------
                col.Item().Row(top =>
                {
                    top.Spacing(12);

                    // العميل
                    top.RelativeItem().MinHeight(95).Border(0.75f).BorderColor(BoxBorder).Column(clientBox =>
                    {
                        clientBox.Item().Background(BrandLight).PaddingVertical(4).PaddingHorizontal(8)
                            .Text("BILL TO").FontSize(8).Bold().FontColor(Brand);

                        clientBox.Item().Padding(8).Column(cc =>
                        {
                            cc.Spacing(2);
                            cc.Item().Text(bill.Client.Name).Bold().FontSize(12);
                            InfoRow(cc, "Phone", bill.Client.Phone);
                            if (!string.IsNullOrWhiteSpace(bill.Client.Email))
                                InfoRow(cc, "Email", bill.Client.Email);
                            if (!string.IsNullOrWhiteSpace(bill.Client.Address))
                                InfoRow(cc, "Address", bill.Client.Address);
                        });
                    });

                    // تفاصيل الفاتورة
                    top.RelativeItem().MinHeight(95).Border(0.75f).BorderColor(BoxBorder).Column(detailsBox =>
                    {
                        detailsBox.Item().Background(BrandLight).PaddingVertical(4).PaddingHorizontal(8)
                            .Text("INVOICE DETAILS").FontSize(8).Bold().FontColor(Brand);

                        detailsBox.Item().Padding(8).Column(dc =>
                        {
                            dc.Spacing(2);
                            InfoRow(dc, "Invoice No.", $"#{bill.Id}");
                            InfoRow(dc, "Issue date", Date(bill.BillDate));
                            InfoRow(dc, "Due date", bill.DueDate.HasValue ? Date(bill.DueDate.Value) : "-");

                            dc.Item().Row(statusRow =>
                            {
                                statusRow.ConstantItem(60).Text("Status").FontColor(Muted);
                                statusRow.RelativeItem().AlignLeft()
                                    .Background(statusColor)
                                    .PaddingHorizontal(6).PaddingVertical(1)
                                    .Text(statusText).FontSize(8).Bold().FontColor(Colors.White);
                            });
                        });
                    });
                });

                // ---------- جدول الأصناف ----------
                col.Item().Element(c => ComposeItemsTable(c, bill));

                col.Item().Text($"Items: {bill.Items.Count}   |   Total quantity: {bill.Items.Sum(i => i.Quantity).ToString(CultureInfo.InvariantCulture)}")
                    .FontSize(8).FontColor(Muted);

                // ---------- الدفعات + الملخص ----------
                col.Item().ShowEntire().Row(bottom =>
                {
                    bottom.Spacing(15);

                    // الدفعات
                    bottom.RelativeItem().Column(payBox =>
                    {
                        payBox.Item().Background(BrandLight).PaddingVertical(4).PaddingHorizontal(8)
                            .Text("PAYMENTS").FontSize(8).Bold().FontColor(Brand);

                        if (payments.Count == 0)
                        {
                            payBox.Item().PaddingTop(8).PaddingHorizontal(8)
                                .Text("No payments recorded yet.").FontColor(Muted);
                        }
                        else
                        {
                            payBox.Item().Table(pt =>
                            {
                                pt.ColumnsDefinition(pcols =>
                                {
                                    pcols.ConstantColumn(62);   // Date
                                    pcols.ConstantColumn(50);   // Method
                                    pcols.ConstantColumn(62);   // Amount
                                    pcols.RelativeColumn();     // Notes
                                });

                                pt.Header(ph =>
                                {
                                    ph.Cell().Element(HeaderCell).Text("Date");
                                    ph.Cell().Element(HeaderCell).Text("Method");
                                    ph.Cell().Element(HeaderCell).AlignRight().Text("Amount");
                                    ph.Cell().Element(HeaderCell).Text("Notes");
                                });

                                var pIndex = 0;
                                foreach (var pay in payments)
                                {
                                    var palt = pIndex++ % 2 == 1;
                                    pt.Cell().Element(x => BodyCell(x, palt)).Text(Date(pay.PaymentDate));
                                    pt.Cell().Element(x => BodyCell(x, palt)).Text(pay.Method.ToString());
                                    pt.Cell().Element(x => BodyCell(x, palt)).AlignRight().Text(Num(pay.Amount));
                                    pt.Cell().Element(x => BodyCell(x, palt))
                                        .Text(string.IsNullOrWhiteSpace(pay.Notes) ? "-" : pay.Notes);
                                }
                            });
                        }
                    });

                    // الملخص
                    bottom.ConstantItem(235).Border(0.75f).BorderColor(BoxBorder).Table(st =>
                    {
                        st.ColumnsDefinition(scols =>
                        {
                            scols.RelativeColumn();
                            scols.ConstantColumn(90);
                        });

                        SummaryRow(st, "Total before discount", Num(gross));
                        if (itemsDiscount > 0)
                            SummaryRow(st, "Items discount", "- " + Num(itemsDiscount));
                        if (bill.ValueDiscount > 0)
                        {
                            // النسبة بتظهر بس لو المستخدم دخل الخصم كنسبة
                            var discountLabel = bill.DiscountType == DiscountType.Percentage
                                ? $"Invoice discount ({Num(bill.PercentageDiscount, "0.##")}%)"
                                : "Invoice discount";
                            SummaryRow(st, discountLabel, "- " + Num(bill.ValueDiscount));
                        }
                        SummaryRow(st, "Net total", Num(bill.TheNet), bold: true, background: BrandLight);
                        SummaryRow(st, "Paid", Num(bill.PaidUp));
                        SummaryRow(st, "Remaining", Num(bill.TheRest), bold: true,
                            valueColor: bill.TheRest > 0 ? Red : Green);
                    });
                });
            });
        }

        private static void ComposeItemsTable(IContainer container, Bill bill)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(25);    // #
                    cols.RelativeColumn(4);     // Item
                    cols.ConstantColumn(45);    // Unit
                    cols.ConstantColumn(35);    // Qty
                    cols.ConstantColumn(58);    // Price
                    cols.ConstantColumn(72);    // Discount
                    cols.ConstantColumn(62);    // Total
                    cols.ConstantColumn(62);    // Net
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("#");
                    h.Cell().Element(HeaderCell).Text("Item");
                    h.Cell().Element(HeaderCell).Text("Unit");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Price");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Discount");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Total");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Net");
                });

                var index = 0;
                foreach (var line in bill.Items)
                {
                    index++;
                    var alt = index % 2 == 0;

                    var discountText = line.DiscountAmount <= 0
                        ? "-"
                        : line.DiscountType == DiscountType.Percentage
                            ? $"{Num(line.Discount, "0.##")}% ({Num(line.DiscountAmount)})"
                            : Num(line.DiscountAmount);

                    table.Cell().Element(x => BodyCell(x, alt)).Text(index.ToString(CultureInfo.InvariantCulture));

                    table.Cell().Element(x => BodyCell(x, alt)).Column(ic =>
                    {
                        ic.Item().Text(line.ItemName).SemiBold();
                        var sub = BuildSubLine(line);
                        if (sub.Length > 0)
                            ic.Item().Text(sub).FontSize(7.5f).FontColor(Muted);
                    });

                    table.Cell().Element(x => BodyCell(x, alt)).Text(line.UnitName);
                    table.Cell().Element(x => BodyCell(x, alt)).AlignRight().Text(line.Quantity.ToString(CultureInfo.InvariantCulture));
                    table.Cell().Element(x => BodyCell(x, alt)).AlignRight().Text(Num(line.SellingPrice));
                    table.Cell().Element(x => BodyCell(x, alt)).AlignRight().Text(discountText);
                    table.Cell().Element(x => BodyCell(x, alt)).AlignRight().Text(Num(line.Total));
                    table.Cell().Element(x => BodyCell(x, alt)).AlignRight().Text(Num(line.Balance));
                }
            });
        }

        // =====================================================================
        //  Footer
        // =====================================================================
        private static void ComposeFooter(IContainer container)
        {
            container.Column(f =>
            {
                f.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                f.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Thank you for your business").FontSize(8).FontColor(Muted);
                    r.RelativeItem().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(8).FontColor(Muted));
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            });
        }

        // =====================================================================
        //  Helpers
        // =====================================================================
        private static (string Text, Color Color) GetStatus(Bill bill)
        {
            if (bill.TheRest <= 0) return ("PAID", Green);
            if (bill.DueDate.HasValue && bill.DueDate.Value.Date < AppClock.Today) return ("OVERDUE", Red);
            if (bill.PaidUp > 0) return ("PARTIALLY PAID", Orange);
            return ("UNPAID", Colors.Grey.Darken2);
        }

        // "النوع · الشركة" تحت اسم الصنف (اللي فاضي بيتشال)
        private static string BuildSubLine(BillItem line)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(line.TypeName)) parts.Add(line.TypeName);
            if (!string.IsNullOrWhiteSpace(line.CompanyName)) parts.Add(line.CompanyName);
            return string.Join(" · ", parts);
        }

        private static void InfoRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(60).Text(label).FontColor(Muted);
                r.RelativeItem().Text(value);
            });
        }

        // ---------- تنسيق ثابت مهما كان culture السيرفر ----------
        private static string Num(decimal value, string format = "0.00")
            => value.ToString(format, CultureInfo.InvariantCulture);

        private static string Date(DateTime value)
            => value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

        private static IContainer HeaderCell(IContainer c) =>
            c.Background(Brand)
             .PaddingVertical(5).PaddingHorizontal(4)
             .DefaultTextStyle(x => x.SemiBold().FontSize(9).FontColor(Colors.White));

        private static IContainer BodyCell(IContainer c, bool alt = false) =>
            c.Background(alt ? Colors.Grey.Lighten5 : Colors.White)
             .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
             .PaddingVertical(5).PaddingHorizontal(4)
             .DefaultTextStyle(x => x.FontSize(9));

        private static void SummaryRow(TableDescriptor t, string label, string value,
            bool bold = false, Color? background = null, Color? valueColor = null)
        {
            var style = bold ? TextStyle.Default.Bold() : TextStyle.Default;
            var valueStyle = valueColor.HasValue ? style.FontColor(valueColor.Value) : style;

            var labelCell = t.Cell().Element(x => background.HasValue ? x.Background(background.Value) : x);
            labelCell.PaddingVertical(4).PaddingHorizontal(8).Text(label).Style(style);

            var valueCell = t.Cell().Element(x => background.HasValue ? x.Background(background.Value) : x);
            valueCell.PaddingVertical(4).PaddingHorizontal(8).AlignRight().Text(value).Style(valueStyle);
        }
    }
}
