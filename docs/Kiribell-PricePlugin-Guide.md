# Twelve Data サンプルから価格プラグインを作る

`Kiribell.PricePlugin.TwelveData.Sample` は、外部APIから価格を取得してKiribellへ通知する実働サンプルです。

このサンプルをコピーして価格取得部分を書き換えることで、独自の価格プラグインを作成できます。

## サンプルの `SMP.` について

Twelve Dataサンプルでは、監視銘柄を次のように登録して動作を確認します。

```text
SMP.NVDA
```

この `SMP.` は、Kiribellの価格プラグインで必要な銘柄コードではありません。

**サンプルプラグインから取得した価格であることを確認しやすくするために、このサンプルだけで使用している接頭辞です。**

Kiribell本体は通常 `US.NVDA` などのコードで価格を取得します。そのまま同じ銘柄をサンプルプラグインから取得すると、本体が取得した価格とプラグインが取得した価格が同じになり、どちらから価格が反映されたのか分かりにくくなります。

そこでサンプルでは、

```text
Kiribellの監視コード    SMP.NVDA
                          ↓
サンプルプラグイン      NVDA に変換
                          ↓
Twelve Data             NVDA の価格を取得
                          ↓
サンプルプラグイン      SMP.NVDA として通知
                          ↓
Kiribell                SMP.NVDA に価格を表示
```

という動作にしています。

これにより、`SMP.NVDA` に価格が表示されれば、Twelve Dataサンプルプラグインから価格が取得できていることを確認できます。

### 実際のプラグインでは `SMP.` は不要です

独自プラグインとして実際に運用するときは、`SMP.` を使用する必要はありません。

たとえば米国株をTwelve Dataから取得するプラグインとして運用するなら、

```text
US.NVDA
```

を受け取り、

```text
US.NVDA → NVDA
```

と変換してTwelve Dataへ問い合わせます。

取得した価格は、元のコードである、

```text
US.NVDA
```

に返します。

つまり最終的には、

```text
US.NVDA
   ↓
価格プラグイン
   ↓
NVDA
   ↓
外部API
   ↓
Quote
   ↓
US.NVDA
```

という形で運用するのが基本です。

`SMP.` はあくまで、**サンプルプラグインが正常に動作していることを確認するための仕組み**と考えてください。

---

## 新しいプラグインを作る

新しい価格プラグインを作る場合は、

```text
Kiribell.PricePlugin.TwelveData.Sample
```

をコピーして使用できます。

基本的に変更するのは次の部分です。

1. プラグイン名
2. Kiribellの銘柄コードからAPI用コードへの変換
3. APIへの問い合わせ
4. APIの応答から `Quote` を作る処理

`StartAsync`、`SetWatches`、`StopAsync`、定期取得、Kiribellへの価格通知などは、そのまま利用できます。

## 1. プラグイン名を変更する

サンプルの、

```csharp
public string Name =>
    "Twelve Data 価格プラグイン サンプル";
```

を変更します。

```csharp
public string Name =>
    "My Price Plugin";
```

## 2. 銘柄コードの変換を変更する

サンプルでは、動作確認のため `SMP.` を取り除いてTwelve Dataへ問い合わせています。

```csharp
private static string? ToTwelveDataSymbol(string code)
{
    const string prefix = "SMP.";

    return code.StartsWith(
        prefix,
        StringComparison.OrdinalIgnoreCase)
            && code.Length > prefix.Length
        ? code[prefix.Length..]
        : null;
}
```

そのため、

```text
SMP.NVDA → NVDA
```

となります。

### 実運用用に変更する

たとえばKiribellの米国株コード、

```text
US.NVDA
```

をTwelve Dataの、

```text
NVDA
```

として取得するなら、この部分を次のように変更できます。

```csharp
private static string? ToTwelveDataSymbol(string code)
{
    const string prefix = "US.";

    return code.StartsWith(
        prefix,
        StringComparison.OrdinalIgnoreCase)
            && code.Length > prefix.Length
        ? code[prefix.Length..]
        : null;
}
```

これだけで、

