# Kiribell.PricePlugin.TwelveData.Sample

Twelve Dataの `/quote` エンドポイントから価格を取得する実働サンプルです。`SMP.` 接頭辞の監視銘柄だけを対象にします。

| キリベルのコード | Twelve Dataへ渡すシンボル |
| --- | --- |
| `SMP.NVDA` | `NVDA` |
| `SMP.AAPL` | `AAPL` |

プラグインは1分ごとに価格を取得し、`close`、`open`、`high`、`low`、`previous_close` をキリベルへ通知します。キリベルの **設定 → 拡張機能** でDLLを選んだ後、**プラグインを設定** から Twelve Data APIキーを入力して保存してください。キーは `ConfigurationJson` を通じてキリベルのプラグイン設定として保存されます。これは秘密情報専用の保管庫ではないため、APIキーの取り扱いは利用するサービスの要件に合わせて判断してください。

```powershell
dotnet build samples\Kiribell.PricePlugin.TwelveData.Sample\Kiribell.PricePlugin.TwelveData.Sample.csproj -c Release
```

出力された `Kiribell.PricePlugin.TwelveData.Sample.dll` を **設定 → 拡張機能** から選択し、**外部DLLを有効にする** をオンにして保存します。外部ライブラリを追加して独自プラグインへ発展させた場合は、必要な依存DLLや `.deps.json` などのビルド出力もプラグインDLLと同じフォルダーに配置してください。

Twelve DataのAPI利用条件、料金、レート制限、提供範囲、データの利用・再配布条件はプラン等により異なります。[公式APIドキュメント](https://twelvedata.com/docs/advanced/api-usage) を確認してください。

独自プラグインへ発展させる場合は [`../../docs/Kiribell-PricePlugin-Guide.md`](../../docs/Kiribell-PricePlugin-Guide.md) も参照してください。
