# Kiribell.PricePlugin.TwelveData.Sample

A working sample that retrieves prices from Twelve Data's `/quote` endpoint. It only handles watch-list symbols with the `SMP.` prefix.

**日本語版: [README.ja.md](README.ja.md)**

| Kiribell code | Symbol sent to Twelve Data |
| --- | --- |
| `SMP.NVDA` | `NVDA` |
| `SMP.AAPL` | `AAPL` |

The plugin retrieves prices once per minute and publishes `close`, `open`, `high`, `low`, and `previous_close` to Kiribell. After selecting the DLL in Kiribell's **Settings → Extensions**, choose **Configure plugin** to enter and save a Twelve Data API key. The key is saved as part of Kiribell's plugin settings through `ConfigurationJson`. This is not a vault dedicated to secrets, so handle the API key according to the requirements of the service you use.

```powershell
dotnet build samples\Kiribell.PricePlugin.TwelveData.Sample\Kiribell.PricePlugin.TwelveData.Sample.csproj -c Release
```

Select the generated `Kiribell.PricePlugin.TwelveData.Sample.dll` in **Settings → Extensions**, turn on **Enable external DLL**, and save. If you extend it with external libraries, place the required build outputs, including dependency DLLs and `.deps.json`, in the same folder as the plugin DLL.

Twelve Data API terms, pricing, rate limits, coverage, and data use and redistribution conditions vary by plan. See the [official API documentation](https://twelvedata.com/docs/advanced/api-usage). For external APIs, follow each provider's terms of use and API usage conditions. A price plugin does not by itself guarantee real-time data.

To develop your own plugin, see the [price plugin development guide](../../docs/Kiribell-PricePlugin-Guide.md).
