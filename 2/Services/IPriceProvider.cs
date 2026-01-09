public interface IPriceProvider
{
    decimal GetPrice(string ticker)
    {
        return 0.0M;
    }
}