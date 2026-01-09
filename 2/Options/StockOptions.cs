//environment variables for Stocks, using from appsettings.json
public class StockOptions
{
    public const string SectionName = "StockSettings";

    public string DefaultCurrency { get; set; } = string.Empty;
    public int UpdateIntervalSeconds { get; set; }
    public string ApiUrl { get; set; } = string.Empty;
}