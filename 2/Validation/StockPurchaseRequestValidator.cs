using FluentValidation;

public class StockPurchaseRequestValidator : AbstractValidator<StockPurchaseRequest>
{

    private readonly ILogger<StockPurchaseRequestValidator> _logger;
    public StockPurchaseRequestValidator(ILogger<StockPurchaseRequestValidator> logger)
    {
        _logger = logger;


        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Тикер не может быть пустым")
            .Length(1, 5).WithMessage("Длина тикера от 1 до 5 символов")
            .Must(t => t == t.ToUpper()).WithMessage("Тикер должен быть в верхнем регистре");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Количество должно быть больше нуля");

        RuleFor(x => x.Price)
            .InclusiveBetween(0.1m, 100000m).WithMessage("Цена вне допустимого диапазона");
    }
}