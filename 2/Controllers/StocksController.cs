using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using FluentValidation;
using StockWatcher.Api.Data;
using StockWatcher.Api.Entities;

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
 
    [HttpPost("purchase/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Swagger tips
    public async Task<IActionResult> Purchase([FromRoute] int userId, [FromQuery] string ticker, [FromBody] StockPurchaseRequest request, [FromServices] IValidator<StockPurchaseRequest> validator)
    {
        var validationReq = await validator.ValidateAsync(request);
        if (!validationReq.IsValid)
        {
            return BadRequest(validationReq.Errors);
        }
        await _stockService.ProcessPurchase(request.Ticker, request.Amount);
        return Ok(new 
        { 
            UserId = userId, 
            TickerFromQuery = ticker, 
            DataFromBody = request 
        });
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddTicker([FromBody] StockAddTickerRequest request)
    {
        await _stockService.AddTicker(request.Ticker, request.Price, request.CompanyName);
        return Ok(new
        {
            TickerName = request.Ticker,
            TickerPrice = request.Price,
            Company = request.CompanyName,
        });
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllTickers()
    {
        var tickers = await _stockService.GetStocks();
        return Ok(tickers);
    }

    [HttpPut("update-stock")]
    public async Task<IActionResult> UpdateStock(string ticker, decimal price)
    {
        await _stockService.UpdatePrice(ticker, price);
        return Ok();
    }

    [HttpGet("get-stock")]
    public async Task<ActionResult<StockDTO>> GetStock([FromQuery] string ticker)
    {
        var stock = await _stockService.GetStock(ticker);
        return Ok(stock);
    }

    [HttpGet("get-stock-history")]
    public async Task<ActionResult<StockPriceDTO>> GetStockPrice([FromQuery] string ticker)
    {
        var stockPrice = await _stockService.GetStockPrice(ticker);
        return Ok(stockPrice);
    } 

    [HttpDelete("delete-stock")]
    public async Task<ActionResult> DeleteStock(string ticker)
    {
        await _stockService.DeleteStock(ticker);
        return Ok();
    }
}