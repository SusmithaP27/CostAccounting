namespace CostAccounting.Models
{
    public class OperationVM
    {
        public List<Operation> Operations { get; set; } = new();
        public string? SearchTerm { get; set; }
        public bool? FilterActive { get; set; }
        public bool? FilterEngineering { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
        public string SortColumn { get; set; } = "Code";
        public string SortDirection { get; set; } = "asc";
    }
}
