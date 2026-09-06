# Kiribell.PricePlugin.Sample

キリベル価格プラグインの最小雛形です。`SamplePricePlugin` は `IPriceUpdatePublisher` を実装しています。

この雛形の `StartAsync` は監視銘柄とキャンセルトークンを受け取るところまでを実装しており、価格取得ループ自体はまだ持っていません。独自プラグインでは `StartAsync` から取得処理を開始し、取得できた価格を `Publish` で通知する実装を追加してください。本体は `ObtainedAt` が新しいバッチだけを一件保持し、次の確認時に反映します。監視銘柄が変わると `SetWatches` が呼ばれ、無効化・終了時には `StopAsync` が呼ばれます。

このままでは価格を返さないため、キリベルで実運用しないでください。

```powershell
dotnet build samples\Kiribell.PricePlugin.Sample\Kiribell.PricePlugin.Sample.csproj -c Release
```

出力された `bin\Release\net10.0\Kiribell.PricePlugin.Sample.dll` を、必要な依存ファイルとともに任意のフォルダーへ配置します。外部ライブラリを追加した場合は、ビルド出力に生成された依存DLLや `.deps.json` なども同じフォルダーに残してください。キリベルの「設定」→「拡張機能」でプラグインDLLを選択し、「外部DLLを有効にする」をオンにします。

詳しいAPI契約、エラー処理、設定UIの実装方法は [`../../docs/PLUGINS.md`](../../docs/PLUGINS.md) を参照してください。
