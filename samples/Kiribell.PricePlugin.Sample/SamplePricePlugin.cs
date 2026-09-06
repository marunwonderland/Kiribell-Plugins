using Kiribell.Plugins;

namespace Kiribell.PricePlugin.Sample;

public sealed class SamplePricePlugin : IPriceUpdatePublisher
{
    private IReadOnlyCollection<Watch> _watches = [];
    private CancellationTokenSource? _cancellation;
    public string Name => "価格プラグイン サンプル";
    public event EventHandler<PriceUpdateBatch>? PricesUpdated;

    public Task StartAsync(IReadOnlyCollection<Watch> watches, CancellationToken token = default)
    {
        _watches = watches.ToArray();
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        return Task.CompletedTask;
    }

    public void SetWatches(IReadOnlyCollection<Watch> watches) => _watches = watches.ToArray();

    public Task StopAsync()
    {
        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;
        return Task.CompletedTask;
    }

    private void Publish(IReadOnlyDictionary<string, Quote> quotes, string? error = null)
    {
        if (_cancellation?.IsCancellationRequested != false) return;
        PricesUpdated?.Invoke(this, new PriceUpdateBatch(quotes, error, DateTimeOffset.UtcNow));
    }
}
