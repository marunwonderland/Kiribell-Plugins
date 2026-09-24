# Kiribell.PricePlugin.JsonFile.Sample

A working sample for verifying that Kiribell price plugins update correctly. It reads `prices.json` from the same folder as the DLL every two seconds and publishes its contents through `PricesUpdated`.

**日本語版: [README.ja.md](README.ja.md)**

```powershell
dotnet build samples\Kiribell.PricePlugin.JsonFile.Sample\Kiribell.PricePlugin.JsonFile.Sample.csproj -c Release
```

Select `Kiribell.PricePlugin.JsonFile.Sample.dll` from the build output folder in Kiribell. Edit and save `prices.json` in the same folder using a text editor. The plugin reads the file every two seconds; Kiribell displays the latest notification every five seconds.

`prices.json` is a JSON object keyed by symbol code. Only entries whose keys match Kiribell watch-list codes are applied.

```json
{
  "JP.7203": {
    "LastPrice": 2500,
    "PreviousClose": 2470,
    "OpenPrice": 2480,
    "TodayHigh": 2520,
    "TodayLow": 2475
  }
}
```

This sample includes start/stop handling, watch-list changes, asynchronous file I/O, partial updates, and error notifications.

See the [plugin API contract](../../docs/PLUGINS.md) for details.
