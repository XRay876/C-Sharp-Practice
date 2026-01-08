<p>Разбор ответов (коротко и по делу)

class / struct / record

class и record class — ссылочные; могут быть и mutable, и immutable (зависит от set/init).

struct — значимый; лучше небольшой и иммутабельный (рекомендация, не правило). Хранится не «всегда в стеке», а там, где лежит владелец: в стеке как локалка, в куче как поле класса и т.п.

record = синтаксис + value-equality + with/deconstruct. Размер не «меньше» автоматически.
✔️ Запомни: record = семантика «по данным» и равенство по содержимому; не про размер.

get/set/init

init — задать только при инициализации/в конструкторе/with.

Модификаторы можно ставить на аксессоры: public string Name { get; private set; }.

Поле «private» никак не мешает свойству — это разные члены.

record struct

Да: значимый + value-equality + with.

Не гарант «в стеке». См. пункт 1.

enum

Да, набор именованных констант. Можно указать базовый тип (: byte) и делать Flags-перечисления (битовые маски).

namespace

Это не «пакет», а логическое пространство имён. Не обязателен, но в проде — почти всегда. Есть file-scoped: namespace X;.

IEnumerable<T>

Интерфейс «перечислимости вперед». Позволяет foreach и LINQ (в памяти). Не рандом-доступ, не модифицирование.

IQueryable<T>

Не «очередь». Это провайдер запросов: LINQ выражение превращается, например, в SQL (EF Core). Исполняется не в памяти, а у источника (БД и т.д.).

Популярные LINQ-операторы

Select, Where, OrderBy/ThenBy, GroupBy, Join/GroupJoin, SelectMany, Distinct, Any/All, Count/Sum/Average/Min/Max, Skip/Take, Concat/Union, Zip.
(А Sort/Find/FindAll — это методы List<T>, не LINQ.)

GroupJoin vs SelectMany

GroupJoin = «левый джойн» с группой совпадений справа для каждого ключа слева.

SelectMany = «сплющивание» последовательностей (каждый элемент → много элементов → одна плоская).

ToList() vs AsEnumerable()

ToList() материализует (выполняет запрос, создаёт список).

AsEnumerable() лишь меняет тип в цепочке (переключает расширения LINQ-to-Objects), не выполняя запрос.

Task vs ValueTask

Task — ссылочный awaitable.

ValueTask — структура для оптимизации, когда результат часто готов синхронно (избегаем аллокации Task). Есть нюансы: один await/одно чтение результата, сложнее композировать. Не «возвращает число».

await

Асинхронно «уступает» управление, не блокируя поток. Возврат не гарантирует смену потока, а «приклеен» к синхронизационному контексту (если он есть).

CancellationToken

Принимаем ct в публичных async-API, прокидываем в I/O-методы (GetAsync(..., ct)), иногда проверяем ct.ThrowIfCancellationRequested() в длинных циклах. Регистрация: using var reg = ct.Register(...);.

Как обрабатывать отмену

Рано проверить входные условия, передать ct в I/O, периодически проверять/ThrowIfCancellationRequested(), при отмене не маскировать OperationCanceledException.

ConfigureAwait(false)

«Не захватывать контекст» при возврате из await. Нужен в библиотечном коде (UI-зависания/дедлоки) и для снижения оверхеда. В ASP.NET Core контекста нет, но false всё равно снижает затраты.

Делегаты

Типобезопасные «указатели на метод». Action/Func/Predicate<T> — готовые делегаты. События на них строятся, но событие ≠ делегат.

Action / Func<T> / Predicate<T>

Action — метод без результата.

Func<T1,..,TResult> — метод с результатом.

Predicate<T> — Func<T, bool> (семантика «предикат»).

event

Обёртка над делегатом с ограничением: только владелец события может вызывать (Invoke), внешние — только +=/-=. Инкапсулирует публикацию.

EventHandler

Общепринятый шаблон: void Handler(object? sender, EventArgs e). Для своих данных — EventHandler<TEventArgs>. Даёт унификацию и совместимость с инструментами.

Стек/куча

В стеке — кадры вызовов и локальные значимые типы; в куче — объекты (включая string). Значимые типы могут в куче (как часть объекта). Управляет GC.

GC и using

