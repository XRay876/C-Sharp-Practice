using System.Linq;
using Microsoft.EntityFrameworkCore;
using StockWatcher.Api.Data;
using StockWatcher.Api.Entities;

public class StockService : IStockService
{
    private readonly ILogger<StockService> _logger;
    private readonly IStockRepository _db;

    public StockService(ILogger<StockService> logger, IStockRepository db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task EnsureTickerExists(string ticker)
    {

        var check = await _db.GetAnyAsync(ticker);
        if (!check)
        {
            throw new StockNotFoundException(ticker);
        }
    }

    public async Task<bool> IsStockExists(string ticker)
    {
        var check = await _db.GetAnyAsync(ticker);
        return check;
    }

    public async Task ProcessPurchase(string ticker, decimal amount)
    {
        
        _logger.LogInformation("Beginning of the purchase: {Ticker}", ticker);
        
        await EnsureTickerExists(ticker);
        Console.WriteLine($"Куплено {amount} акций {ticker}");
    }
    public int GetPrice(string ticker)
    {
        return 0;
    }

    public async Task AddTicker(string ticker, decimal price, string companyName)
    {
        var exists = await IsStockExists(ticker);
        if (exists)
        {
           
            throw new StockAlreadyExistsException(ticker);
        }
       
        var newStock = new Stock
        {
            Ticker = ticker.ToUpper(),
            Price = price,
            CompanyName = companyName,
            LastUpdate = DateTime.UtcNow,
            // PriceHistory = [new StockPriceHistory{Price = price}]
        };
        newStock.PriceHistory.Add(new StockPriceHistory{Price = price});
        await _db.AddAsync(newStock);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Stock {Ticker} added to database", ticker);
    
        
    }

    public async Task UpdatePrice(string ticker, decimal price)
    {
        // var exists = await IsStockExists(ticker);

        // var stock = await _db.Stocks
        //     .Include(s => s.PriceHistory) 
        //     .FirstOrDefaultAsync(x => x.Ticker == ticker.ToUpper());
        var stock = await _db.GetByTickerAsync(ticker);

        if (stock == null)
        {
            throw new StockNotFoundException(ticker);
        }



        stock.PriceHistory.Add(new StockPriceHistory{Price = stock.Price});
        stock.Price = price;
        stock.LastUpdate = DateTime.UtcNow;
        // await _db.SaveChangesAsync();

        await _db.UpdateAsync(stock);
        await _db.SaveChangesAsync();    
        
    }

    public async Task<List<StockDTO>> GetStocks()
    {
        var allStocks = await _db.GetAllAsync();
        return allStocks.Select(s=>s.ToDto()).ToList();
    }

    public async Task<StockDTO?> GetStock(string ticker)
    {
        var stock = await _db.GetByTickerAsync(ticker);
        return stock?.ToDto();
    }
    

    public async Task<StockPriceDTO?> GetStockPrice(string ticker)
        {
            return await _db.GetPriceOnlyAsync(ticker);
        }

    public async Task DeleteStock(string ticker)
    {
        var rowsAffected = await _db.DeleteByTickerAsync(ticker);
        
        if (rowsAffected < 1)
        {
            throw new StockNotFoundException(ticker);
        }
        
    }

    public async Task<StockDTO?> GetStockByCompany(string companyName)
    {
        var stock = await _db.GetStockByCompanyNameAsync(companyName);
        return stock?.ToDto();
    }
    
}