namespace SalesAnalyticsData.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderRefId { get; set; }
    public Order? Order { get; set; }

    public int ProductRefId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal ShippingCost { get; set; }

    public decimal TotalPrice => (UnitPrice * Quantity) * (1 - Discount) + ShippingCost;
}
