using StockWatcher.Api.Entities;

public static class StockMappingExtensions
{
    public static StockDTO ToDto(this Stock stock)
    {
        return new StockDTO
        {
            Ticker = stock.Ticker,
            Price = stock.Price,
            PriceHistory = stock.PriceHistory.Select(ph => ph.ToDto()).ToList()
        };
    }

    public static StockPriceDTO ToDto(this StockPriceHistory history)
    {
        return new StockPriceDTO
        {
            
            Price = history.Price,
            ChangeDate = history.ChangeDate,
        };
    }
}