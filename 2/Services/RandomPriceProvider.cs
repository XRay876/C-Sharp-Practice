public class RandomPriceProvider : IPriceProvider
{
    public decimal GetPrice(string ticker)
    {
        return Random.Shared.Next(100);
    }
}