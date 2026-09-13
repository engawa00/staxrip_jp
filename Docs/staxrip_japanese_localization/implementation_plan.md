# 実装計画: StaxRip 日本語版および言語切り替え機能の実装

StaxRip は元来、多言語化（ローカライズ機構）を持たず、Visual Basic コード中に英語の文言が直接ハードコードされています。
本計画では、StaxRip に言語切り替え機能（日本語 / English）を導入し、メイン画面・メニューバー・設定画面から各エンコーダー詳細設定（x264/x265/NVEnc等）まで網羅的に日本語化する基盤と翻訳辞書を構築します。

---

## ユーザー確認事項 (User Review Required)

> [!IMPORTANT]
> **1. 言語切り替えと翻訳辞書の外部化**
> 翻訳データはプログラム内に組み込まれる既定辞書に加え、`Settings/Languages/ja.json` のような外部 JSON ファイルとしても配置します。これにより、後から文言を微調整・追加したい場合にも再コンパイル不要で即座に反映できます。
>
> **2. ビルド構成の調整**
> 現在の環境（Visual Studio Community 2026）には .NET Framework 4.8 の SDK/Targeting Pack がプリインストールされていませんでしたが、公式の NuGet 参照アセンブリパッケージ (`Microsoft.NETFramework.ReferenceAssemblies.net48`) を `StaxRip.vbproj` に組み込むことで、追加のインストーラーなしで MSBuild によるクリーンビルドが成功することを確認しました。

---

## 変更内容とアーキテクチャ

### 1. ローカライズ基盤モジュール (`Source/General/Localization.vb`) [NEW]
- **辞書エンジン**: `Dictionary(Of String, String)` によるキー（英語オリジナル文言）から翻訳文言への高速マッピング。
- **翻訳関数**: 
  - `Localization._(text As String) As String`（翻訳があれば日本語、なければ英語原文を返す安全なフォールバック機構）
  - `Localization._(format As String, ParamArray args() As Object) As String`（書式付き文字列の安全な補間）
- **外部ファイル読み込み**: アプリケーション起動時または言語切り替え時に JSON 辞書ファイルを読み込む。
- **イベント通知**: 言語切り替え時に UI 全体を更新するイベント（`LanguageChanged`）を提供。

### 2. 設定管理への言語項目の追加 (`Source/General/ApplicationSettings.vb`, `MainForm_ShowSettings.vb`) [MODIFY]
- `ApplicationSettings.vb` に `Public Language As String = "ja"` を追加。
- 設定画面（Tools > Settings）の General タブに「言語 / Language」ドロップダウンを追加。

### 3. メインメニューおよびUIコントロールの自動ローカライズ [MODIFY]
- **メニュー**: `Source/UI/Menu.vb` の `BuildMenu` メソッドにおいて、表示用テキスト `tsi.Text` に `Localization._(cmi.Text)` を適用。内部コマンド識別キーは原文のまま保持し、機能の互換性を完全に維持。
- **メイン画面**: `Source/Forms/MainForm.vb` 内の各グループボックス、ボタン、ラベル、コンテキストメニューのテキストに `Localization._(...)` を適用。
- **アシスタントヒント**: `MainForm_Assistant.vb` 内のアシスタント表示文言のローカライズ。

### 4. エンコーダー詳細設定の自動ローカライズ (`Source/Forms/CommandLineForm.vb`, `VideoEncoderCommandLine.vb`) [MODIFY]
- `CommandLineForm.vb` の `InitUI` メソッドにおいて、ページパス（タブ名）、ラベル、ヘルプ、オプション選択肢に対して `Localization._(...)` を適用。
- これにより、x264, x265, NVEnc, QSVEnc, VCEEnc, SVT-AV1, AOMEnc などの大量のエンコーダー設定画面が一元的に日本語化可能に。

### 5. 主要ダイアログの日本語化 [MODIFY]
- `JobsForm.vb`（ジョブ一覧・バッチ処理）
- `CropForm.vb`（クロップ調整）
- `PreviewForm.vb`（プレビュー再生・シーク）
- `AudioForm.vb`（音声ストリーム・エンコーダー設定）
- `MuxerForm.vb`（コンテナ・多重化設定）
- `AppsForm.vb`（外部ツール管理）

### 6. ビルド環境の永続化 (`Source/StaxRip.vbproj`, `Source/packages.config`) [MODIFY]
- `Microsoft.NETFramework.ReferenceAssemblies.net48` をプロジェクトファイルに正式に組み込み、MSBuild 単体で即座にビルドできるようにする。

---

## 検証計画 (Verification Plan)

### 自動テスト / ビルド検証
- MSBuild を実行し、コンパイルエラー・警告が 0 であることを確認。
  ```powershell
  MSBuild.exe "Source\StaxRip.vbproj" -p:Configuration=Release -p:Platform=x64 -m
  ```

### 手動検証
1. **画面表示確認**:
   - `StaxRip.exe` を起動し、メイン画面が自然な日本語で表示されることを確認。
   - メインメニュー（ファイル、クロップ、プレビュー、プロジェクト、ツール、アプリ、ヘルプ）が日本語化されていることを確認。
2. **言語切り替え確認**:
   - 設定画面（Tools > Settings）から言語を「English」に変更して再起動後、英語に戻ることを確認。
   - 再度「日本語」に変更して日本語に戻ることを確認。
3. **エンコーダー設定確認**:
   - エンコーダーオプション（x264 / x265 / NVEnc 等）を開き、タブ名や各パラメータの説明が日本語で表示されることを確認。
4. **動作整合性確認**:
   - コマンドライン生成やエンコード処理に日本語化による悪影響（予期せぬ文字列置換など）が生じないことを確認。
