# Kiribell Plugins

Kiribell の価格プラグインを作成するための公開APIとサンプル実装です。

## 必要環境

- .NET 10 SDK
- Kiribell のプラグイン機能に対応したバージョン
- Twelve Data サンプルの設定画面をビルドする場合は Windows

## 構成

- `src/StockBeacon.PluginContracts/` — Kiribell 本体とプラグインの公開契約
- `samples/Kiribell.PricePlugin.Sample/` — 最小のプラグイン雛形
- `samples/Kiribell.PricePlugin.JsonFile.Sample/` — `prices.json` を2秒ごとに読む実働サンプル
- `samples/Kiribell.PricePlugin.TwelveData.Sample/` — Twelve Data APIを使う実働サンプル

> `StockBeacon.PluginContracts` というアセンブリ名は Kiribell 本体との互換性のため維持しています。

## 最小実装

プラグインDLLには、`Kiribell.Plugins.IPriceUpdatePublisher` を実装した **抽象でない型を1つだけ** 含めてください。Kiribell はその型を引数なしで生成します。

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

必要に応じて `IConfigurablePricePlugin` と `ISettingsPricePlugin` も実装できます。

## ビルド

```powershell
dotnet build samples\Kiribell.PricePlugin.JsonFile.Sample\Kiribell.PricePlugin.JsonFile.Sample.csproj -c Release
```

生成されたプラグインDLLを Kiribell の **設定 → 拡張機能** で選び、**外部DLLを有効にする** をオンにして保存します。

## 価格通知の基本ルール

- `PriceUpdateBatch.Quotes` のキーには、Kiribellから渡された `Watch.Code` を使用してください。
- サービス側の銘柄コードへ変換して問い合わせても、Kiribellへ返すときは元の `Watch.Code` に戻します。
- `ObtainedAt` には取得時点を入れてください。Kiribellは新しいバッチだけを採用します。
- `Error` が `null` 以外の場合はプラグインエラーとして扱われます。
- Kiribell は監視銘柄変更時に `SetWatches`、開始時に `StartAsync`、停止時に `StopAsync` を呼びます。

## JsonFile.Sample

DLLと同じフォルダーの `prices.json` を読みます。外部サービスを使わず、Kiribellとの連携を確認するのに向いています。

## TwelveData.Sample

監視コード `SMP.NVDA` を Twelve Data の `NVDA` として問い合わせ、結果を `SMP.NVDA` としてKiribellへ返します。APIキーはサンプルの設定画面で入力します。APIキーはこのリポジトリには含まれていません。

Twelve Data の利用条件・料金・レート制限は Twelve Data 側の規約を確認してください。このサンプルは外部サービスの利用を保証・仲介するものではありません。

## セキュリティ

プラグインはKiribellと同じユーザー権限で実行されるコードです。信頼できないDLLを読み込まないでください。また、公開するプラグインへAPIキーやアクセストークンをハードコードしないでください。

## License

ライセンスは現在未設定です。ライセンスファイルが追加されるまでは、公開されていること自体が再利用許諾を意味するものではありません。
