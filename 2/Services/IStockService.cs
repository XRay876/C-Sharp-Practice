using StockWatcher.Api.Entities;

public interface IStockService
{
    Task ProcessPurchase(string ticker, decimal amount);
    Task AddTicker(string ticker, decimal price, string companyName);
    Task<List<StockDTO>> GetStocks();
    int GetPrice(string ticker);
    Task UpdatePrice(string ticker, decimal price);
    Task<StockDTO?> GetStock(string ticker);
    Task<StockPriceDTO?> GetStockPrice(string ticker);
    Task DeleteStock(string ticker);
    Task EnsureTickerExists(string ticker);
    Task<StockDTO?> GetStockByCompany(string companyName);
}