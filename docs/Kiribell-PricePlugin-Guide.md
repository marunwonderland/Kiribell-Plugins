# Build a Price Plugin from the Twelve Data Sample

`Kiribell.PricePlugin.TwelveData.Sample` is a working sample that retrieves prices from an external API and publishes them to Kiribell.

Copy this sample and replace the price retrieval parts to create your own price plugin.

**日本語版: [Kiribell-PricePlugin-Guide.ja.md](Kiribell-PricePlugin-Guide.ja.md)**

## About `SMP.` in the sample

To verify the Twelve Data sample, add this watch:

```text
SMP.NVDA
```

`SMP.` is not a symbol prefix required by Kiribell price plugins.

**This prefix is used only by this sample to make it easy to identify prices retrieved by the sample plugin.**

Kiribell normally retrieves prices using codes such as `US.NVDA`. If the sample plugin retrieves the same symbol under the same code, the price from Kiribell and the plugin may be identical, making it difficult to tell which one updated the display.

The sample therefore works as follows:

```text
Kiribell watch code      SMP.NVDA
                           ↓
Sample plugin           converts to NVDA
                           ↓
Twelve Data              retrieves the NVDA price
                           ↓
Sample plugin           publishes it as SMP.NVDA
                           ↓
Kiribell                 displays the price for SMP.NVDA
```

If a price appears for `SMP.NVDA`, you can confirm that it was retrieved by the Twelve Data sample plugin.

### `SMP.` is not needed in a production plugin

You do not need to use `SMP.` when operating your own plugin. For example, a plugin that retrieves US stock prices from Twelve Data can receive:

```text
US.NVDA
```

convert it to `NVDA` for the Twelve Data request, and return the price using the original code `US.NVDA`:

```text
US.NVDA
   ↓
Price plugin
   ↓
NVDA
   ↓
External API
   ↓
Quote
   ↓
US.NVDA
```

Think of `SMP.` only as a way to verify that the sample plugin is working correctly.

---

## Create a new plugin

You can copy `Kiribell.PricePlugin.TwelveData.Sample` to create a new price plugin. The main parts to change are:

1. Plugin name
2. Conversion from Kiribell symbol codes to API symbol codes
3. API request
4. Conversion of the API response into a `Quote`

You can keep `StartAsync`, `SetWatches`, `StopAsync`, periodic retrieval, and publishing prices to Kiribell as they are.

## 1. Change the plugin name

Change the sample's name:

```csharp
public string Name =>
    "Twelve Data 価格プラグイン サンプル";
```

For example:

```csharp
public string Name =>
    "My Price Plugin";
```

## 2. Change symbol conversion

For verification, the sample removes `SMP.` before requesting data from Twelve Data:

```csharp
private static string? ToTwelveDataSymbol(string code)
{
    const string prefix = "SMP.";

    return code.StartsWith(
        prefix,
        StringComparison.OrdinalIgnoreCase)
            && code.Length > prefix.Length
        ? code[prefix.Length..]
        : null;
}
```

Thus, `SMP.NVDA` becomes `NVDA`.

### Change it for production use

For example, to retrieve the Kiribell US stock code `US.NVDA` as `NVDA` from Twelve Data, change the code as follows:

```csharp
private static string? ToTwelveDataSymbol(string code)
{
    const string prefix = "US.";

    return code.StartsWith(
        prefix,
        StringComparison.OrdinalIgnoreCase)
            && code.Length > prefix.Length
        ? code[prefix.Length..]
        : null;
}
```

This enables retrieval for `US.NVDA → NVDA`, `US.AAPL → AAPL`, and `US.MSFT → MSFT`.

**This is the first part to change when turning the sample into a plugin for actual use.**

For another price API, convert the code to the format required by that API. For example, `JP.7203 → 7203.T` is also valid. The code sent to the API and the code returned to Kiribell can be different.

## 3. Change the API request

`FetchQuoteAsync` retrieves the price. The Twelve Data sample makes its request as follows:

```csharp
var url =
    $"https://api.twelvedata.com/quote" +
    $"?symbol={Uri.EscapeDataString(symbol)}" +
    $"&apikey={Uri.EscapeDataString(apiKey)}";

using var response =
    await Http.GetAsync(url, token);
```

You do not need to change this part if you continue using Twelve Data. To use another price service, replace it with that service's API call. Follow the service's terms of use and API usage conditions; availability, pricing, limits, and latency depend on the service and plan.

## 4. Convert API values into a `Quote`

The Twelve Data JSON is converted to Kiribell's `Quote` inside `FetchQuoteAsync`:

```csharp
return (
    new Quote(
        lastPrice.Value,
        TryGetDecimal(root, "open"),
        TryGetDecimal(root, "high"),
        TryGetDecimal(root, "low"),
        TryGetDecimal(root, "previous_close")),
    string.Empty);
```

The `Quote` values, in order, are `LastPrice`, `OpenPrice`, `TodayHigh`, `TodayLow`, and `PreviousClose`. For another API, update this part to match the field names in its JSON response.

## 5. Keep the part that returns prices to Kiribell

`FetchAllAsync` stores a retrieved quote as follows:

```csharp
if (result.Quote is { } quote)
    quotes[watch.Code] = quote;
```

This uses the original `watch.Code` received from Kiribell, **not** the `symbol` sent to the API. For example, even if `US.NVDA` is sent to the API as `NVDA`, return the quote as:

```csharp
quotes["US.NVDA"] = quote;
```

Do not change this part.

---

## The sample is nearly ready if you want to keep using Twelve Data

The sample already implements API requests, JSON parsing, `Quote` creation, retrieval every minute, watch-list changes, error notifications, an API key settings screen, saving settings, and start/stop handling.

To retrieve US stocks from Twelve Data, the main change is to replace:

```csharp
const string prefix = "SMP.";
```

with:

```csharp
const string prefix = "US.";
```

This changes the sample mapping `SMP.NVDA → NVDA → Twelve Data` into a plugin mapping `US.NVDA → NVDA → Twelve Data`, `US.AAPL → AAPL → Twelve Data`, and `US.MSFT → MSFT → Twelve Data`.

For another price service, also adapt the API request in `FetchQuoteAsync` and conversion of the response into a `Quote`.

## Summary

`Kiribell.PricePlugin.TwelveData.Sample` is not just a demonstration; you can use it as a base for your own price plugin. If you continue using Twelve Data, start by **changing `SMP.` to the market code you actually use**.

For another price service, you can keep the basic Kiribell integration and focus your changes on **symbol conversion → API request → conversion to `Quote`**.
