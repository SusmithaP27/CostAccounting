namespace CostAccounting.Models
{
    public class RoadVM
    {
        public List<Road> Roads { get; set; } = new();
        public string? SearchTerm { get; set; }
        public bool? FilterActive { get; set; }
        public bool? FilterEngineering { get; set; }
        public string SortColumn { get; set; } = "Code";
        public string SortDirection { get; set; } = "asc";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
    }
}
