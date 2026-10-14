using System;
using System.Collections.Generic;

namespace CostAccounting.Services.Reports
{
    public class ReportColumn<T>
    {
        public string Header { get; init; } = "";
        public Func<T, string> Value { get; init; } = _ => "";
        public float? Width { get; init; }      // fixed width in points; null = relative width
        public float Relative { get; init; } = 1;
        public bool AlignRight { get; init; }
    }

    public class ReportDefinition<T>
    {
        public string Title { get; init; } = "Report";
        public string RunBy { get; init; } = "";
        public string TotalLabel { get; init; } = "Total Records";
        public List<(string Label, string Value)> Parameters { get; } = new();
        public List<ReportColumn<T>> Columns { get; } = new();
        public IReadOnlyList<T> Rows { get; init; } = Array.Empty<T>();
    }
}