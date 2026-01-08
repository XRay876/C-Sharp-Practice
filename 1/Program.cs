
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

public static class Programm
{
    // A1
    public static Dictionary<string, int> WordFreq(string text)
    {
        return Regex.Split(text, @"\W+").Where(w => !string.IsNullOrEmpty(w)).GroupBy(w => w.ToLower()).ToDictionary(g => g.Key, g => g.Count());
    }

    // A3
    public static IEnumerable<int> Expand(IEnumerable<(int value, int count)> spec) {
        return spec.SelectMany(pair => Enumerable.Repeat(pair.value, pair.count));
    }

    // A4
    public static double Median(IEnumerable<double> xs)
    {
        var sorted = xs.OrderBy(x => x).ToList();
        int n = sorted.Count;

        if (n == 0) throw new InvalidOperationException();

        return (n & 1) == 1 ? sorted[n / 2] : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
    }

    // A5
    public sealed class User
    {
        public int Id { get; init; }
        public string Name { get; set; } = "";
    }

    public sealed class Purchase
    {
        public int UserId { get; init; }
        public decimal Amount { get; init; }
        public DateTime Date { get; init; }
    }

    public sealed class UserSummary
    {
        public int UserId { get; init; }
        public string Name { get; init; } = "";
        public decimal TotalSpent { get; init; }
        public DateTime? LastPurchaseDate { get; init; }
    }

    public static IEnumerable<UserSummary> BuildUserSummary(IEnumerable<User> users, IEnumerable<Purchase> purchases) {
        return users.GroupJoin(purchases, u => u.Id, p => p.UserId, (u, ps) => new UserSummary { UserId = u.Id, Name = u.Name, TotalSpent = ps.Sum(x => x.Amount), LastPurchaseDate = ps.Select(x => (DateTime?)x.Date).Max() });
    }


    // B1
    public readonly record struct Money
    {
        public decimal Amount { get; }
        public string Currency { get; }
        public Money(decimal amount, string currency) : this()
        {
            if (currency is not { Length: 3 })
                throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

            Amount = amount;
            Currency = currency;
        }

        public static Money operator +(Money m1, Money m2) => m1.Currency == m2.Currency
            ? new Money(m1.Amount + m2.Amount, m1.Currency)
            : throw new InvalidOperationException("Cannot add money in different currencies.");
    }

    // B2
    public record Error(Exception Exception);


    public static string Route(object msg) => msg switch
    {
        "Ping" => "pong",
        Error(Exception ex) => ex.Message,
        List<int> { Count: > 0 } list and [0, ..] => $"starts-with-zero(len={list.Count})",
        _ => "unknown"

    };

    // B3 
    public sealed class ExternalServiceException(string message, Exception inner) : Exception(message, inner) { } // кастомный класс для выброса ошибок
    public sealed record ApiCallResult(string? Content, int? RetryAfterSeconds);

