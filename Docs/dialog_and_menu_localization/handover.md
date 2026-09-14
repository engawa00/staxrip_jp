# 引き継ぎ情報 (Handover): サブメニュー・ダイアログ・エラーメッセージの日本語化

## 1. 目的とプロジェクト概要
本プロジェクトは、Windows 向け動画/音声エンコード GUI ツール「StaxRip」の非公式日本語化フォーク（リポジトリ: `engawa00/staxrip_jp`）です。
設定（Tools > Settings > General）から言語（日本語 / English）を切り替え可能な多言語辞書アーキテクチャを導入し、メイン画面、エンコーダー設定、フィルター設定に加え、**サブメニュー、各種対話ダイアログ（TaskDialog/フォーム）、およびエラー・警告・情報・確認メッセージ（MsgError, MsgWarn, MsgInfo, MsgQuestion, ShowException）まで網羅的な日本語ローカライズ**を行っています。

---

## 2. 決定事項と実装アーキテクチャ

1. **メッセージ・ダイアログ基盤**:
   - [Source/General/General.vb](../../Source/General/General.vb):
     - `MsgError(title, content, handle, timeout)` および `Msg(title, content, icon, buttons)` において、`title` と `content` に `Localization.Translate` を適用。
     - `MsgWarn`, `MsgOK`, `MsgQuestion` も内部で `Msg` を経由するため自動的に日本語化。
   - [Source/UI/TaskDialog.vb](../../Source/UI/TaskDialog.vb):
     - `Init()` 内で `Title`, `Content`, `ExpandedContent` を `Localization.Translate` 経由に統一。
     - コマンドボタン（`cb.Title`, `cb.Description`）、通常ボタン（`b.Text`）をローカライズ（戻り値 `Tag` / `Value` は維持）。
     - 標準ボタン（OK, Yes, No, Cancel, Retry, Close）の表示テキストを「OK」「はい」「いいえ」「キャンセル」「再試行」「閉じる」にローカライズ。
     - コピーボタン（"メッセージをコピー"）およびコピー完了通知（"メッセージをクリップボードにコピーしました。"）を日本語化。
2. **階層サブメニューの多重生成防止**:
   - [Source/UI/Menu.vb](../../Source/UI/Menu.vb):
     - `MenuItemEx.Add`（482行目付近）で、親ノード探索判定を `If i.Text = a(x) OrElse i.Text = Localization.Translate(a(x)) Then` に修正。
     - 親メニュー名がすでに日本語化されていても正しく同一メニューとして検出され、階層が崩れたりサブメニューが重複生成される問題を防止。
3. **各種フォーム画面の自動コントロール翻訳**:
   - [Source/General/Localization.vb](../../Source/General/Localization.vb):
     - `ApplyLocalization(control As Control)` を新設。
     - フォームのタイトルバー（`Form.Text`）、子コントロール群（Button, Label, CheckBox, RadioButton, GroupBox, TabPage, ListView 列ヘッダー等）を再帰的に辞書引き翻訳。
     - `TextBox`, `RichTextBox`, `NumericUpDown`, 編集可能な `ComboBox` 等のデータ入力コントロールは除外。
     - ToolStrip / MenuStrip / ContextMenuStrip の各項目も再帰的に翻訳。
   - [Source/UI/Misc.vb](../../Source/UI/Misc.vb):
     - `FormBase.OnLoad` 内で `Localization.ApplyLocalization(Me)` を実行。
     - これにより、`AppsForm`（外部ツール管理）、`JobsForm`（ジョブ一覧）、`AudioForm`（音声設定）、`SourceFilesForm`、`CropForm`、`HelpForm`、`LogForm` 等の各種画面が個別のコード修正なしに一括で日本語化。
4. **辞書データ同期**:
   - [Source/General/Localization.vb](../../Source/General/Localization.vb) の内蔵辞書、および [Source/Settings/Languages/ja.json](../../Source/Settings/Languages/ja.json) に、エラー、警告、情報、確認メッセージ、サブメニュー項目など約90件を追加し、完全同期。
5. **バイナリ配置**:
   - `Source/bin/StaxRip.exe` に最新バイナリをビルド配備済み。
   - `Source/bin/Settings/Languages/ja.json` も最新の辞書ファイルを自動配置済み。

---

## 3. 制約事項および技術的注意事項（Gotchas）

1. **テスト制約**:
   - **完全ヘッドレス**: 起動テスト・検証時は GUI ウィンドウを一切表示させない（PowerShell / .NET リフレクション経由で検証）。
   - **成果物動画のクリーンアップ**: テスト用に生成した動画ファイル（ソース動画およびエンコード出力）はテスト終了時に**必ず全て削除（クリーンアップ）する**こと。
2. **VB.NET 構文の制約**:
   - `_` 単体は VB.NET の「行継続文字」であるため、メソッド名に `_` を定義・呼び出しすると構文エラー（`BC30203`）になる。必ず `Translate` / `GetText` を使用すること。
3. **ボタンの戻り値の保持**:
   - ダイアログボタンのテキスト（`.Text`）はローカライズするが、`Tag` や戻り値（`DialogResult.OK`, `DialogResult.Yes` 等）は絶対に変更しないこと。
4. **ビルドコマンド**:
   - Visual Studio 2026 環境の MSBuild パス: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
   - ビルド実行: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "Source\StaxRip.vbproj" /p:Configuration=Release /p:Platform=x64`
5. **PowerShell 互換性**:
   - `run_headless_tests.ps1` は PowerShell 7 (`pwsh`) および Windows PowerShell 5.1 の双方に対応。UTF-8 with BOM で保存し、NativeCommandError を防止するため外部 CLI コマンド実行前に `$ErrorActionPreference = "Continue"` を設定している。

---

## 4. 現在のステータス
- **コード変更 & バイナリ**: 全実装・改修完了、コンパイル確認済み（Release / x64、0警告 0エラー）。
  - 実行可能バイナリ: `Source/bin/StaxRip.exe`
- **テスト**: ヘッドレス自動テストスイート [run_headless_tests.ps1](../../run_headless_tests.ps1) により、
  1. ローカライズ辞書引きテスト（全88項目）: 正常パス
  2. `ApplyLocalization` による Form / Button / Label の自動翻訳: 正常パス
  3. 英語モード切り替えとフォールバック: 正常パス
  4. ヘッドレスソース動画生成: 正常パス
  5. 4種類のエンコードオプションパターン（CRF 23 fast / CRF 28 veryfast resize / 2pass 500k / Tune film Profile high）のエンコード実行: 全て正常パス
  6. テスト用動画ファイルの完全クリーンアップ: 完了確認済み
- **外部辞書ファイル**: `Source/Settings/Languages/ja.json` および `Source/bin/Settings/Languages/ja.json` を同期済み。

---

## 5. 次に行うべきこと（実機確認・運用）
1. **実機 GUI での動作確認**:
   - `Source/bin/StaxRip.exe` を直接ダブルクリックして起動。
   - `Apps > Manage...`（外部ツールの管理）画面、`Tools > Jobs...`（ジョブ一覧）画面、`Audio` 設定画面を開き、サブメニューやボタン、リストヘッダーの日本語表示を確認する。
   - 存在しないファイルを開く等の操作を行い、エラーメッセージや確認ダイアログが日本語で表示されることを確認する。
