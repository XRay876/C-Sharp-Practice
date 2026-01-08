try
{
    int x = int.Parse("abc");
}
catch (FormatException x)
{
    Console.WriteLine(x.Message);
}
finally
{
    Console.WriteLine("всегда выполняется");
}