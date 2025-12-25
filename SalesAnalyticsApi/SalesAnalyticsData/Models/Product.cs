namespace SalesAnalyticsData.Models;

public class Product
{
    public int Id { get; set; }
    public string ProductId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
}
