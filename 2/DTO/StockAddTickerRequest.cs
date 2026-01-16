using System.ComponentModel.DataAnnotations;

public class StockAddTickerRequest
{
    [Required(ErrorMessage = "Ticker required")]
    [StringLength(5, MinimumLength = 1)]
    public required String Ticker { get; set; }

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    public string CompanyName {get;set;} = string.Empty;
}