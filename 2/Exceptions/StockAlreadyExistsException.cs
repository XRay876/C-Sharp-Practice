public class StockAlreadyExistsException : Exception
{
    public string Ticker {get;}

    public StockAlreadyExistsException(string ticker) 
        : base($"Акция с тикером {ticker} уже существует в базе данных")
    {
        Ticker = ticker;
    }
}