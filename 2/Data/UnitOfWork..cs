using StockWatcher.Api.Data;

public interface IUnitOfWork : IDisposable
{
    IStockRepository Stocks { get; }
    //other repositories..
    Task<bool> CompleteAsync(); // Один метод сохранения для всех
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public IStockRepository Stocks { get; private set; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Stocks = new StockRepository(_context);
    }

    public async Task<bool> CompleteAsync() => await _context.SaveChangesAsync() > 0;

    public void Dispose() => _context.Dispose();
}

// in a code we are no longer using separate contexts, we can use one _uow
// Example:
// public async Task UpdatePrice(string ticker, decimal price)
// {
//     var stock = await _uow.Stocks.GetByTickerAsync(ticker);
//     // ... логика ...
//     await _uow.CompleteAsync(); // Сохраняем всё сразу
// }