using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Infrastructure.Assets;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Infrastructure.Services
{
    /// <summary>
    /// Renders an Invoice as a branded PDF (logo, brand accent colour, line items,
    /// totals) using QuestPDF
    /// </summary>
    public class InvoicePdfService : IInvoicePdfService
    {
        private const string AccentHex = "#12D3DE";
        private const string TextHex = "#111827";
        private const string MutedHex = "#6B7280";
        private const string BorderHex = "#E5E7EB";
        private const string SurfaceHex = "#F8FAFC";

        public byte[] Generate(Invoice invoice)
        {
            var logo = LogoAsset.GetBytes();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextHex));

                    // ---------- header ----------
                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().Row(row =>
                        {
                            row.ConstantItem(64).Height(54).Image(logo).FitArea();

                            row.RelativeItem().PaddingLeft(12).Column(col =>
                            {
                                col.Item().Text("Delgender Communications").FontSize(15).Bold();
                                col.Item().Text("Strategic Communications & Brand Consultancy").FontSize(9).FontColor(MutedHex);
                                col.Item().Text("https://delgendercommunications.co.za").FontSize(9).FontColor(MutedHex);
                                col.Item().Text("delgendercommunications@gmail.com").FontSize(9).FontColor(MutedHex);
                            });

                            row.ConstantItem(160).Column(col =>
                            {
                                col.Item().AlignRight().Text("INVOICE").FontSize(20).Bold().FontColor(AccentHex);
                                col.Item().AlignRight().Text(invoice.InvoiceNumber).FontSize(11).Bold();
                            });
                        });

                        headerCol.Item().PaddingTop(14).LineHorizontal(1).LineColor(BorderHex);
                    });

                    // ---------- content ----------
                    page.Content().PaddingTop(20).Column(col =>
                    {
                        col.Spacing(18);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(bill =>
                            {
                                bill.Item().Text("Bill to").FontSize(9).Bold().FontColor(MutedHex);
                                bill.Item().PaddingTop(4).Text(invoice.Customer.FullName).FontSize(11).Bold();
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.CompanyName))
                                    bill.Item().Text(invoice.Customer.CompanyName).FontSize(10);
                                bill.Item().Text(invoice.Customer.Email).FontSize(10).FontColor(MutedHex);
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.PhoneNumber))
                                    bill.Item().Text(invoice.Customer.PhoneNumber).FontSize(10).FontColor(MutedHex);
                            });

                            row.ConstantItem(200).Column(meta =>
                            {
                                meta.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Issue date").FontSize(9.5f).FontColor(MutedHex);
                                    r.RelativeItem().AlignRight().Text(invoice.IssueDate.ToString("d MMMM yyyy")).FontSize(9.5f).Bold();
                                });
                                meta.Item().PaddingTop(3).Row(r =>
                                {
                                    r.RelativeItem().Text("Due date").FontSize(9.5f).FontColor(MutedHex);
                                    r.RelativeItem().AlignRight().Text(invoice.DueDate.ToString("d MMMM yyyy")).FontSize(9.5f).Bold();
                                });
                                if (invoice.PaidAt is not null)
                                {
                                    meta.Item().PaddingTop(3).Row(r =>
                                    {
                                        r.RelativeItem().Text("Paid").FontSize(9.5f).FontColor(MutedHex);
                                        r.RelativeItem().AlignRight().Text(invoice.PaidAt.Value.ToString("d MMMM yyyy")).FontSize(9.5f).Bold();
                                    });
                                }
                            });
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.4f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.4f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Description").FontSize(9).Bold().FontColor(MutedHex);
                                header.Cell().Text("Qty").FontSize(9).Bold().FontColor(MutedHex);
                                header.Cell().AlignRight().Text("Unit price").FontSize(9).Bold().FontColor(MutedHex);
                                header.Cell().AlignRight().Text("Tax").FontSize(9).Bold().FontColor(MutedHex);
                                header.Cell().AlignRight().Text("Amount").FontSize(9).Bold().FontColor(MutedHex);

                                header.Cell().ColumnSpan(5).PaddingTop(4).BorderBottom(1.5f).BorderColor(AccentHex);
                            });

                            foreach (var item in invoice.Items)
                            {
                                table.Cell().PaddingTop(8).BorderBottom(1).BorderColor(BorderHex).PaddingBottom(10)
                                    .Text(item.Description).FontSize(9.5f);
                                table.Cell().PaddingTop(8).BorderBottom(1).BorderColor(BorderHex).PaddingBottom(10)
                                    .Text(item.Quantity.ToString("0.##")).FontSize(9.5f);
                                table.Cell().PaddingTop(8).BorderBottom(1).BorderColor(BorderHex).PaddingBottom(10)
                                    .AlignRight().Text($"R{item.UnitPrice:N2}").FontSize(9.5f);
                                table.Cell().PaddingTop(8).BorderBottom(1).BorderColor(BorderHex).PaddingBottom(10)
                                    .AlignRight().Text($"{item.TaxRate:0.#}%").FontSize(9.5f);
                                table.Cell().PaddingTop(8).BorderBottom(1).BorderColor(BorderHex).PaddingBottom(10)
                                    .AlignRight().Text($"R{item.TotalAmount:N2}").FontSize(9.5f).Bold();
                            }
                        });

                        col.Item().AlignRight().Width(240).Column(totals =>
                        {
                            totals.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Subtotal").FontSize(10).FontColor(MutedHex);
                                r.ConstantItem(110).AlignRight().Text($"R{invoice.Subtotal:N2}").FontSize(10);
                            });
                            totals.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem().Text("Discount").FontSize(10).FontColor(MutedHex);
                                r.ConstantItem(110).AlignRight().Text($"-R{invoice.DiscountAmount:N2}").FontSize(10);
                            });
                            totals.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem().Text("Tax").FontSize(10).FontColor(MutedHex);
                                r.ConstantItem(110).AlignRight().Text($"R{invoice.TaxAmount:N2}").FontSize(10);
                            });
                            totals.Item().PaddingTop(6).LineHorizontal(1).LineColor(BorderHex);
                            totals.Item().PaddingTop(6).Row(r =>
                            {
                                r.RelativeItem().Text("Total due").FontSize(12).Bold();
                                r.ConstantItem(110).AlignRight().Text($"R{invoice.TotalAmount:N2}").FontSize(14).Bold().FontColor(AccentHex);
                            });
                        });

                        if (!string.IsNullOrWhiteSpace(invoice.Notes))
                        {
                            col.Item().Background(SurfaceHex).Padding(12).Column(notes =>
                            {
                                notes.Item().Text("Notes").FontSize(9).Bold().FontColor(MutedHex);
                                notes.Item().PaddingTop(4).Text(invoice.Notes).FontSize(9.5f);
                            });
                        }

                        if (invoice.Status == InvoiceStatus.Paid && !string.IsNullOrWhiteSpace(invoice.PaymentReference))
                        {
                            col.Item().Text($"Payment reference: {invoice.PaymentReference}").FontSize(9).FontColor(MutedHex);
                        }
                    });

                    // ---------- footer ----------
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(BorderHex);
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Text("Please do not reply to this email address directly for billing queries.")
                                .FontSize(8).FontColor(MutedHex);
                            row.ConstantItem(160).AlignRight().Text($"© {DateTime.UtcNow.Year} Delgender Communications")
                                .FontSize(8).FontColor(MutedHex);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
