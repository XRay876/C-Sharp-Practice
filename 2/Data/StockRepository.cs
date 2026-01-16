using Microsoft.EntityFrameworkCore;
using StockWatcher.Api.Data;
using StockWatcher.Api.Entities;

public interface IStockRepository
{
    Task<IEnumerable<Stock>> GetAllAsync();
    Task<Stock?> GetByTickerAsync(string ticker);
    Task AddAsync(Stock stock);
    Task UpdateAsync(Stock stock);
    Task DeleteAsync(Stock stock);
    Task<bool> SaveChangesAsync();
    Task<bool> GetAnyAsync(string ticker);
    Task<StockPriceDTO?> GetPriceOnlyAsync(string ticker);

    Task<int> DeleteByTickerAsync(string ticker);
    Task<Stock?> GetStockByCompanyNameAsync(string companyName);
}

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;

    public StockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Stock>> GetAllAsync()
    {
        return await _context.Stocks.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Stock>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _context.Stocks.AsNoTracking().OrderBy(s=>s.Ticker).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<bool> GetAnyAsync(string ticker)
    {
        return await _context.Stocks.AnyAsync(x => x.Ticker == ticker.ToUpper());
    }
    public async Task<Stock?> GetByTickerAsync(string ticker)
    {
        return await _context.Stocks
            .Include(s => s.PriceHistory)
            .FirstOrDefaultAsync(s => s.Ticker == ticker.ToUpper());
    }

    public async Task<StockPriceDTO?> GetPriceOnlyAsync(string ticker)
    {
        return await _context.Stocks
            .AsNoTracking()
            .Where(s => s.Ticker == ticker.ToUpper())
            .Select(s => new StockPriceDTO
            {
                Price = s.Price,
                ChangeDate = s.LastUpdate
            })
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Stock stock) => await _context.Stocks.AddAsync(stock);

    public async Task UpdateAsync(Stock stock) => _context.Stocks.Update(stock);

    public async Task DeleteAsync(Stock stock) => _context.Stocks.Remove(stock);

    public async Task<int> DeleteByTickerAsync(string ticker)
    {
        
        return await _context.Stocks
            .Where(x => x.Ticker == ticker.ToUpper())
            .ExecuteDeleteAsync();
    }

    public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;

    public async Task<Stock?> GetStockByCompanyNameAsync(string companyName) => await _context.Stocks.Where(x=> x.CompanyName.Contains(companyName)).FirstOrDefaultAsync();
}