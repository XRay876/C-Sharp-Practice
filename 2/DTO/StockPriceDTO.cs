using StockWatcher.Api.Entities;

public class StockPriceDTO
{
    public required decimal Price { get; set; }
    public DateTime changeDate { get; set; }
}