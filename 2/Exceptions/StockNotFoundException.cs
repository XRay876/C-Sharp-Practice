public class StockNotFoundException : Exception
{
    public string Ticker { get; }

    public StockNotFoundException(string ticker) : base($"Акция с тикером '{ticker}' не найдена на бирже.")
    {
        Ticker = ticker;
    }
}