    public static async Task<ApiCallResult> ApiCallAsync(string link, CancellationToken cancellationToken = default) {
        using var client = new HttpClient();
        HttpResponseMessage? response = null;
        int? retryAfterSeconds = null;

        try
        {
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, link), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            retryAfterSeconds = TryGetRetryAfterSeconds(response);

            if (response.StatusCode == (HttpStatusCode)429)
            {
                return new ApiCallResult(null, retryAfterSeconds);
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ApiCallResult(content, null);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == (HttpStatusCode)429)
        {
            return new ApiCallResult(null, retryAfterSeconds);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, link);
            throw new ExternalServiceException("External API call failed", ex);
        }

    }

    private static int? TryGetRetryAfterSeconds(HttpResponseMessage response)
    {
        var ra = response.Headers.RetryAfter;
        if (ra is null) return null;

        if (ra.Delta.HasValue)
        {
            return (int)Math.Max(0, ra.Delta.Value.TotalSeconds);
        }

        if (ra.Date.HasValue)
        {
            var seconds = (int)Math.Round((ra.Date.Value - DateTimeOffset.UtcNow).TotalSeconds);
            return Math.Max(0, seconds);
        }

        return null;
    }

    private static void LogError(Exception ex, string link)
    {
        Console.Error.WriteLine($"[API ERROR] {link}: {ex.GetType().Name} - {ex.Message}");
    }


    // C1
    public static async Task<string> FetchWithTimeoutAsync(Uri url, TimeSpan timeout, CancellationToken ct)
    {
        using var client = new HttpClient();

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            var response = await client.GetAsync(url, linkedCts.Token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new OperationCanceledException("..", ct);
        }

    }

    // C2
    public sealed class AsyncCache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _ready = new();

        private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _inflight = new();

        public ValueTask<TValue> GetOrAddAsync(TKey key, Func<CancellationToken, ValueTask<TValue>> factory, CancellationToken ct)
        {
            if (_ready.TryGetValue(key, out var value))
            { // если уже значение найдено, то возращаем
                return ValueTask.FromResult(value);
            }

            // получаем или создаем задачу для ключа 
            var lazy = _inflight.GetOrAdd(key, _ => new Lazy<Task<TValue>>(() => StartFactoryOnceAsync(key, factory), LazyThreadSafetyMode.ExecutionAndPublication));

            return new(lazy.Value.WaitAsync(ct));
            // throw new NotImplementedException();

        }

        private async Task<TValue> StartFactoryOnceAsync(TKey key, Func<CancellationToken, ValueTask<TValue>> factory)
        {
            try
            {
                TValue value = await factory(CancellationToken.None).ConfigureAwait(false);
                _ready[key] = value;
                return value;
            }
            finally
            {
                _inflight.TryRemove(key, out _);
            }
        }
    }

    //1-5 ez Tasks
    public static void PrintEvenOrOdd(int number)
    {
        Console.WriteLine(number % 2 == 0 ? "even" : "odd");
    }

    public static int MinOfThree(int a, int b, int c)
    {
        return Math.Min(Math.Min(a, b), c);
    }

    public static int SumOfN(int n)
    {
        int s = 0;
        for (int i = 0; i < n; i++)
        {
            s += i;
        }
        return s;
    }

    public static string ReverseString(string text)
    {
        string res = "";
        for (int i = text.Length - 1; i >= 0; i--)
        {
            res += text[i];
        }
        return res;
    }

    // 6 - 10
    public static (int vowels, int consonants) CountLetters(string text)
    {
        int vowelsCount = 0;
        int cCount = 0;
        char[] vowels = ['a', 'e', 'y', 'u', 'i', 'o'];

        IEnumerable<char> letters = [.. text.Select(l => char.ToLower(l)).Where(x => char.IsLetter(x))];
        foreach (char l in letters)
        {
            if (vowels.Contains(l))
            {
                vowelsCount += 1;
            }
            else
            {
                cCount += 1;
            }
        }
        return (vowelsCount, cCount);
    }

    public static List<int> UniqueStable(int[] nums)
    {
        // return nums.ToHashSet().ToList();
        HashSet<int> numsC = new HashSet<int>();
        foreach (int num in nums)
        {
            if (!numsC.Contains(num))
            {
                numsC.Add(num);
            }
        }
        return numsC.ToList();
    }

    public static Dictionary<char, int> CharFreq(string text)
    {
        return text.Where(x => char.IsLetterOrDigit(x)).GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
    }

    public sealed class StopwatchLite
    {

        private DateTime? _startTime;
        private DateTime? _stopTime;
        private TimeSpan _elapsed = TimeSpan.Zero;
        public void Start()
        {
            if (_startTime == null)
            {
                _startTime = DateTime.UtcNow;
            }
            else
            {
                Console.WriteLine("Уже запущен");
            }

        }

        public void Stop()
        {
            if (_startTime != null)
            {
                _elapsed += DateTime.UtcNow - _startTime.Value;
                _startTime = null;
            }
        }

        public void Reset()
        {
            _elapsed = TimeSpan.Zero;
            _startTime = null;
        }

        public TimeSpan Elapsed
        {
            get
            {
                if (_startTime != null)
                {

                    return _elapsed + (DateTime.UtcNow - _startTime.Value);
                }
                return _elapsed;
            }
        }


    }


    public record struct Transaction(string Category, decimal Amount);

    public static Dictionary<string, (int count, decimal total)> SummarizeByCategory(IEnumerable<Transaction> txs)
    {
        return txs.GroupBy(x => x.Category).ToDictionary(x => x.Key, y => (y.Count(), y.Sum(t => t.Amount)));
    }


    //11-15
    public static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var sb = new StringBuilder(text.Length);
        bool inWhitespace = false;

        foreach (char l in text)
        {
            if (char.IsWhiteSpace(l))
            {
                if (!inWhitespace)
                {
                    sb.Append(' ');
                    inWhitespace = true;
                }
            }
            else
            {
                sb.Append(l);
                inWhitespace = false;

            }
        }
        return sb.ToString().Trim();
    }

    public readonly record struct Interval(int Start, int End);
    public static List<Interval> MergeIntervals(IEnumerable<Interval> intervals)
    {
        List<Interval> res = [];
        List<Interval> sorted_intervals = intervals.OrderBy(x => x.Start).ToList();
        if (sorted_intervals.Count == 0) return [];

        int curStart = sorted_intervals[0].Start;
        int curEnd = sorted_intervals[0].End;

        for (int i = 1; i < sorted_intervals.Count; i++)
        {
            var curr = sorted_intervals[i];


            if (curr.Start <= curEnd)
            {

                if (curr.End > curEnd) curEnd = curr.End;
            }
            else
            {

                res.Add(new Interval(curStart, curEnd));

                curStart = curr.Start;
                curEnd = curr.End;
            }
        }
        res.Add(new Interval(curStart, curEnd));
        return res;

    }


    public static Dictionary<string, List<string>> ParseKv(string text)
    {
        Dictionary<string, List<string>> res = new(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text)) return res;
        IEnumerable<string> splittedText = text.Split(';');
        foreach (var raw in splittedText)
        {
            var segment = raw.Trim();
            if (segment.Length == 0) continue;

            int eq = segment.IndexOf('=');
            if (eq < 0) continue;

            var key = segment.Substring(0, eq).Trim();
            var value = segment.Substring(eq + 1).Trim();

            if (key.Length == 0) continue;

            if (!res.TryGetValue(key, out var list))
            {
                list = new List<string>();
                res[key] = list;
            }
            list.Add(value);
        }
        return res;
    }

    public sealed class Purchase1 { public string User = ""; public string Item = ""; public int Qty; }
    public static List<(string user, int totalQty)> TopKUsersByQty(IEnumerable<Purchase1> purchases, int k)
    {
        return purchases.GroupBy(x => x.User).Select(x => (User: x.Key, totalQty: x.Sum(g => g.Qty))).OrderByDescending(y => y.totalQty).ThenBy(t => t.User, StringComparer.Ordinal).Take(k).ToList();
    }


    //16-20
    public sealed class AsyncRetryPolicy
    {
        private int _maxRetries;
        private Func<int, TimeSpan> _delayProvider;
        private Func<Exception, bool> _shouldHandle;
        private readonly Func<int, TimeSpan, TimeSpan>? _jitter;

        public AsyncRetryPolicy(int maxRetries, Func<int, TimeSpan> delayProvider, Func<Exception, bool> shouldHandle, Func<int, TimeSpan, TimeSpan>? jitter = null) {
            _maxRetries = maxRetries;
            _delayProvider = delayProvider;
            _shouldHandle = shouldHandle;
            _jitter = jitter;
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(action);

            Exception? lastError = null;
            for (int i = 0; i <= _maxRetries; i++) {
                ct.ThrowIfCancellationRequested();

                if (i > 1)
                {
                    var baseDelay = _delayProvider(i);
                    var delay = _jitter is null ? baseDelay : _jitter(i, baseDelay);
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, ct).ConfigureAwait(false);
                    }

                }

                try
                {
                    return await action(ct).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (!_shouldHandle(ex))
                    {
                        throw;
                    }
                    lastError = ex;
                    if (i == _maxRetries)
                    {
                        throw;
                    }
                }
            }
            throw lastError ?? new InvalidOperationException("Unexpected retry policy state.");
        }
    }

    public interface IEvent { }

    public readonly record struct UserRegistered(Guid UserId, string Email) : IEvent;
    public readonly record struct OrderPlaced(Guid OrderId, Guid UserId, decimal Total) : IEvent;

    public sealed class EventBus
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

        public IDisposable Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var list = _handlers.GetOrAdd(typeof(T), _ => new List<Delegate>());
            lock (list)
            {
                list.Add(handler);
            }
            return new Subscription(this, typeof(T), handler);
        }

        public void Publish<T>(T evt) where T : IEvent
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;

            Delegate[] snapshot;
            lock (list) snapshot = list.ToArray();

            List<Exception>? errors = null;
            foreach (var del in snapshot)
            {
                var h = (Action<T>)del;
                try { h(evt); }
                catch (Exception ex) { (errors ??= new()).Add(ex); }
            }
            if (errors is not null) throw new AggregateException(errors);
        }

        private void Unsubscribe(Type type, Delegate handler)
        {
            if (!_handlers.TryGetValue(type, out var list)) return;
            lock (list)
            {
                list.Remove(handler);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly EventBus _bus;
            private readonly Type _type;
            private readonly Delegate _handler;
            private bool _disposed;

            public Subscription(EventBus bus, Type type, Delegate handler)
            {
                _bus = bus; _type = type; _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _bus.Unsubscribe(_type, _handler);
            }
        }
    }


    public interface IMessage { }

    public sealed record TextMessage(string Text) : IMessage;
    public sealed record NumberMessage(int Value) : IMessage;

    public sealed class SimpleMessageBus
    {
        private readonly ConcurrentDictionary<Type, List<(Delegate handler, Delegate filter)>> _handlers = new();


        public IDisposable Subscribe<T>(Func<T, bool> filter, Action<T> handler) where T : IMessage
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var list = _handlers.GetOrAdd(typeof(T), _ => new List<(Delegate handler, Delegate filter)>());
            lock (list)
            {
                list.Add((handler, filter));
            }
            return new Subscription(this, typeof(T), handler, filter);

        }

        public void Publish<T>(T Message) where T : IMessage
        {
            var listException = new List<Exception>();
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;

            (Delegate handler, Delegate filter)[] snapshot;
            lock (list)
            {
                snapshot = list.ToArray();
            }

            foreach (var el in snapshot)
            {
                var handler = (Action<T>)el.handler;
                var filter = (Func<T, bool>)el.filter;
                try
                {
                    if (filter(Message))
                    {
                        handler(Message);
                    }
                }
                catch (Exception ex)
                {
                    listException.Add(ex);
                }
            }

            if (listException.Count > 0)
            {
                throw new AggregateException(listException);
            }
        }



        private void Unsubscribe(Type type, Delegate handler, Delegate filter)
        {
            if (!_handlers.TryGetValue(type, out var list)) return;
            lock (list)
            {
                list.Remove((handler, filter));
                if (list.Count == 0) _handlers.TryRemove(type, out _);
            }


        }

        private sealed class Subscription : IDisposable
        {
            private readonly SimpleMessageBus _bus;
            private readonly Type _type;
            private readonly Delegate _handler;
            private readonly Delegate _filter;
            private bool _disposed;

            public Subscription(SimpleMessageBus bus, Type type, Delegate handler, Delegate filter)
            {
                _bus = bus; _type = type; _handler = handler; _filter = filter;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _bus.Unsubscribe(_type, _handler, _filter);
            }
        }
    }


    public sealed record Request(string Path, IReadOnlyDictionary<string, string> Headers);
    public sealed record Response(int StatusCode, string Body);

    public delegate Task<Response> App(Request req, CancellationToken ct);
    public delegate App Middleware(App next);

    public sealed class PipelineBuilder
    {
        private readonly List<Middleware> _middlewares = [];
        public PipelineBuilder Use(Middleware m)
        {
            _middlewares.Add(m);
            return this;
            
        }
        public App Build(App terminal)
        {
            ArgumentNullException.ThrowIfNull(terminal);
            App current = terminal;
            for (int i = _middlewares.Count - 1; i >= 0; i--)
                current = _middlewares[i](current);
            return current;
        }


    }

    public static Middleware UseExceptions() {
        return next =>
        {
            return async (req, ct) =>
            {
                try
                {
                    return await next(req, ct);
                }
                catch (Exception ex)
                {
                    return new Response(500, "Internal Server Error");
                }
            };
        };
    }


    public interface ILogger {
        void Info(string message);
        void Error(string message);
    }

    public static Middleware UseLogger(ILogger logger)
    {
        return next =>
        {
            return async (req, ct) =>
            {
                var started = DateTime.UtcNow;
                logger.Info($"Start {req.Path}");
                try
                {
                    var resp = await next(req, ct);
                    var ms = (DateTime.UtcNow - started).TotalMilliseconds;
                    logger.Info($"End {req.Path} -> {resp.StatusCode} in {ms:F1}ms");
                    return resp;
                }
                catch (Exception ex)
                {
                    var ms = (DateTime.UtcNow - started).TotalMilliseconds;
                    logger.Error($"Fail {req.Path} in {ms:F1}ms: {ex.Message}");
                    throw;
                }
            };
        };
        
    }

    public sealed class TtlAsyncCache<TKey,TValue> : IAsyncDisposable
    {
        sealed class Entry
        {
            private readonly object _gate = new();
            public TValue? Value;
            public long CreatedAt;
            public Task<TValue>? Inflight;

            public bool TryGetFreshValue(long now, long ttlMs, out TValue value)
            {
                lock (_gate)
                {
                    if (Value is not null && (now - CreatedAt) < ttlMs)
                    {
                        value = Value;
                        return true;
                    }
                }
                value = default!;
                return false;
            }
            public Task<TValue>? TryGetInflight()
            {
                lock (_gate)
                {
                    return Inflight;
                }
            }

            public Task<TValue> StartInflight(Func<Task<TValue>> start)
            {
                lock (_gate)
                {
                    if (Inflight is null)
                    {
                        Inflight = start();
                    }
                    return Inflight;
                }
            }
            
            public void Commit(TValue newValue, long now)
            {
                lock (_gate)
                {
                    Value = newValue;
                    CreatedAt = now;
                    Inflight = null;
                }
            }

            public void RollbackInflight()
            {
                lock (_gate)
                {
                    Inflight = null;
                }
            }

            public bool IsExpired(long now, long ttlMs)
            {
                lock (_gate)
                {
                    return Value is null || (now - CreatedAt) >= ttlMs;
                }
            }

            public TValue? TakeValueForDispose()
            {
                lock (_gate)
                {
                    var v = Value;
                    Value = default;
                    Inflight = null;
                    return v;
                }
            }

        }
        
        private readonly long _ttlMs;
        private readonly ConcurrentDictionary<TKey, Entry> _entries;

        public TtlAsyncCache(TimeSpan ttl, IEqualityComparer<TKey>? cmp = null)
        {
            if (ttl <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(ttl));
            _ttlMs = (long)ttl.TotalMilliseconds;
            _entries = new ConcurrentDictionary<TKey, Entry>(cmp ?? EqualityComparer<TKey>.Default);
        }

        public ValueTask<TValue> GetOrAddAsync(
            TKey key,
            Func<CancellationToken, ValueTask<TValue>> factory,
            CancellationToken ct)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (factory is null) throw new ArgumentNullException(nameof(factory));

            var now = Environment.TickCount64;
            var entry = _entries.GetOrAdd(key, _ => new Entry());

           
            if (entry.TryGetFreshValue(now, _ttlMs, out var fresh))
                return ValueTask.FromResult(fresh);

            
            var inflight = entry.TryGetInflight();
            if (inflight is not null)
                return AwaitShared(inflight, entry, now, ct);

           
            var started = entry.StartInflight(() =>
            {
                
                var vt = factory(CancellationToken.None);
                return vt.IsCompletedSuccessfully ? Task.FromResult(vt.Result) : vt.AsTask();
            });

            return AwaitShared(started, entry, now, ct);

            static async ValueTask<TValue> AwaitShared(Task<TValue> task, Entry e, long now, CancellationToken ct)
            {
                try
                {
                    var value = await task.WaitAsync(ct).ConfigureAwait(false);
                    e.Commit(value, now);
                    return value;
                }
                catch (OperationCanceledException)
                {
                    
                    throw;
                }
                catch
                {
                    
                    e.RollbackInflight();
                    throw;
                }
            }



        }
        public int Count
        {
            get
            {
                var now = Environment.TickCount64;
                int cnt = 0;
                foreach (var kv in _entries)
                {
                    var entry = kv.Value;
                    if (entry.IsExpired(now, _ttlMs))
                    {
                       
                        if (entry.TryGetInflight() is null)
                        {
                            _entries.TryRemove(kv.Key, out _);
                        }
                    }
                    else
                    {
                        cnt++;
                    }
                }
                return cnt;
            }
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var kv in _entries)
            {
                var v = kv.Value.TakeValueForDispose();
                if (v is null) continue;

                switch (v)
                {
                    case IAsyncDisposable iad:
                        await iad.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable d:
                        d.Dispose();
                        break;
                }
            }

            _entries.Clear();
            await Task.CompletedTask;
        }
    }



    public record Article
    (
        Guid Id,
        string Title,
        string Content,
        DateTime CreatedAt
    );


    public class NewsService
    {
        private List<Article> _articles = new();

        public event Action<Article>? ArticleAdded;

        public NewsService(IEnumerable<Article>? seed = null)
        {
            _articles = seed is null ? new List<Article>() : [.. seed]; ;
        }

        public void Add(Article article)
        {
            if (article is null) throw new ArgumentNullException(nameof(article));
            _articles.Add(article);
            ArticleAdded?.Invoke(article);

        }

        public IReadOnlyList<Article> GetRecent(int count)
        {
            if (count <= 0) return Array.Empty<Article>();
            return _articles.OrderByDescending(a => a.CreatedAt).Take(count).ToList();
        }

        public bool TryFind(string keyword, out Article? articleRes)
        {
            articleRes = null;
            if (string.IsNullOrWhiteSpace(keyword)) return false;

            articleRes = _articles.Find(a => a.Title.ToLower().Contains(keyword.ToLower()) || a.Content.ToLower().Contains(keyword.ToLower()));
            if (articleRes == null) return false;
            return true;

        }
        

        public IReadOnlyList<Article> Find(string keyword)
        {
            return _articles.Where(a => a.Title.ToLower().Contains(keyword.ToLower()) || a.Content.ToLower().Contains(keyword.ToLower())).ToList();
        }
    }

    public class CachedHttpClient : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly TtlAsyncCache<Uri, string> _cache;

        public CachedHttpClient(HttpClient? httpClient = null, TimeSpan? ttl = null)
        {
            _httpClient = httpClient ?? new HttpClient();

            var cacheTtl = ttl ?? TimeSpan.FromSeconds(10);
            _cache = new TtlAsyncCache<Uri, string>(cacheTtl);
        }

        public async ValueTask DisposeAsync()
        {
            await _cache.DisposeAsync().ConfigureAwait(false);

        }

        public async Task<string> GetAsync(Uri url, TimeSpan timeout, CancellationToken ct)
        {
            if (url is null) throw new ArgumentNullException(nameof(url));
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

            async ValueTask<string> Factory(CancellationToken _)
            {
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

                var lct = linkedCts.Token;

                using var resp = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, lct).ConfigureAwait(false);

                resp.EnsureSuccessStatusCode();

                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                return body;
            }

            return await _cache.GetOrAddAsync(url, Factory, ct).ConfigureAwait(false);
        }
    }


    public static class Stats
    {
        public static (double avg, double median, int min, int max) Analyze(ReadOnlySpan<int> data)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            var sorted = data.ToArray();
            Array.Sort(sorted);
            double median = (sorted.Length % 2 == 1)
                ? sorted[sorted.Length / 2]
                : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2.0;
            int total = 0;
            foreach (int n in data)
            {
                if (n < min) min = n;
                if (n > max) max = n;
                total += n;
            }
            return ((double)total / data.Length, median, min, max);
        }
    }


    public static void Main(string[] args)
    {
        // string text = "Hello world! Hello C# world, hello!";
        // var freq = WordFreq(text);
        // foreach (var lala in freq)
        // {
        //     Console.WriteLine(lala.Key + " : " + lala.Value);
        // }

        Console.WriteLine(CountLetters("Hello, World!"));

    }
}