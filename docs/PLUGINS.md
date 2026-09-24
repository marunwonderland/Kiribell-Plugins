# Kiribell Price Plugin

Kiribell loads a price plugin from the DLL selected in Settings. The plugin determines when and how often to retrieve prices, then notifies the application of the results. The public plugin API is defined in [`../src/StockBeacon.PluginContracts/IPriceUpdatePublisher.cs`](../src/StockBeacon.PluginContracts/IPriceUpdatePublisher.cs).

**日本語版: [PLUGINS.ja.md](PLUGINS.ja.md)**

## Creating and installing a plugin

For an initial check, use [`Kiribell.PricePlugin.JsonFile.Sample`](../samples/Kiribell.PricePlugin.JsonFile.Sample/). This working sample reads `prices.json` from the same folder as the DLL every two seconds and publishes prices. Edit a price in the file to verify plugin notifications and updates in Kiribell.

For an implementation that calls an external API, see [`Kiribell.PricePlugin.TwelveData.Sample`](../samples/Kiribell.PricePlugin.TwelveData.Sample/). It looks up the watch-list code `SMP.NVDA` as `NVDA` in Twelve Data and returns the price under the original code `SMP.NVDA`. It includes API key configuration, periodic retrieval, and error notifications.

[`Kiribell.PricePlugin.Sample`](../samples/Kiribell.PricePlugin.Sample/) is a minimal template with no retrieval logic. Use it as a starting point for a new plugin.

```powershell
dotnet build samples\Kiribell.PricePlugin.JsonFile.Sample\Kiribell.PricePlugin.JsonFile.Sample.csproj -c Release
```

Select `Kiribell.PricePlugin.JsonFile.Sample.dll` in the output folder `bin\Release\net10.0\`. In Kiribell, open **Settings → Extensions**, select the DLL, turn on **Enable external DLL**, and save. Add `JP.7203` or `US.AAPL` to the watch list, then edit `LastPrice` in `prices.json` in the same folder and save. The plugin reads the file every two seconds; Kiribell applies the latest notification every five seconds.

As in the samples, reference `StockBeacon.PluginContracts` from your plugin.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\StockBeacon.PluginContracts\StockBeacon.PluginContracts.csproj" />
  </ItemGroup>
</Project>
```

Include exactly one implementation of `IPriceUpdatePublisher` that references `StockBeacon.PluginContracts` in the DLL. Kiribell counts concrete implementations: if it finds none, loading stops with `IPriceUpdatePublisher 実装が見つかりません`; if it finds two or more, loading stops with `IPriceUpdatePublisher 実装はDLL内に一つだけ配置してください`.

If Kiribell cannot instantiate the selected type, plugin loading fails. For example, the type must have a parameterless constructor or `MissingMethodException` is thrown. The settings screen displays `プラグインの読み込みに失敗しました: <例外メッセージ>`, and the log records `Plugin Load Error` with exception details.

## Plugin API

```csharp
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
```

`IPriceUpdatePublisher` is required. Add `IConfigurablePricePlugin` and `ISettingsPricePlugin` only when needed.

## Communication with Kiribell

Kiribell calls `StartAsync` when the application loads and when the plugin is enabled by saving settings. Its argument contains all watch-list symbols except test data. Kiribell calls `SetWatches` after symbols are added, edited, or removed. It calls `StopAsync` when the application exits and when settings are saved. Kiribell does not pass a `CancellationToken` to these calls.

The plugin retrieves prices at any times and intervals and raises `PricesUpdated`. Kiribell has a five-second polling timer that takes and applies the latest `PriceUpdateBatch` held at that time. This timer does not ask the plugin to retrieve prices.

Kiribell holds only one received `PriceUpdateBatch`. It replaces the held batch only when the new batch's `ObtainedAt` is later than the held batch's timestamp. When applying a batch, Kiribell removes it from the holding slot. Batches with the same or an earlier timestamp are discarded.

## Publishing prices

`Watch.Code` is the watch-list code stored in Kiribell. You may convert it to a code required by a service, but keys in `PriceUpdateBatch.Quotes` must use the original `Watch.Code` received from Kiribell. For each watch, Kiribell looks up a quote with `Quotes.TryGetValue(row.Code, …)`. A watch with no matching key is not updated.

| Value | Behavior in Kiribell |
| --- | --- |
| `LastPrice` | Applied as the current price for the matching symbol. |
| `OpenPrice` | Applied only when a value is provided, the current day's open has not already been applied (the open's market date is not today or the open is 0 or less), and it is after the market open on a weekday. Market open is 9:00 Japan time for `JP`, 9:30 US Eastern time for `US`, and 9:30 China time for `HK`. Plugins cannot apply an open price for other codes. |
| `TodayHigh` / `TodayLow` | Applied only when a value is provided. |
| `PreviousClose` | Applied as the previous close only when a value is provided. |

Once a symbol receives a plugin price, the plugin owns its price state: regular price updates no longer update its current, open, high, or low prices. Regular updates continue to update only the previous close.

Kiribell defers applying a candidate current price if it is at least 50% away from the symbol's immediately preceding current price and matches the current price of another watch-list symbol. This check helps prevent applying a price to the wrong symbol when OCR, for example, causes rows to move.

If `Error` is not `null`, Kiribell displays it as a plugin error. An empty string is also an error because it is not `null`. Prices in `Quotes` are applied even when `Error` is present.

## Plugin settings

When a DLL implementing `IConfigurablePricePlugin` is loaded, Kiribell sets its saved `ConfigurationJson` immediately after loading. The value is saved in Kiribell settings per absolute DLL path. Switching to another DLL does not change that DLL's saved configuration; selecting the original DLL again supplies its saved value.

Implement `ISettingsPricePlugin` to show a settings screen. Kiribell calls `ShowSettings` when the user presses the settings button.

```csharp
public bool ShowSettings(nint ownerWindowHandle)
```

Kiribell reads and saves `ConfigurationJson` only if the call returns `true` and the plugin also implements `IConfigurablePricePlugin`. If it returns `false`, settings are not saved. If the plugin does not implement `ISettingsPricePlugin`, Kiribell displays `選択したDLLは設定画面に対応していません`.

## Errors and logs

In Kiribell, open **Settings → Extensions → Error Log** and turn on **Save errors to log file** to record DLL load, start, stop, and settings-screen exceptions, along with errors reported by the plugin through `PriceUpdateBatch.Error`. The log is written to:

```text
%LOCALAPPDATA%\Kiribell\Data\error.log
```

This option is on by default in Debug builds and off by default in Release builds. To report failures that occur during a plugin's autonomous update loop to the screen and log, set `PriceUpdateBatch.Error` and publish the batch.

## DLL notes

Kiribell uses a separate load context for each plugin. After stopping the current plugin during settings save, you can switch to another plugin DLL with the same assembly name.

## Included implementations

- [`Kiribell.PricePlugin.JsonFile.Sample`](../samples/Kiribell.PricePlugin.JsonFile.Sample/) is a working sample that reads prices from a file.
- [`Kiribell.PricePlugin.TwelveData.Sample`](../samples/Kiribell.PricePlugin.TwelveData.Sample/) is a working sample that retrieves prices from an external API. It handles codes prefixed with `SMP.`, such as `SMP.NVDA`, and accepts the API key in the plugin settings screen. External API access remains subject to the provider's terms and API usage conditions; data latency and availability depend on the provider and plan.
- [`Kiribell.PricePlugin.Sample`](../samples/Kiribell.PricePlugin.Sample/) is a minimal template with no retrieval logic.
