using StockWatcher.Api.Entities;

public class StockDTO
{
    public required string Ticker { get; set; }
    public decimal Price { get; set; }

    public List<StockPriceDTO> PriceHistory {get;set;} = new();
}