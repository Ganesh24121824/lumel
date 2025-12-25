namespace SalesAnalyticsData.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderId { get; set; } = null!;
    public DateTime DateOfSale { get; set; }
    public string? Region { get; set; }
    public string? PaymentMethod { get; set; }

    public int CustomerIdRef { get; set; }
    public Customer? Customer { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
