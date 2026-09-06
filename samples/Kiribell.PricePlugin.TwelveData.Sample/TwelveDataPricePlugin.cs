using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Kiribell.Plugins;

namespace Kiribell.PricePlugin.TwelveData.Sample;

public sealed class TwelveDataPricePlugin : IPriceUpdatePublisher, IConfigurablePricePlugin, ISettingsPricePlugin
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private readonly object _sync = new();
    private IReadOnlyCollection<Watch> _watches = [];
    private CancellationTokenSource? _cancellation;
    private Task? _updateTask;

    public string Name => "Twelve Data 価格プラグイン サンプル";
    public string? ConfigurationJson { get; set; }
    public event EventHandler<PriceUpdateBatch>? PricesUpdated;

    public bool ShowSettings(nint ownerWindowHandle)
    {
        using var dialog = new TwelveDataSettingsDialog(ConfigurationJson);
        var result = ownerWindowHandle == 0 ? dialog.ShowDialog() : dialog.ShowDialog(new WindowOwner(ownerWindowHandle));
        if (result != System.Windows.Forms.DialogResult.OK) return false;
        ConfigurationJson = dialog.ConfigurationJson;
        return true;
    }

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
                PricesUpdated?.Invoke(this, await FetchAllAsync(watches, token));
                await Task.Delay(TimeSpan.FromMinutes(1), token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    private async Task<PriceUpdateBatch> FetchAllAsync(IReadOnlyCollection<Watch> watches, CancellationToken token)
    {
        var apiKey = ParseConfiguration()?.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            return new PriceUpdateBatch(new Dictionary<string, Quote>(), "プラグイン設定でTwelve Data APIキーを入力してください", DateTimeOffset.UtcNow);

        var quotes = new Dictionary<string, Quote>(StringComparer.Ordinal);
        var errors = new List<string>();
        foreach (var watch in watches)
        {
            var symbol = ToTwelveDataSymbol(watch.Code);
            if (symbol is null) continue;
            try
            {
                var result = await FetchQuoteAsync(symbol, apiKey, token);
                if (result.Quote is { } quote) quotes[watch.Code] = quote;
                else errors.Add($"{watch.Code}: {result.Error}");
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
            catch (Exception exception) { errors.Add($"{watch.Code}: {exception.Message}"); }
        }
        return new PriceUpdateBatch(quotes, errors.Count == 0 ? null : string.Join(" / ", errors), DateTimeOffset.UtcNow);
    }

    private static async Task<(Quote? Quote, string Error)> FetchQuoteAsync(string symbol, string apiKey, CancellationToken token)
    {
        var url = $"https://api.twelvedata.com/quote?symbol={Uri.EscapeDataString(symbol)}&apikey={Uri.EscapeDataString(apiKey)}";
        using var response = await Http.GetAsync(url, token);
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);
        var root = document.RootElement;
        if (!response.IsSuccessStatusCode || root.TryGetProperty("status", out var status) && status.GetString() == "error")
            return (null, GetError(root, response.StatusCode.ToString()));
        var lastPrice = TryGetDecimal(root, "close");
        if (lastPrice is null) return (null, "現在値が返されませんでした");
        return (new Quote(lastPrice.Value, TryGetDecimal(root, "open"), TryGetDecimal(root, "high"), TryGetDecimal(root, "low"), TryGetDecimal(root, "previous_close")), string.Empty);
    }

    private static string? ToTwelveDataSymbol(string code)
    {
        const string prefix = "SMP.";
        return code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && code.Length > prefix.Length ? code[prefix.Length..] : null;
    }

    private static decimal? TryGetDecimal(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
                ? number : null;

    private static string GetError(JsonElement root, string fallback)
        => root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String
            ? message.GetString() ?? fallback : fallback;

    private TwelveDataConfiguration? ParseConfiguration()
    {
        try
        {
            return string.IsNullOrWhiteSpace(ConfigurationJson) ? null : JsonSerializer.Deserialize<TwelveDataConfiguration>(ConfigurationJson);
        }
        catch (JsonException) { return null; }
    }
}
