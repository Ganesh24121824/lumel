using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAnalyticsData;

namespace SalesAnalyticsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly SalesAnalyticsContext _dbContext;

    public AnalyticsController(SalesAnalyticsContext db)
    {
        _dbContext = db;
    }


    [HttpGet("revenue/total")]
    public async Task<IActionResult> TotalRevenue([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var q = _dbContext.OrderItems.Include(oi => oi.Order).AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var total = await q.SumAsync(oi => (decimal?)oi.TotalPrice) ?? 0m;

        return Ok(new { total });
    }

    [HttpGet("revenue/by-product")]
    public async Task<IActionResult> RevenueByProduct([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var items = await q
            .GroupBy(oi => new { oi.ProductRefId, oi.Product!.Name })
            .Select(g => new { ProductId = g.Key.ProductRefId, Name = g.Key.Name, Revenue = g.Sum(x => x.TotalPrice) })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("revenue/by-category")]
    public async Task<IActionResult> RevenueByCategory([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var items = await q
            .GroupBy(oi => oi.Product!.Category)
            .Select(g => new { Category = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        return Ok(items);
    }


    [HttpGet("revenue/by-region")]
    public async Task<IActionResult> RevenueByRegion([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var items = await q
            .GroupBy(oi => oi.Order!.Region)
            .Select(g => new { Region = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        return Ok(items);
    }


    [HttpGet("revenue/trends")]
    public async Task<IActionResult> RevenueTrends([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] string interval = "monthly")
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var list = await q.ToListAsync();

        var grouped = interval.ToLowerInvariant() switch
        {
            "yearly" => list.GroupBy(oi => oi.Order!.DateOfSale.Year)
                .Select(g => new { Period = g.Key.ToString(), Revenue = g.Sum(x => x.TotalPrice) }),
            "quarterly" => list.GroupBy(oi => new { Year = oi.Order!.DateOfSale.Year, Quarter = ((oi.Order!.DateOfSale.Month - 1) / 3) + 1 })
                .Select(g => new { Period = $"{g.Key.Year}-Q{g.Key.Quarter}", Revenue = g.Sum(x => x.TotalPrice) }),
            _ => list.GroupBy(oi => new { oi.Order!.DateOfSale.Year, oi.Order!.DateOfSale.Month })
                .Select(g => new { Period = $"{g.Key.Year}-{g.Key.Month:D2}", Revenue = g.Sum(x => x.TotalPrice) }),
        };

        var result = grouped.OrderBy(x => x.Period);

        return Ok(result);
    }


    [HttpGet("top-products/overall")]
    public async Task<IActionResult> TopProductsOverall([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] int n = 10)
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var items = await q
            .GroupBy(oi => new { oi.ProductRefId, oi.Product!.Name })
            .Select(g => new { ProductId = g.Key.ProductRefId, Name = g.Key.Name, Quantity = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .Take(n)
            .ToListAsync();

        return Ok(items);
    }


    [HttpGet("top-products/by-category")]
    public async Task<IActionResult> TopProductsByCategory([FromQuery] string category, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] int n = 10)
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.Product != null && oi.Product.Category == category)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var items = await q
            .GroupBy(oi => new { oi.ProductRefId, oi.Product!.Name })
            .Select(g => new { ProductId = g.Key.ProductRefId, Name = g.Key.Name, Quantity = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .Take(n)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("top-products/by-region")]
    public async Task<IActionResult> TopProductsByRegion([FromQuery] string region, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] int n = 10)
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.Order != null && oi.Order.Region == region)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var items = await q
            .GroupBy(oi => new { oi.ProductRefId, oi.Product!.Name })
            .Select(g => new { ProductId = g.Key.ProductRefId, Name = g.Key.Name, Quantity = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .Take(n)
            .ToListAsync();

        return Ok(items);
    }


    [HttpGet("customers/count")]
    public async Task<IActionResult> TotalCustomers([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var q = _dbContext.Orders.AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(o => o.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(o => o.DateOfSale <= end.Value);
        }

        var count = await q.Select(o => o.CustomerIdRef).Distinct().CountAsync();

        return Ok(new { count });
    }


    [HttpGet("customers/orders/count")]
    public async Task<IActionResult> TotalOrders([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var q = _dbContext.Orders.AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(o => o.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(o => o.DateOfSale <= end.Value);
        }

        var count = await q.CountAsync();

        return Ok(new { count });
    }


    [HttpGet("customers/average-order-value")]
    public async Task<IActionResult> AverageOrderValue([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var ordersQ = _dbContext.Orders.AsQueryable();

        if (start.HasValue)
        {
            ordersQ = ordersQ.Where(o => o.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            ordersQ = ordersQ.Where(o => o.DateOfSale <= end.Value);
        }

        var orderTotals = await ordersQ
            .Select(o => new
            {
                o.Id,
                Total = _dbContext.OrderItems.Where(oi => oi.OrderRefId == o.Id).Sum(oi => (decimal?)oi.TotalPrice) ?? 0m
            })
            .ToListAsync();

        var avg = orderTotals.Any() ? orderTotals.Average(x => x.Total) : 0m;

        return Ok(new { averageOrderValue = avg });
    }

    [HttpGet("profit-margin/by-product")]
    public async Task<IActionResult> ProfitMarginByProduct([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] decimal costPercent = 0.6m)
    {
        var q = _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .AsQueryable();

        if (start.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale >= start.Value);
        }

        if (end.HasValue)
        {
            q = q.Where(oi => oi.Order!.DateOfSale <= end.Value);
        }

        var items = await q
            .GroupBy(oi => new { oi.ProductRefId, oi.Product!.Name })
            .Select(g => new
            {
                ProductId = g.Key.ProductRefId,
                Name = g.Key.Name,
                Revenue = g.Sum(x => x.TotalPrice),
                Cost = g.Sum(x => x.TotalPrice) * costPercent
            })
            .ToListAsync();

        var result = items.Select(x => new
        {
            x.ProductId,
            x.Name,
            x.Revenue,
            Cost = x.Cost,
            Profit = x.Revenue - x.Cost,
            Margin = x.Revenue == 0 ? 0 : (x.Revenue - x.Cost) / x.Revenue
        });

        return Ok(result);
    }
}
