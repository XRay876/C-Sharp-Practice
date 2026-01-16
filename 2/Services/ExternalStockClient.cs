public class ExternalStockClient
{
    private readonly HttpClient _httpClient;

    public ExternalStockClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetRealPrice(string ticker)
    {
        
        var response = await _httpClient.GetFromJsonAsync<decimal>($"quote?symbol={ticker}");
        
        return response;
    }

    public async Task<int> GetPriceAsync(int num)
    {
        return await _httpClient.GetFromJsonAsync<int>($"posts/{num}");
    }
}