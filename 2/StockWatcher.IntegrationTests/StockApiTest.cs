using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.AspNetCore.Hosting;

public class StockApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public StockApiTest(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Ping_ShouldReturnPong()
    {
        // Given
        var response = await _httpClient.GetAsync("/ping");
        // When
        
        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Pong", content);
    
    }


    [Fact]
    public async Task Middleware_ShouldBlockForbiddenWords()
    {
        var response = await _httpClient.GetAsync("/api/stocks/forbidden");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // [Fact]
    // public async Task Purchase_ShouldReturnNotFound_WhenStockDoesNotExist()
    // {
    //     // Arrange: Настраиваем фабрику с подменой сервиса
    //     var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    //     {
    //         builder.ConfigureServices(services =>
    //         {
    //             // Здесь можно заменить реальный IPriceProvider на фейковый
    //             // services.AddScoped<IPriceProvider, FakePriceProvider>();
    //         });
    //     });
    //     var client = factory.CreateClient();

    //     var request = new { Ticker = "NON-EXISTENT", Amount = 10, Price = 100 };

    //     // Act
    //     var response = await client.PostAsJsonAsync("/api/stocks/purchase/1", request);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    // }
}