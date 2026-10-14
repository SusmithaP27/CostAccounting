using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CostAccounting.Services.Reports
{
    public class PdfReportService : IPdfReportService
    {
        private const string HeaderBg = "#1F3864";
        private const string BandBg = "#F2F2F2";
        private const string White = "#FFFFFF";
        private const string GreyText = "#595959";
        private const string RuleGrey = "#BFBFBF";

        static PdfReportService()
        {
            // Check QuestPDF's license terms for your organization.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] Build<T>(ReportDefinition<T> def)
        {
            var runOn = DateTime.Now;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // ---------- Header: title, run date, parameters ----------
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(def.Title).FontSize(18).Bold().FontColor(HeaderBg);
                            row.ConstantItem(200).AlignRight().AlignBottom()
                                .Text($"Run date: {runOn:MM/dd/yyyy h:mm tt}")
                                .FontSize(8).FontColor(GreyText);
                        });

                        col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(HeaderBg);

                        col.Item().PaddingVertical(6).Row(row =>
                        {
                            foreach (var (label, value) in def.Parameters)
                            {
                                row.AutoItem().PaddingRight(20).Text(t =>
                                {
                                    t.Span(label + ": ").SemiBold();
                                    t.Span(value);
                                });
                            }

                            row.AutoItem().Text(t =>
                            {
                                t.Span("Records: ").SemiBold();
                                t.Span(def.Rows.Count.ToString());
                            });
                        });
                    });

                    // ---------- Body: table + total ----------
                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                foreach (var column in def.Columns)
                                {
                                    if (column.Width.HasValue) c.ConstantColumn(column.Width.Value);
                                    else c.RelativeColumn(column.Relative);
                                }
                            });

                            // Column headings (repeat on every page)
                            table.Header(h =>
                            {
                                foreach (var column in def.Columns)
                                {
                                    var cell = h.Cell().Element(HeaderCell);
                                    if (column.AlignRight) cell = cell.AlignRight();
                                    cell.Text(column.Header);
                                }
                            });

                            // Data rows with alternating banding
                            for (int i = 0; i < def.Rows.Count; i++)
                            {
                                var item = def.Rows[i];
                                var bg = i % 2 == 0 ? White : BandBg;

                                foreach (var column in def.Columns)
                                {
                                    var cell = table.Cell().Element(c => BodyCell(c, bg));
                                    if (column.AlignRight) cell = cell.AlignRight();
                                    cell.Text(column.Value(item) ?? "");
                                }
                            }
                        });

                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(HeaderBg);
                        col.Item().PaddingTop(4).Text($"{def.TotalLabel}: {def.Rows.Count}").Bold();
                    });

                    // ---------- Footer: executed by, page x of y, report name ----------
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f).LineColor(RuleGrey);
                        col.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text($"Executed by: {def.RunBy}")
                                .FontSize(8).FontColor(GreyText);

                            row.RelativeItem().AlignCenter().Text(t =>
                            {
                                t.DefaultTextStyle(x => x.FontSize(8).FontColor(GreyText));
                                t.Span("Page ");
                                t.CurrentPageNumber();
                                t.Span(" of ");
                                t.TotalPages();
                            });

                            row.RelativeItem().AlignRight().Text($"Cost Accounting - {def.Title}")
                                .FontSize(8).FontColor(GreyText);
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static IContainer HeaderCell(IContainer c) =>
            c.Background(HeaderBg).Border(0.5f).BorderColor(White)
             .PaddingVertical(5).PaddingHorizontal(4)
             .DefaultTextStyle(x => x.FontColor(White).SemiBold());

        private static IContainer BodyCell(IContainer c, string bg) =>
            c.Background(bg).BorderBottom(0.5f).BorderColor(RuleGrey)
             .PaddingVertical(4).PaddingHorizontal(4);
    }
}