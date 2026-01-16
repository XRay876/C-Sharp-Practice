using Microsoft.Extensions.Options;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddBusinessServices(builder.Configuration);
builder.Services.AddAppRateLimiter();
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("https://api.marketdata.com/"), "External Market API");

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<StockPurchaseRequestValidator>();


var app = builder.Build();

// 3. Конвейер (Pipeline)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    
    await next();
    Console.WriteLine("Method: " + context.Request.Path + " Status: " + context.Response.StatusCode);
});
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

app.UseMiddleware<ForbiddenWordsMiddleware>();

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(x => new 
            {
                Component = x.Key,
                Status = x.Value.Status.ToString(),
                Description = x.Value.Description
            }),
            Duration = report.TotalDuration
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});
app.MapControllers();

app.Run();




public partial class Program
{
    
}