```text
US.NVDA → NVDA
US.AAPL → AAPL
US.MSFT → MSFT
```

のように取得できるようになります。

**サンプルを実際のプラグインとして使用するときに、まず変更するのがこの部分です。**

別の価格APIを使用する場合は、そのAPIが要求する銘柄コードへ変換してください。

たとえば、

```text
JP.7203 → 7203.T
```

のような変換でも構いません。

重要なのは、APIへ問い合わせるコードとKiribellへ返すコードは別に考えられるという点です。

## 3. APIへの問い合わせを変更する

実際に価格を取得しているのは `FetchQuoteAsync` です。

Twelve Dataサンプルでは、

```csharp
var url =
    $"https://api.twelvedata.com/quote" +
    $"?symbol={Uri.EscapeDataString(symbol)}" +
    $"&apikey={Uri.EscapeDataString(apiKey)}";

using var response =
    await Http.GetAsync(url, token);
```

としてTwelve Dataへ問い合わせています。

Twelve Dataをそのまま利用する場合、この部分は変更する必要はありません。

別の価格サービスを利用する場合は、この部分をそのサービスのAPI呼び出しへ置き換えます。

## 4. APIの値を `Quote` へ変換する

Twelve Dataから取得したJSONは、`FetchQuoteAsync` の中でKiribellの `Quote` へ変換しています。

```csharp
return (
    new Quote(
        lastPrice.Value,
        TryGetDecimal(root, "open"),
        TryGetDecimal(root, "high"),
        TryGetDecimal(root, "low"),
        TryGetDecimal(root, "previous_close")),
    string.Empty);
```

`Quote` の各値は、

```text
LastPrice
OpenPrice
TodayHigh
TodayLow
PreviousClose
```

の順です。

別のAPIを利用する場合は、そのAPIが返すJSONの項目名に合わせて、この部分を変更します。

## 5. Kiribellへ価格を返す部分は変更しない

取得した価格は `FetchAllAsync` で次のように格納されています。

```csharp
if (result.Quote is { } quote)
    quotes[watch.Code] = quote;
```

ここでは、APIへ問い合わせた `symbol` ではなく、**Kiribellから受け取った元の `watch.Code` を使用します。**

たとえば、

```text
US.NVDA
   ↓
NVDAとしてAPIへ問い合わせ
   ↓
価格取得
```

した場合でも、Kiribellへ返すのは、

```csharp
quotes["US.NVDA"] = quote;
```

です。

この部分は変更しないでください。

---

## Twelve Dataをそのまま使うなら、実はほとんど完成しています

Twelve Dataを実際の価格取得先として使用する場合、サンプルにはすでに、

- APIへの問い合わせ
- JSONの解析
- `Quote` の作成
- 1分ごとの定期取得
- 監視銘柄変更への対応
- エラー通知
- APIキー設定画面
- 設定の保存
- 開始・停止処理

が実装されています。

そのため、米国株をTwelve Dataから取得するだけなら、主な変更は、

```csharp
const string prefix = "SMP.";
```

を、

```csharp
const string prefix = "US.";
```

へ変更することです。

これで、

```text
サンプル

SMP.NVDA → NVDA → Twelve Data
```

から、

```text
実運用

US.NVDA → NVDA → Twelve Data
US.AAPL → AAPL → Twelve Data
US.MSFT → MSFT → Twelve Data
```

というプラグインへ変更できます。

別の価格サービスを使用したい場合は、さらに `FetchQuoteAsync` のAPI呼び出しと `Quote` への変換部分を、そのサービスに合わせて変更してください。

## まとめ

`Kiribell.PricePlugin.TwelveData.Sample` は、単に動作を見るだけのサンプルではなく、独自価格プラグインを作るためのベースとして利用できます。

Twelve Dataをそのまま使用するなら、

**`SMP.` を実際に使用する市場コードへ変更する**

ところから始められます。

別の価格サービスを利用する場合でも、

**銘柄コード変換 → API呼び出し → `Quote` への変換**

の3か所を中心に変更すれば、Kiribellとの基本的な連携処理はそのまま利用できます。
