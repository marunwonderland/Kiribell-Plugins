# キリベル価格プラグイン

キリベルは、設定で選択したDLLから価格プラグインを読み込みます。価格の取得時刻と間隔はプラグイン側で決め、取得結果を本体へ通知します。プラグイン用の公開APIは [`../src/StockBeacon.PluginContracts/IPriceUpdatePublisher.cs`](../src/StockBeacon.PluginContracts/IPriceUpdatePublisher.cs) にあります。

## 作成と配置

初めて動作を確認する場合は、[`Kiribell.PricePlugin.JsonFile.Sample`](../samples/Kiribell.PricePlugin.JsonFile.Sample/) を使います。これはDLLと同じフォルダーの `prices.json` を2秒ごとに読み、価格を通知する実働サンプルです。ファイルの価格を書き換えると、プラグインの通知と本体の反映を確認できます。

外部APIを呼ぶ実装例は、[`Kiribell.PricePlugin.TwelveData.Sample`](../samples/Kiribell.PricePlugin.TwelveData.Sample/) です。監視リストの `SMP.NVDA` を Twelve Data の `NVDA` として取得し、同じ `SMP.NVDA` に価格を返します。APIキーの設定方法と、定期取得・エラー通知を含んでいます。

[`Kiribell.PricePlugin.Sample`](../samples/Kiribell.PricePlugin.Sample/) は、取得処理を持たない最小の雛形です。新しいプラグインを作るときの出発点として使います。

```powershell
dotnet build samples\Kiribell.PricePlugin.JsonFile.Sample\Kiribell.PricePlugin.JsonFile.Sample.csproj -c Release
```

出力先の `bin\Release\net10.0\` にある `Kiribell.PricePlugin.JsonFile.Sample.dll` を選択します。キリベルの **設定 → 拡張機能** でDLLを選び、**外部DLLを有効にする** をオンにして保存します。監視リストに `JP.7203` または `US.AAPL` を追加し、同じフォルダーの `prices.json` にある `LastPrice` を編集して保存してください。プラグインは2秒ごとにファイルを読み、本体は5秒ごとに最新の通知を反映します。

サンプルと同様に、プラグインは `StockBeacon.PluginContracts` を参照します。

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\StockBeacon.PluginContracts\StockBeacon.PluginContracts.csproj" />
  </ItemGroup>
</Project>
```

DLLには、`StockBeacon.PluginContracts` を参照する `IPriceUpdatePublisher` 実装を一つだけ入れてください。本体は抽象ではない実装を数え、0件なら「`IPriceUpdatePublisher 実装が見つかりません`」、2件以上なら「`IPriceUpdatePublisher 実装はDLL内に一つだけ配置してください`」として読み込みを中止します。

選ばれた型を生成できない場合、プラグインの読み込みは失敗します。たとえば引数なしコンストラクターがない場合は `MissingMethodException` が発生します。設定画面には「プラグインの読み込みに失敗しました: <例外メッセージ>」と表示され、ログには `Plugin Load Error` と例外の詳細が記録されます。

## プラグイン用API

```csharp
namespace Kiribell.Plugins;

public sealed record Watch(string Code, string Name);

public sealed record Quote(
    decimal LastPrice,
    decimal? OpenPrice = null,
    decimal? TodayHigh = null,
    decimal? TodayLow = null,
    decimal? PreviousClose = null);

public sealed record PriceUpdateBatch(
    IReadOnlyDictionary<string, Quote> Quotes,
    string? Error,
    DateTimeOffset ObtainedAt);

public interface IPriceUpdatePublisher
{
    string Name { get; }
    event EventHandler<PriceUpdateBatch>? PricesUpdated;
    Task StartAsync(IReadOnlyCollection<Watch> watches, CancellationToken token = default);
    void SetWatches(IReadOnlyCollection<Watch> watches);
    Task StopAsync();
}

public interface IConfigurablePricePlugin
{
    string? ConfigurationJson { get; set; }
}

public interface ISettingsPricePlugin
{
    bool ShowSettings(nint ownerWindowHandle);
}
```

`IPriceUpdatePublisher` は必須です。`IConfigurablePricePlugin` と `ISettingsPricePlugin` は必要な場合だけ追加します。

## 本体とのやり取り

アプリの読み込み時と、設定を保存してプラグインを有効にしたときに、本体は `StartAsync` を呼びます。引数はテストデータを除く全監視銘柄です。銘柄の追加、編集、削除の後には `SetWatches` を呼びます。アプリ終了時、および設定保存時には `StopAsync` を呼びます。これらの呼び出しに、本体は `CancellationToken` を指定しません。

