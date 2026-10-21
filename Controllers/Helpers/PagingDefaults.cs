namespace CostAccounting.Helpers
{
    public static class PagingDefaults
    {
        public const int DefaultPageSize = 15;
        public static readonly int[] PageSizes = { 10, 15, 25, 50, 100 };

        // Falls back to 15 if the query string asks for something not in the list
        public static int Normalize(int pageSize) =>
            PageSizes.Contains(pageSize) ? pageSize : DefaultPageSize;
    }
}