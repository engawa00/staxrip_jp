# 修正内容の確認 (Walkthrough): サブメニュー・ダイアログ・エラーメッセージの網羅的日本語化

## 1. 実施概要
ユーザーからの「サブメニューやダイアログやエラーメッセージも可能な限り日本語化して」という要求に基づき、StaxRip 全体のダイアログ（`TaskDialog` および各種フォーム画面）、エラー・警告・情報・確認メッセージ（`MsgError`, `MsgWarn`, `MsgInfo`, `MsgQuestion`）、およびサブメニューのローカライズ基盤を強化し、約90項目の頻出メッセージ・UI項目を辞書に拡充しました。

---

## 2. 変更内容一覧

### 2.1 メッセージ共通関数の多言語化 ([Source/General/General.vb](../../Source/General/General.vb))
- `MsgError(title, content, handle, timeout)`: `title` および `content` を `Localization.Translate` 経由で表示。
- `Msg(title, content, icon, buttons)`: `title` および `content` を `Localization.Translate` 経由で表示。
- `MsgWarn`, `MsgOK`, `MsgQuestion` は内部で上記 `Msg` を経由するため、呼び出し元の英語メッセージが自動的に日本語化。

### 2.2 TaskDialog の包括的ローカライズ ([Source/UI/TaskDialog.vb](../../Source/UI/TaskDialog.vb))
- `Init()` 内で `Title`, `Content`, `ExpandedContent` を `Localization.Translate` 経由に統一。
- コマンドボタン（`CommandDefinitions`）の表示テキスト（`cb.Title`）および説明文（`cb.Description`）を自動翻訳。
- ボタン定義（`ButtonDefinitions`）の画面表示用テキスト（`b.Text`）を自動翻訳（`Tag` や戻り値は保持）。
- `Buttons` プロパティの標準ボタン（OK, Yes, No, Cancel, Retry, Close）の表示テキストをローカライズ（「OK」「はい」「いいえ」「キャンセル」「再試行」「閉じる」）。
- `ShowCopyButton` のコピーボタンテキスト（"メッセージをコピー"）およびコピー完了通知（"メッセージをクリップボードにコピーしました。"）を日本語化。

### 2.3 階層サブメニューのマッチング修正 ([Source/UI/Menu.vb](../../Source/UI/Menu.vb))
- `MenuItemEx.Add`（482行目付近）：
  - 既存の親メニュー探索で `If i.Text = a(x) OrElse i.Text = Localization.Translate(a(x)) Then` に修正。
  - 親メニューが既に日本語化されている場合でも正しく同一ノードとして検出され、サブメニューが別メニューとして多重生成されるバグを防止。

### 2.4 各種フォームの自動コントロール翻訳 ([Source/General/Localization.vb](../../Source/General/Localization.vb) & [Source/UI/Misc.vb](../../Source/UI/Misc.vb))
- `Localization.vb` に `ApplyLocalization(control As Control)` を新設：
  - フォームタイトル（`Form.Text`）、子コントロール群（Button, Label, CheckBox, RadioButton, GroupBox, TabPage, ListView 列ヘッダー等）を再帰的に走査し、辞書にある文字列を自動翻訳。
  - `TextBox`, `RichTextBox`, `NumericUpDown`, ユーザー入力可能な `ComboBox` などのデータ入力コントロールは安全に除外。
  - メニューバー・ツールバー・コンテキストメニュー（`ToolStrip`, `MenuStrip`, `ContextMenuStrip`）の項目も再帰的に翻訳。
- `Misc.vb` の `FormBase.OnLoad` に `Localization.ApplyLocalization(Me)` を追加：
  - これにより、`AppsForm`（外部ツール管理）、`JobsForm`（ジョブ一覧）、`AudioForm`（音声設定）、`SourceFilesForm`、`CropForm`、`HelpForm`、`LogForm` などの全画面で UI が自動的に日本語化。

### 2.5 辞書データの網羅的拡充 ([Source/General/Localization.vb](../../Source/General/Localization.vb) & [Source/Settings/Languages/ja.json](../../Source/Settings/Languages/ja.json))
- 以下の約90エントリを内蔵辞書および外部 JSON 辞書へ網羅的に登録：
  - **エラーメッセージ**: ソース/プロジェクトファイル未検出、一時フォルダ作成失敗、スクリプトエラー、ポータブル動作通知、字幕形式制限、3Dデマックス未対応、PATH関連など。
  - **警告メッセージ**: Windows Terminal未検出、互換性の問題、ソースファイル不足、アシスタント警告など。
  - **情報メッセージ**: 「すべて正常です！」「アップデートはありません」「再起動が必要です」「プロファイルを保存しました」など。
  - **確認メッセージ**: 「設定をリセットしますか？」「初期設定に戻しますか？」「すべての音声トラックを処理しますか？」など。
  - **サブメニュー・ダイアログUI**: 外部ツール管理メニュー項目（パス編集、検索、全確認、自動更新等）、ジョブ一覧ヘッダー・操作ボタン、音声設定項目など。

---

## 3. テスト・検証結果

### 3.1 ビルド検証
- Visual Studio 2026 MSBuild（Release / x64）によるビルドを実行：
  - **結果**: 成功（警告 0件、エラー 0件）
  - 出力バイナリ: `Source/bin/StaxRip.exe`
  - 外部辞書: `Source/bin/Settings/Languages/ja.json`

### 3.2 ヘッドレス自動テストスイート ([run_headless_tests.ps1](../../run_headless_tests.ps1))
PowerShell 7 (`pwsh`) にて自動テストスイートを実行：
- **テスト 1 (ローカライズエンジン)**: 全88項目の辞書引きテスト、`ApplyLocalization` による Form / Button / Label の自動翻訳、英語フォールバック切り替え → **全項目パス**
- **テスト 2 (ソース動画生成)**: 1秒テストパターン動画のヘッドレス生成 → **成功**
- **テスト 3 (動画エンコード回帰テスト)**:
  - パターン 1 (x264 CRF 23 fast): **成功**
  - パターン 2 (x264 CRF 28 veryfast 640x360リサイズ): **成功**
  - パターン 3 (x264 2パス 500kbps): **成功**
  - パターン 4 (x264 Tune film Profile high): **成功**
- **クリーンアップ**: テスト用動画ファイル（ソース動画およびエンコード出力）の完全削除確認 → **完了**

---

## 4. ユーザー向け確認手順 (実機確認)
1. `Source/bin/StaxRip.exe` を起動します。
2. **メニューバーおよびサブメニュー**:
   - `Apps` > `Manage...` を開き、外部ツール管理画面のメニュー（パス編集、全確認、自動更新など）やリスト列ヘッダーが日本語化されていることを確認。
   - `Tools` > `Jobs...` を開き、ジョブ画面のボタンや状態列が日本語化されていることを確認。
   - `Audio` 設定画面を開き、各種ラベルやボタンが日本語化されていることを確認。
3. **ダイアログ・エラーメッセージ**:
   - 存在しないファイルを開こうとした際や設定リセット時、確認ダイアログのタイトル・本文・ボタン（「はい」「いいえ」「キャンセル」）が日本語で表示されることを確認。
