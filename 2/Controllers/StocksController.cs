using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IPriceProvider _priceProvider;

    private readonly IGuidService _guidService;
    private readonly IHelperService _helperService;
    private readonly StockOptions _options;

    public StocksController(IStockService stockService, IPriceProvider priceProvider, IGuidService guidService, IHelperService helperService, IOptions<StockOptions> options)
    {
        _options = options.Value;

        _stockService = stockService;
        _priceProvider = priceProvider;

        _guidService = guidService;
        _helperService = helperService;
    }

    [HttpGet]
    public IActionResult GetPrice(string ticker)
    {
        var price = _priceProvider.GetPrice(ticker);
        return Ok(new { Ticker = ticker, Price = price});
    }

    [HttpGet("test-options")]
    public decimal GetOptions()
    {
        Console.WriteLine($"Request to: {_options.ApiUrl} in a currency {_options.DefaultCurrency}");
        return 150.0M;
    }

    [HttpGet("test-di")]
    public IActionResult TestDi()
    {
        return Ok(new {
            ControllerGuid = _guidService.GetGuid(),
            HelperGuid = _helperService.GetGuidFromHelper()
        });
    }
}