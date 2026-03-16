namespace PeripheralsStore.ViewModels;

public class ProductFilterViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    public string SortBy { get; set; } = "nameAsc";
    public int Page { get; set; } = 1;
}
