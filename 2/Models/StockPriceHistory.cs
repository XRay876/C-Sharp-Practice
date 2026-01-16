namespace StockWatcher.Api.Entities;

public class StockPriceHistory
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public DateTime ChangeDate {get; set; } = DateTime.UtcNow;

    //foreign key
    public int StockId { get; set; }

    //navigation property
    public Stock? Stock { get; set; }
}