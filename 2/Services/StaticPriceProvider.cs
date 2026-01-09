public class StaticPriceProvider : IPriceProvider
{
    public decimal GetPrice(string ticker)
    {
        return 100.0M;
    }
}