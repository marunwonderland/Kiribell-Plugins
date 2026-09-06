using System.Text.Json;
using Kiribell.Plugins;

namespace Kiribell.PricePlugin.JsonFile.Sample;

public sealed class JsonFilePricePlugin : IPriceUpdatePublisher
{
    private readonly object _sync = new();
    private IReadOnlyCollection<Watch> _watches = [];
    private CancellationTokenSource? _cancellation;
    private Task? _updateTask;

    public string Name => "JSONファイル価格プラグイン サンプル";
    public event EventHandler<PriceUpdateBatch>? PricesUpdated;

    public Task StartAsync(IReadOnlyCollection<Watch> watches, CancellationToken token = default)
    {
        lock (_sync)
        {
            _watches = watches.ToArray();
            if (_updateTask is not null) return Task.CompletedTask;
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            _updateTask = RunUpdatesAsync(_cancellation.Token);
        }
        return Task.CompletedTask;
    }

    public void SetWatches(IReadOnlyCollection<Watch> watches)
    {
        lock (_sync) _watches = watches.ToArray();
    }

    public async Task StopAsync()
    {
        Task? updateTask;
        CancellationTokenSource? cancellation;
        lock (_sync)
        {
            updateTask = _updateTask;
            cancellation = _cancellation;
            _updateTask = null;
            _cancellation = null;
        }
        if (cancellation is null) return;
        cancellation.Cancel();
        try { if (updateTask is not null) await updateTask; }
        catch (OperationCanceledException) { }
        finally { cancellation.Dispose(); }
    }

    private async Task RunUpdatesAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                IReadOnlyCollection<Watch> watches;
                lock (_sync) watches = _watches;
                PricesUpdated?.Invoke(this, await ReadFileAsync(watches, token));
                await Task.Delay(TimeSpan.FromSeconds(2), token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    private static async Task<PriceUpdateBatch> ReadFileAsync(IReadOnlyCollection<Watch> watches, CancellationToken token)
    {
        var path = Path.Combine(Path.GetDirectoryName(typeof(JsonFilePricePlugin).Assembly.Location)!, "prices.json");
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var fileQuotes = await JsonSerializer.DeserializeAsync<Dictionary<string, Quote>>(stream, cancellationToken: token)
                ?? new Dictionary<string, Quote>();
            var quotes = new Dictionary<string, Quote>(StringComparer.Ordinal);
            foreach (var watch in watches)
                if (fileQuotes.TryGetValue(watch.Code, out var quote)) quotes.Add(watch.Code, quote);
            return new PriceUpdateBatch(quotes, null, DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            return new PriceUpdateBatch(new Dictionary<string, Quote>(), $"prices.json を読み込めませんでした: {exception.Message}", DateTimeOffset.UtcNow);
        }
    }
}
