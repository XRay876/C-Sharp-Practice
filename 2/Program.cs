using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

//Transient (every time when called, light services), Scoped (once for each request, db), Singelton (forever one, cache/config)
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddTransient<IPriceProvider, RandomPriceProvider>();

//AddScoped - Guid will be the same, AddTransient - Guid will be different, AddSingleton - Guid will be the same until you restart the server
builder.Services.AddScoped<IGuidService, GuidService>(); 
builder.Services.AddScoped<IHelperService, HelperService>();

builder.Services.Configure<StockOptions>(builder.Configuration.GetSection(StockOptions.SectionName));
builder.Services.Configure<WordsOptions>(builder.Configuration.GetSection(WordsOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}


app.Use(async (context, next) =>
{
    
    await next();
    Console.WriteLine("Method: " + context.Request.Path + " Status: " + context.Response.StatusCode);
});
app.UseExceptionHandler("/error"); // ловит ошибки от всех кто дальше
app.UseHttpsRedirection(); // перенаправляет на https
app.Use(async (context, next) =>
{
    var options = context.RequestServices.GetRequiredService<IOptions<WordsOptions>>();
    var restrictedWords = options.Value.RestrictedWords;

    var path = context.Request.Path.Value?.ToLower() ?? "";
    bool isForbidden = restrictedWords.Any(word => path.Contains(word.ToLower()));

    if (isForbidden)
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Access Denied!");
    } 
    else
    {
        await next();
    }
});
app.UseStaticFiles(); // css
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// app.Map("/ping", (appBranch) =>
// {
//     appBranch.Run(async (context) =>
//     {
//         await context.Response.WriteAsync("Pong");
//     });
// });
app.MapGet("/ping", () => "Pong");
app.MapControllers();


app.Run();
