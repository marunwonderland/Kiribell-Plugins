# AIコーディングエージェントでKiribellプラグインを作る

Kiribellは価格取得部分を外部プラグインとして差し替えられます。

このページは、CodexなどのAIコーディングエージェントに、利用したい証券会社・市場データAPI向けの価格取得プラグインを作らせるためのガイドです。

> [!IMPORTANT]
> AIが生成したコードは、そのまま信用して実行せず、内容・利用規約・認証情報の扱いを必ず確認してください。

## 先に確認すること

対象サービスが、利用者自身によるプログラムからの価格取得を正式に許可しているか確認してください。

特に次を確認します。

- 公式API、SDK、RSSなどの提供有無
- 個人利用・商用利用の条件
- リアルタイム価格取得の利用条件
- レート制限
- データの保存・再配布条件
- APIキー、アクセストークン、口座情報などの取り扱い条件

非公式スクレイピングや、利用規約に反する取得方法は使用しないでください。

## Codexなどへ渡す指示テンプレート

次の内容をコピーし、`【使用したいデータ提供元】` を書き換えてAIコーディングエージェントへ渡してください。

```text
Kiribell用の価格取得プラグインを作成してください。

Kiribellの公開Plugin SDKはこちらです。
https://github.com/marunwonderland/Kiribell-Plugins

まずREADME、docs/PLUGINS.md、docs/Kiribell-PricePlugin-Guide.md、samplesを確認し、現在の公開仕様に従って実装してください。

【使用したいデータ提供元】
ここに証券会社・API・サービス名を記入してください。

【目的】
Kiribellの監視銘柄について価格情報を取得し、IPriceUpdatePublisherを実装したプラグインとしてKiribellへ通知してください。

【実装条件】
- StockBeacon.PluginContractsをKiribell Plugin SDKの契約アセンブリとして使用する
- Kiribell.Plugins.IPriceUpdatePublisherを実装する
- プラグインDLL内のIPriceUpdatePublisher実装クラスは1つだけにする
- 引数なしで生成できること
- StartAsyncで初期監視銘柄を受け取る
- SetWatchesで監視銘柄変更を反映する
- StopAsyncで通信・タイマー・イベント購読等を確実に終了する
- PriceUpdateBatchのQuotesキーには必ずKiribellから渡された元のWatch.Codeを使用する
- API用の銘柄コードへ変換した場合も、返却時には元のWatch.Codeへ戻す
- 一銘柄の取得失敗でプラグイン全体を停止させない
- CancellationTokenを適切に扱う
- APIのレート制限を守る
- APIキー・パスワード・トークン等をソースコードへハードコードしない
- 必要ならIConfigurablePricePlugin / ISettingsPricePluginを利用して設定画面を実装する
- API認証情報を保存する場合は保存場所と安全上の注意をREADMEに明記する
- 外部NuGetパッケージを使用する場合は、Kiribellで読み込む際に必要な依存DLLや.deps.json等も含めて配置できるようにする
- 注文機能は実装しない。Kiribellで必要なのは価格取得のみとする

【調査】
実装前に、対象サービスの最新の公式API仕様・利用条件・レート制限を確認してください。
非公式スクレイピングや利用規約に抵触する方法は使用しないでください。
正式に価格情報をプログラム取得できない場合は、無理に実装せず、その理由を報告してください。

【成果物】
1. プラグインプロジェクト
2. 実装ソースコード
3. ビルド方法
4. Kiribellへの導入方法
5. 必要な設定方法
6. 対応する銘柄コード変換ルール
7. API利用上の制約・注意事項を記載したREADME
8. 可能な範囲の自動テスト

既存のKiribell本体や公開Plugin SDKの仕様は、必要がない限り変更しないでください。
```

## AIへ認証情報を渡さない

APIキー、アクセストークン、証券口座のログイン情報、パスワードなどをプロンプトへ貼り付けないでください。

実装中は、たとえば次のようなダミー値を使います。

```text
YOUR_API_KEY
```

実際の認証情報は、完成後に利用者自身の環境で設定してください。

## 生成後に最低限確認すること

AIが実装を完了したら、少なくとも次を確認してください。

- `IPriceUpdatePublisher` の具体実装がDLL内に1つだけか
- Kiribellが引数なしでプラグインを生成できるか
- `StartAsync` / `SetWatches` / `StopAsync` が正しく実装されているか
- 停止時に通信、タイマー、バックグラウンドタスクが残らないか
- `PriceUpdateBatch.Quotes` のキーが元の `Watch.Code` になっているか
- APIキー等がソースコード、ログ、テストデータへ残っていないか
- APIの呼び出し頻度がレート制限を超えていないか
- 一銘柄の失敗で取得ループ全体が停止しないか
- エラー発生時にもKiribell本体を巻き込んで終了しないか
- 必要な依存DLLや `.deps.json` が出力先に揃っているか
- 対象サービスの利用規約上、その使い方が許可されているか

## 最初の動作確認

いきなり多くの銘柄を登録せず、まず1銘柄で確認することをおすすめします。

1. プラグインをReleaseビルドする
2. Kiribellの **設定 → 拡張機能** でプラグインDLLを選択する
3. **外部DLLを有効にする** をオンにする
4. 必要なAPI設定を行う
5. 対象となる監視銘柄を1件だけ登録する
6. 価格更新を確認する
7. Kiribell終了後に通信やプロセスが残っていないことを確認する
8. 問題がなければ監視銘柄を増やす

## 参考にするサンプル

目的に応じて既存サンプルを参考にしてください。

- [`Kiribell.PricePlugin.Sample`](../samples/Kiribell.PricePlugin.Sample/) — 最小構成
- [`Kiribell.PricePlugin.JsonFile.Sample`](../samples/Kiribell.PricePlugin.JsonFile.Sample/) — Kiribellとの連携確認
- [`Kiribell.PricePlugin.TwelveData.Sample`](../samples/Kiribell.PricePlugin.TwelveData.Sample/) — HTTP API、設定画面、定期取得の参考

実装仕様そのものは [価格プラグイン仕様](PLUGINS.md) を優先してください。

## 公開する場合

自作プラグインを第三者へ配布する場合は、さらに慎重な確認が必要です。

- 対象サービスが第三者向けツールでの利用を許可しているか
- APIやマーケットデータの再配布に制限がないか
- 認証情報を利用者ごとに設定する構造になっているか
- 利用者の口座情報や個人情報を収集しないか
- ライセンス表記が必要な依存ライブラリを使っていないか

KiribellのPlugin SDKがMIT Licenseであることと、接続先サービスの利用条件は別です。

## Kiribellが担当する範囲

Kiribellの価格プラグインは、価格情報をKiribellへ渡すための拡張ポイントです。

注文、資産管理、口座操作などを行うことは前提としていません。AIへ実装を依頼するときも、必要以上の権限や機能を追加させないことをおすすめします。
