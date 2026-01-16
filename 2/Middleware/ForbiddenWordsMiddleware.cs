using Microsoft.Extensions.Options;

public class ForbiddenWordsMiddleware
{
    private readonly RequestDelegate _next;

    public ForbiddenWordsMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IOptions<WordsOptions> options)
    {
        var restrictedWords = options.Value.RestrictedWords;
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (restrictedWords.Any(word => path.Contains(word.ToLower())))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access Denied!");
            return;
        }

        await _next(context);
    }
}