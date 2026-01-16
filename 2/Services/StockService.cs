using System.Linq;
using Microsoft.EntityFrameworkCore;
using StockWatcher.Api.Data;
using StockWatcher.Api.Entities;

public class StockService : IStockService
{
    private readonly ILogger<StockService> _logger;
    private readonly AppDbContext _db;

    public StockService(ILogger<StockService> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task EnsureTickerExists(string ticker)
    {

        var check = await _db.Stocks.AnyAsync(x => x.Ticker == ticker.ToUpper());
        if (!check)
        {
            throw new StockNotFoundException(ticker);
        }
    }

    public async Task<bool> IsStockExists(string ticker)
    {
        var check = await _db.Stocks.AnyAsync(x => x.Ticker == ticker.ToUpper());
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
        _db.Stocks.Add(newStock);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Stock {Ticker} added to database", ticker);
    
        
    }

    public async Task UpdatePrice(string ticker, decimal price)
    {
        // var exists = await IsStockExists(ticker);

        var stock = await _db.Stocks
            .Include(s => s.PriceHistory) 
            .FirstOrDefaultAsync(x => x.Ticker == ticker.ToUpper());

        if (stock == null)
        {
            throw new StockNotFoundException(ticker);
        }



        stock.PriceHistory.Add(new StockPriceHistory{Price = stock.Price});
        stock.Price = price;
        stock.LastUpdate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    
        
    }

    public async Task<List<StockDTO>> GetStocks()
    {
        var allStocks = await _db.Stocks.Include(s=>s.PriceHistory).AsNoTracking().Select(s => new StockDTO{Ticker=s.Ticker, Price = s.Price, PriceHistory = s.PriceHistory.Select(ph => new StockPriceDTO{Price = ph.Price, changeDate = ph.ChangeDate}).ToList()}).ToListAsync();
        return allStocks;
    }

    public async Task<StockDTO?> GetStock(string ticker)
    {
        return await _db.Stocks
            .AsNoTracking()
            .Where(s => s.Ticker == ticker.ToUpper())
            .Select(s => new StockDTO
            {
                Ticker = s.Ticker,
                Price = s.Price,
                PriceHistory = s.PriceHistory.Select(ph => new StockPriceDTO
                {
                    Price = ph.Price, 
                    changeDate = ph.ChangeDate
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }
    

    public async Task<StockPriceDTO?> GetStockPrice(string ticker)
        {
            return await _db.Stocks
                .AsNoTracking()
                .Where(s => s.Ticker == ticker.ToUpper())
                .Select(s => new StockPriceDTO
                {
                    Price = s.Price,
                    changeDate = s.LastUpdate
                })
                .FirstOrDefaultAsync();
    }

    public async Task DeleteStock(string ticker)
    {
        var rowsAffected = await _db.Stocks
            .Where(s => s.Ticker == ticker.ToUpper())
            .ExecuteDeleteAsync();
        
        if (rowsAffected < 1)
        {
            throw new StockNotFoundException(ticker);
        }
        
    }
    
}