プラグインは任意の時刻・間隔で価格を取得し、`PricesUpdated` を発火します。本体には5秒間隔の確認タイマーがあり、その時点で保持している最新の `PriceUpdateBatch` を取り出して反映します。このタイマーはプラグインに価格取得を指示しません。

本体は受け取った `PriceUpdateBatch` を一件だけ保持します。新しいバッチの `ObtainedAt` が保持中のものより新しい場合だけ置き換え、反映時に取り出して消去します。同じ時刻または過去の時刻のバッチは捨てられます。

## 価格の通知

`Watch.Code` はキリベルに保存されている監視銘柄コードです。サービスへの問い合わせ用にコードを変換しても構いませんが、`PriceUpdateBatch.Quotes` のキーには入力された `Watch.Code` を使います。本体は、各監視銘柄について `Quotes.TryGetValue(row.Code, …)` で価格を探します。見つからない銘柄は更新しません。

| 値 | 本体の動作 |
| --- | --- |
| `LastPrice` | 返された銘柄の現在値として反映します。 |
| `OpenPrice` | 値があり、当日分の始値が未反映（始値の市場日付が当日ではない、または始値が0以下）で、かつ平日の市場開始時刻以降の場合だけ反映します。市場開始時刻は `JP` が日本時間9:00、`US` が米国東部時間9:30、`HK` が中国時間9:30です。これ以外のコードではプラグインから始値を反映しません。 |
| `TodayHigh` / `TodayLow` | 値がある場合だけ反映します。 |
| `PreviousClose` | 値がある場合だけ前日終値として反映します。 |

価格が反映された銘柄はプラグイン価格を所有する状態になり、通常の価格更新では現在値・始値・高値・安値を更新しなくなります。通常の価格更新は前日終値の更新だけを続けます。

本体は、候補の現在値が対象銘柄の直前の現在値から50%以上離れ、かつ別の監視銘柄の現在値と一致する場合、その銘柄への反映を保留します。OCRなどで行が入れ替わったときの誤反映を防ぐための判定です。

`Error` は `null` 以外ならプラグインエラーとして画面に表示します。空文字列も `null` ではないため、エラーとして扱われます。`Error` があっても、`Quotes` に入っている価格は反映します。

## プラグイン設定

`IConfigurablePricePlugin` を実装すると、DLLの読み込み直後に保存済みの `ConfigurationJson` が設定されます。値はキリベルの設定として、DLLの絶対パスごとに保存されます。別のDLLへ切り替えても、そのDLLの設定値は変更しません。同じDLLへ戻すと、そのDLL用に保存した値が渡されます。

設定画面を出す場合は、`ISettingsPricePlugin` を実装します。本体は設定画面のボタン操作から `ShowSettings` を呼びます。

```csharp
public bool ShowSettings(nint ownerWindowHandle)
```

呼び出し結果が `true` で、かつ `IConfigurablePricePlugin` も実装している場合だけ、`ConfigurationJson` を読み出して保存します。`false` の場合は設定値を保存しません。`ISettingsPricePlugin` を実装していない場合は「選択したDLLは設定画面に対応していません」と表示します。

## エラーとログ

キリベルの **設定 → 拡張機能 → エラーログ** で **エラーをログファイルへ保存する** をオンにすると、DLLの読み込み・開始・停止・設定画面の例外と、プラグインが `PriceUpdateBatch.Error` で通知したエラーを次のファイルへ記録します。

```text
%LOCALAPPDATA%\Kiribell\Data\error.log
```

Debug ビルドでは既定でオン、Release ビルドでは既定でオフです。プラグイン内部の自律更新中に起きた失敗を画面とログへ伝えるには、`PriceUpdateBatch.Error` に設定して通知します。

## DLLに関する補足

本体はプラグインごとに読み込み領域を分けます。設定保存時に現在のプラグインを停止した後であれば、同じアセンブリ名の別のプラグインDLLへ切り替えられます。

## 付属の実装

- [`Kiribell.PricePlugin.JsonFile.Sample`](../samples/Kiribell.PricePlugin.JsonFile.Sample/) は、ファイルから価格を取得する実働サンプルです。
- [`Kiribell.PricePlugin.TwelveData.Sample`](../samples/Kiribell.PricePlugin.TwelveData.Sample/) は、外部APIから価格を取得する実働サンプルです。`SMP.NVDA` のように `SMP.` を付けたコードを対象とし、APIキーはプラグイン設定画面から入力します。
- [`Kiribell.PricePlugin.Sample`](../samples/Kiribell.PricePlugin.Sample/) は、取得処理を持たない最小の雛形です。