using/Dispose() освобождает внешние ресурсы (файл/сокет), не память. Принудительный GC — GC.Collect(), почти всегда вредно. Память объектов освобождает GC самостоятельно.

Span<T>/ReadOnlySpan<T>

ref struct представляют непрерывный кусок памяти (массив, stackalloc, unmanaged). Не аллоцируются в куче, нельзя хранить в полях класса/в async/итераторах. Плюс — нулевая аллокация и безопасность. Не «лучше массива всегда» из-за ограничений жизненного цикла.

DateTime.UtcNow vs Stopwatch

UtcNow — «стенные» часы (может прыгать из-за синхронизации). Stopwatch — монотонные высокоточные тики → правильнее для измерений.

Ссылки

Strong — держат объект живым.

Weak (WeakReference<T>) — не мешают GC собирать объект; полезны для кэшей, где «не страшно потерять» значение.</p>






















<p>
Юнит-тесты: быстрый старт
Стек

Фреймворк: xUnit (популярен в .NET).

Ассерты: FluentAssertions (читабельные сообщения).

Моки: Moq или NSubstitute.

Организация

Отдельный проект: MyApp.Tests, ссылка на основной проект.

Имена тестов: Method_Should_Do_When.

Структура: AAA — Arrange / Act / Assert.

Примеры

1) NewsService

public class NewsServiceTests
{
    [Fact]
    public void Add_Raises_Event_And_Stores()
    {
        var svc = new NewsService();
        Article? raised = null;
        svc.ArticleAdded += a => raised = a;

        var art = new Article(Guid.NewGuid(), "Hello", "Body", DateTime.UtcNow);
        svc.Add(art);

        raised.Should().Be(art);
        svc.GetRecent(1).Single().Should().Be(art);
    }

    [Fact]
    public void Find_Is_Case_Insensitive()
    {
        var svc = new NewsService(new[]
        {
            new Article(Guid.NewGuid(), "Hello", "world", DateTime.UtcNow)
        });

        svc.TryFind("WORLD", out var hit).Should().BeTrue();
        hit!.Title.Should().Be("Hello");
    }
}


2) CachedHttpClient (мокаем HTTP через кастомный HttpMessageHandler)

public sealed class StaticHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _r;
    public StaticHandler(HttpResponseMessage r) => _r = r;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        => Task.FromResult(_r);
}

public class CachedHttpClientTests
{
    [Fact]
    public async Task Returns_From_Cache_Within_Ttl()
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK){ Content = new StringContent("OK") };
        var http = new HttpClient(new StaticHandler(resp));
        var cli = new CachedHttpClient(http, TimeSpan.FromSeconds(10));

        var u = new Uri("http://x");
        var s1 = await cli.GetAsync(u, TimeSpan.FromMilliseconds(200), CancellationToken.None);
        var s2 = await cli.GetAsync(u, TimeSpan.FromMilliseconds(200), CancellationToken.None);

        s1.Should().Be("OK");
        s2.Should().Be("OK"); // из кэша
    }

    [Fact]
    public async Task Respects_Timeout()
    {
        var handler = new HttpMessageHandlerThatDelays(TimeSpan.FromSeconds(5)); // напиши аналогично
        var http = new HttpClient(handler);
        var cli = new CachedHttpClient(http, TimeSpan.FromSeconds(1));

        var act = async () => await cli.GetAsync(new Uri("http://x"), TimeSpan.FromMilliseconds(50), CancellationToken.None);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}


3) Stats

public class StatsTests
{
    [Fact]
    public void Analyze_Computes_All()
    {
        var (avg, med, min, max) = Stats.Analyze(new int[]{3,1,4,2});
        avg.Should().Be(2.5);
        med.Should().Be((2+3)/2.0);
        min.Should().Be(1);
        max.Should().Be(4);
    }

    [Fact]
    public void Analyze_Throws_On_Empty()
    {
        Action act = () => Stats.Analyze(Array.Empty<int>());
        act.Should().Throw<ArgumentException>();
    }
}

Практические правила

Тест маленький, изолированный, детерминированный.

Исключения проверяем await act.Should().ThrowAsync<T>().

Асинхронные тесты — async Task, не async void.

Внешние зависимости — мок/фейк (сети, БД, время).

Покрывай «счастливые пути», уголки (пусто/границы) и ошибки.

Как запускать
dotnet test
</p>