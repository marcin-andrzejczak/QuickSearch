namespace QuickSearch.Benchmark.Benchmarks.Filtering;

public static class Categories
{
    public static class Filter
    {
        public const string EqualString = nameof(EqualString);
        public const string MultipleStrings = nameof(MultipleStrings);
        public const string ContainsString = nameof(ContainsString);
    }

    public static class Sort
    {
        public const string ByString = nameof(ByString);
        public const string ByMultipleStrings = nameof(ByMultipleStrings);
    }
}
