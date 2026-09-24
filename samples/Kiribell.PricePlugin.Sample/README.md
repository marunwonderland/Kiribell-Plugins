# Kiribell.PricePlugin.Sample

A minimal template for a Kiribell price plugin. `SamplePricePlugin` implements `IPriceUpdatePublisher`.

**日本語版: [README.ja.md](README.ja.md)**

The template's `StartAsync` accepts the watch list and cancellation token, but it does not yet include a price retrieval loop. In your plugin, start retrieval from `StartAsync` and add code to publish retrieved prices with `Publish`. Kiribell holds one batch at a time, keeping only a batch with a newer `ObtainedAt`, and applies it on its next poll. `SetWatches` is called when the watch list changes; `StopAsync` is called when the plugin is disabled or the application exits.

This template does not return prices and should not be used for production in Kiribell.

```powershell
dotnet build samples\Kiribell.PricePlugin.Sample\Kiribell.PricePlugin.Sample.csproj -c Release
```

Place the resulting `bin\Release\net10.0\Kiribell.PricePlugin.Sample.dll` in any folder along with required dependency files. If you add external libraries, keep the dependency DLLs and `.deps.json` generated in the build output in the same folder. In Kiribell, open **Settings → Extensions**, select the plugin DLL, and turn on **Enable external DLL**.

See the [plugin API contract](../../docs/PLUGINS.md) for API contracts, error handling, and settings UI details.
