namespace Kiribell.Plugins;

public sealed record Watch(string Code, string Name);

public sealed record Quote(
    decimal LastPrice,
    decimal? OpenPrice = null,
    decimal? TodayHigh = null,
    decimal? TodayLow = null,
    decimal? PreviousClose = null);

public sealed record PriceUpdateBatch(
    IReadOnlyDictionary<string, Quote> Quotes,
    string? Error,
    DateTimeOffset ObtainedAt);

public interface IPriceUpdatePublisher
{
    string Name { get; }
    event EventHandler<PriceUpdateBatch>? PricesUpdated;
    Task StartAsync(IReadOnlyCollection<Watch> watches, CancellationToken token = default);
    void SetWatches(IReadOnlyCollection<Watch> watches);
    Task StopAsync();
}

public interface IConfigurablePricePlugin
{
    string? ConfigurationJson { get; set; }
}

public interface ISettingsPricePlugin
{
    bool ShowSettings(nint ownerWindowHandle);
}
