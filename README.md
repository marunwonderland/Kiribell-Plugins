# Kiribell Plugins

**Use your own price source with Kiribell.**

Kiribell's standard Japanese stock price data is delayed by approximately 15 minutes.

Price plugins let you connect Kiribell to an API available from your broker or market data service, helping you supplement this limitation when a suitable API is available.

This repository contains sample plugins, the public plugin API, and guides for building a price plugin for the service you use.

**日本語版: [README.ja.md](README.ja.md)**

> [!IMPORTANT]
> When using a broker or data service API, you must follow the provider's terms of use and API terms.
> API availability, pricing, rate limits, data latency, and data usage conditions vary by provider.

## Samples

The easiest way to get started is to try one of the included samples.

### JsonFile

A simple sample that reads prices from `prices.json`.

It is useful for understanding the minimum structure of a Kiribell price plugin without connecting to an external service.

### Twelve Data

A sample price plugin that retrieves price data using the Twelve Data API.

It shows how a Kiribell plugin can connect to an external price data service and is a useful starting point when building your own plugin.

## Build a plugin for the service you use

If your broker or data service provides an API that you can use, try building a price plugin for it.

Kiribell provides a public plugin API, samples, and guides to help you get started.

### Documentation

- [Plugin specification](docs/PLUGINS.md)  
  Public API, communication with Kiribell, configuration UI, and other plugin requirements.

- [Price plugin development guide](docs/Kiribell-PricePlugin-Guide.md)  
  A step-by-step guide based on the Twelve Data sample.

- [Guide for AI coding agents](docs/AI-PLUGIN-GUIDE.md)  
  Instructions and context you can give to an AI coding agent when asking it to build a Kiribell plugin.

You can also start from the minimum plugin template included in this repository.

## Build with an AI coding agent

You don't necessarily have to write the plugin from scratch yourself.

The [Guide for AI coding agents](docs/AI-PLUGIN-GUIDE.md) contains information that can be given to an AI coding agent together with the plugin specification and sample code.

If the API you want to use has documentation, you can provide that documentation as well and use the existing samples as a reference implementation.

## Public plugin API

Kiribell exposes a public API for price plugins.

A price plugin can provide price updates to Kiribell through the published interfaces and can provide its own configuration UI when necessary.

For the API contract, requirements, and integration details, see:

**[Plugin specification](docs/PLUGINS.md)**

## Using external APIs

The data available to Kiribell depends on the API and plan provided by the broker or data service you connect.

A price plugin does not by itself guarantee real-time data.

Before using an external API, check the provider's:

- Terms of use
- API usage terms
- Pricing
- Rate limits
- Data latency
- Data usage and redistribution conditions

You are responsible for using each external service in accordance with its terms.

## Security

Kiribell plugins run as code on your computer.

Only install plugins from sources you trust. When using third-party plugins, review their source and behavior when possible.

For technical requirements and details about the plugin interface, see the [plugin specification](docs/PLUGINS.md).

## License

See the license information included in this repository.