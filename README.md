# Kiribell Plugins

Kiribell の価格取得を拡張するための公開プラグインAPIとサンプル集です。

独自の価格取得処理を Kiribell に組み込みたい場合は、このリポジトリを出発点にできます。

## まず読むもの

- [価格プラグイン仕様](docs/PLUGINS.md) — 公開API、本体とのやり取り、エラー、設定UI、読み込み条件など
- [Twelve Data サンプルから独自プラグインを作る](docs/Kiribell-PricePlugin-Guide.md) — 実働サンプルをベースに独自プラグインを作る手順
- [AIコーディングエージェントでプラグインを作る](docs/AI-PLUGIN-GUIDE.md) — Codexなどへ渡せる指示テンプレートと安全な作成手順

## サンプル

| サンプル | 用途 |
| --- | --- |
| [Kiribell.PricePlugin.Sample](samples/Kiribell.PricePlugin.Sample/) | 最小構成。ゼロから実装したい場合の雛形 |
| [Kiribell.PricePlugin.JsonFile.Sample](samples/Kiribell.PricePlugin.JsonFile.Sample/) | `prices.json` を読む実働サンプル。Kiribellとの連携確認向け |
| [Kiribell.PricePlugin.TwelveData.Sample](samples/Kiribell.PricePlugin.TwelveData.Sample/) | Twelve Data APIを利用する実働サンプル。外部API連携の参考実装 |

## 最短で動かす

外部サービスなしで確認するなら JsonFile サンプルが簡単です。

```powershell
dotnet build samples\Kiribell.PricePlugin.JsonFile.Sample\Kiribell.PricePlugin.JsonFile.Sample.csproj -c Release
```

生成された `Kiribell.PricePlugin.JsonFile.Sample.dll` を Kiribell の **設定 → 拡張機能** で選択し、**外部DLLを有効にする** をオンにします。

同じ出力フォルダーの `prices.json` を編集すると、プラグインが価格をKiribellへ通知します。

詳しい手順は [JsonFile サンプルのREADME](samples/Kiribell.PricePlugin.JsonFile.Sample/README.md) を参照してください。

## 独自プラグインを作る

プラグインは `Kiribell.Plugins.IPriceUpdatePublisher` を実装します。

```csharp
public sealed class MyPricePlugin : IPriceUpdatePublisher
{
    public string Name => "My Price Plugin";
    public event EventHandler<PriceUpdateBatch>? PricesUpdated;

    public Task StartAsync(IReadOnlyCollection<Watch> watches, CancellationToken token = default)
        => Task.CompletedTask;

    public void SetWatches(IReadOnlyCollection<Watch> watches) { }

    public Task StopAsync() => Task.CompletedTask;
}
```

重要な点は次のとおりです。

- プラグインDLL内の具体的な `IPriceUpdatePublisher` 実装は1つだけにする
- Kiribell が引数なしで生成できる型にする
- `PriceUpdateBatch.Quotes` のキーには、Kiribellから受け取った元の `Watch.Code` を使う
- 外部サービス用に銘柄コードを変換しても、Kiribellへ返すときは元のコードへ戻す
- APIキーやアクセストークンをDLLへハードコードしない
- 外部ライブラリを使う場合は、プラグインDLLだけでなく必要な依存DLLや `.deps.json` などのビルド出力も一緒に配置する

詳細は [価格プラグイン仕様](docs/PLUGINS.md) にまとめています。

CodexなどのAIコーディングエージェントに作成を依頼する場合は、[AIコーディングエージェントでプラグインを作る](docs/AI-PLUGIN-GUIDE.md) に、そのまま渡せる指示テンプレートを用意しています。

## 公開API

公開契約は [`src/StockBeacon.PluginContracts/`](src/StockBeacon.PluginContracts/) にあります。

`StockBeacon.PluginContracts` というアセンブリ名は Kiribell 本体との互換性のため維持しています。

主な型は次のとおりです。

- `IPriceUpdatePublisher`
- `IConfigurablePricePlugin`
- `ISettingsPricePlugin`
- `Watch`
- `Quote`
- `PriceUpdateBatch`

## 必要環境

- .NET 10 SDK
- Kiribell のプラグイン機能に対応したバージョン
- Twelve Data サンプルの設定画面をビルドする場合は Windows

## 外部サービスについて

Twelve Data サンプルは、外部APIを利用する価格プラグインの参考実装です。APIキーはリポジトリには含まれていません。

Twelve Dataを含む外部サービスを利用する場合は、各サービスの利用規約、料金、レート制限、データの再配布条件などを利用者自身で確認してください。

## セキュリティ

Kiribell のプラグインは、Kiribell と同じユーザー権限で実行されるコードです。信頼できないDLLを読み込まないでください。

`IConfigurablePricePlugin.ConfigurationJson` はプラグイン設定の受け渡し・保存用であり、秘密情報専用の保管庫ではありません。APIキーなどを扱うプラグインでは、利用するサービスの要件とリスクを確認してください。

## License

MIT Licenseです。詳しくは [LICENSE](LICENSE) を参照してください。
