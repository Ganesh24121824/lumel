namespace SalesAnalyticsData.Models;

public class Customer
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = null!;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
