using System.ComponentModel.DataAnnotations;

public class StockPurchaseRequest
{
    [Required(ErrorMessage = "Ticker required")]
    [StringLength(5, MinimumLength = 1)]
    public String Ticker { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue)]
    public int Amount { get; set; } 

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    //example: [EmailAddress]
}