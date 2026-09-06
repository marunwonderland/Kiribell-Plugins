# Kiribell.PricePlugin.JsonFile.Sample

キリベルの価格プラグインが正しく更新されるかを確認するための、実働サンプルです。DLLと同じフォルダーにある `prices.json` を2秒ごとに読み、内容を `PricesUpdated` で通知します。

```powershell
dotnet build samples\Kiribell.PricePlugin.JsonFile.Sample\Kiribell.PricePlugin.JsonFile.Sample.csproj -c Release
```

ビルド出力フォルダーの `Kiribell.PricePlugin.JsonFile.Sample.dll` をキリベルで選択します。同じフォルダーの `prices.json` をテキストエディターで編集して保存します。プラグインは2秒ごとにファイルを読み、本体は5秒ごとに最新の通知を画面へ反映します。

`prices.json` は、銘柄コードをキーにしたJSONオブジェクトです。キーがキリベルの監視銘柄コードと一致した項目だけを反映します。

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

このサンプルには、開始・停止、監視銘柄変更、非同期ファイルI/O、部分更新、エラー通知の実装が含まれます。

詳しいAPI契約は [`../../docs/PLUGINS.md`](../../docs/PLUGINS.md) を参照してください。
