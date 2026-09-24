# Build a Kiribell Plugin with an AI Coding Agent

Kiribell lets you replace price retrieval with an external plugin.

This guide helps you ask an AI coding agent such as Codex to build a price plugin for an API provided by your broker or market data service.

**日本語版: [AI-PLUGIN-GUIDE.ja.md](AI-PLUGIN-GUIDE.ja.md)**

> [!IMPORTANT]
> Do not trust and run AI-generated code without review. Always check its behavior, applicable terms, and handling of credentials.

## Check these points first

Confirm that the service formally permits you to retrieve prices programmatically. In particular, check:

- Whether an official API, SDK, RSS feed, or similar option is available
- Terms for personal and commercial use
- Conditions for real-time price retrieval
- Rate limits
- Conditions for data storage and redistribution
- Requirements for API keys, access tokens, account information, and other credentials

Do not use unofficial scraping or any retrieval method that violates the terms of use.

## Prompt template for Codex and other agents

Copy the following text, replace `【データ提供元】` with the provider you want to use, and give it to your AI coding agent.

```text
Create a price retrieval plugin for Kiribell.

The public Kiribell Plugin SDK is here:
https://github.com/marunwonderland/Kiribell-Plugins

First inspect the README, docs/PLUGINS.md, docs/Kiribell-PricePlugin-Guide.md, and samples. Implement against the current published specification.

【Data provider】
Enter the broker, API, or service name here.

【Purpose】
Retrieve price information for Kiribell watch-list symbols and publish it to Kiribell through a plugin implementing IPriceUpdatePublisher.

【Implementation requirements】
- Use StockBeacon.PluginContracts as the Kiribell Plugin SDK contract assembly
- Implement Kiribell.Plugins.IPriceUpdatePublisher
- Include exactly one IPriceUpdatePublisher implementation class in the plugin DLL
- Make the class instantiable with no arguments
- Receive the initial watch list in StartAsync
- Apply watch-list changes in SetWatches
- Reliably stop communication, timers, event subscriptions, etc. in StopAsync
- Always use the original Watch.Code received from Kiribell as a key in PriceUpdateBatch.Quotes
- If converting to an API symbol code, map back to the original Watch.Code when returning the result
- Do not stop the entire plugin when retrieval for one symbol fails
- Handle CancellationToken appropriately
- Follow the API rate limits
- Do not hard-code API keys, passwords, tokens, etc. in source code
- Implement a settings screen with IConfigurablePricePlugin / ISettingsPricePlugin if needed
- If API credentials are saved, document their storage location and security considerations in the README
- If using external NuGet packages, include required dependency DLLs, .deps.json, etc. in the deployment output so Kiribell can load them
- Do not implement order functionality; Kiribell only needs price retrieval

【Research】
Before implementation, check the provider's latest official API specification, terms, and rate limits.
Do not use unofficial scraping or methods that violate the terms of use.
If programmatic price retrieval is not formally allowed, do not force an implementation; report why.

【Deliverables】
1. Plugin project
2. Implementation source code
3. Build instructions
4. Installation instructions for Kiribell
5. Required configuration instructions
6. Supported symbol-code conversion rules
7. README documenting API usage limits and caveats
8. Automated tests where practical

Do not change the existing Kiribell application or public Plugin SDK specification unless necessary.
```

## Do not give credentials to the AI

Do not paste API keys, access tokens, brokerage login details, passwords, or similar credentials into the prompt. Use a placeholder such as:

```text
YOUR_API_KEY
```

Configure the real credentials in your own environment after the plugin is complete.

## Minimum checks after generation

After the AI completes the implementation, check at least the following:

- Is there exactly one concrete `IPriceUpdatePublisher` implementation in the DLL?
- Can Kiribell instantiate the plugin without arguments?
- Are `StartAsync` / `SetWatches` / `StopAsync` implemented correctly?
- Do communication, timers, and background tasks stop cleanly?
- Are keys in `PriceUpdateBatch.Quotes` the original `Watch.Code` values?
- Are API keys and other credentials absent from source, logs, and test data?
- Does the API call frequency stay within rate limits?
- Does a failure for one symbol leave the retrieval loop running?
- Does an error avoid terminating the Kiribell application?
- Are required dependency DLLs and `.deps.json` present in the output folder?
- Does the provider's terms of use permit this usage?

## First run

We recommend starting with one symbol rather than registering many at once.

1. Build the plugin in Release configuration.
2. In Kiribell, open **Settings → Extensions** and select the plugin DLL.
3. Turn on **Enable external DLL**.
4. Configure the required API settings.
5. Add one applicable symbol to the watch list.
6. Confirm that prices update.
7. After closing Kiribell, confirm that no communication or process remains running.
8. Add more symbols if everything works.

## Sample plugins

Use the sample that best matches your goal:

- [`Kiribell.PricePlugin.Sample`](../samples/Kiribell.PricePlugin.Sample/) — minimal template
- [`Kiribell.PricePlugin.JsonFile.Sample`](../samples/Kiribell.PricePlugin.JsonFile.Sample/) — verify integration with Kiribell
- [`Kiribell.PricePlugin.TwelveData.Sample`](../samples/Kiribell.PricePlugin.TwelveData.Sample/) — reference for HTTP APIs, settings UI, and periodic retrieval

For implementation requirements, follow the [price plugin specification](PLUGINS.md). Kiribell's standard Japanese stock prices are delayed by approximately 15 minutes. A plugin can supplement that limitation when a suitable broker or data service API is available, but a plugin does not guarantee real-time prices. Follow each provider's terms of use and API usage conditions.

## If you distribute the plugin

Review the following carefully before distributing a custom plugin to others:

- Does the provider allow use in third-party tools?
- Are there restrictions on redistribution of API or market data?
- Are credentials configured separately by each user?
- Does the plugin collect users' account or personal information?
- Do any dependency libraries require license notices?

The Kiribell Plugin SDK's MIT License is separate from the terms of the connected service.

## What Kiribell handles

Kiribell price plugins are an extension point for providing price information to Kiribell.

They are not intended to place orders, manage assets, or operate brokerage accounts. When asking an AI to implement a plugin, do not add unnecessary permissions or functionality.
