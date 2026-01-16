using Microsoft.EntityFrameworkCore;
using StockWatcher.Api.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StockOptions>(configuration.GetSection(StockOptions.SectionName));
        services.Configure<WordsOptions>(configuration.GetSection(WordsOptions.SectionName));

        //db
        services.AddScoped<IStockRepository, StockRepository>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        //Transient (every time when called, light services), Scoped (once for each request, db), Singelton (forever one, cache/config)
        
        //AddScoped - Guid will be the same, AddTransient - Guid will be different, AddSingleton - Guid will be the same until you restart the server
        services.AddScoped<IStockService, StockService>();
        services.AddTransient<IPriceProvider, RandomPriceProvider>();
        services.AddScoped<IGuidService, GuidService>(); 
        services.AddScoped<IHelperService, HelperService>();
        

        services.AddHttpClient<ExternalStockClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.marketdata.com/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}