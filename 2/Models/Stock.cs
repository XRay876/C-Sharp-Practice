namespace StockWatcher.Api.Entities;

public class Stock
{
    public int Id {get;set;}
    public required string Ticker {get;set;}
    public string CompanyName {get;set;} = string.Empty;
    public decimal Price {get;set;}
    public DateTime LastUpdate {get;set;} = DateTime.UtcNow;


    public List<StockPriceHistory> PriceHistory {get;set;} = new();
}