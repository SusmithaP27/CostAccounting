namespace CostAccounting.Services.Reports
{
    public interface IPdfReportService
    {
        /// <summary>
        /// Builds a PDF report (title, parameters, column headings, rows, total, footer)
        /// from a report definition and returns the PDF bytes.
        /// </summary>
        byte[] Build<T>(ReportDefinition<T> def);
    }
}