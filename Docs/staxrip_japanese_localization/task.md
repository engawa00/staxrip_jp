# タスクリスト: StaxRip 日本語化プロジェクト

## 概要
StaxRip の多言語対応（言語切り替え機能）および網羅的な日本語ローカライズの実現。

## タスク一覧

- [x] **フェーズ 1: ビルド環境の整備**
  - [x] VS Community 2026 環境での MSBuild 検出およびビルド失敗要因の特定
  - [x] NuGet 参照アセンブリ (`Microsoft.NETFramework.ReferenceAssemblies.net48`) によるビルド確認
  - [x] `StaxRip.vbproj` および `packages.config` への正式組み込み（いつでも誰でもビルド可能にする）
  - [x] MSBuild 単体でのビルド検証完了

- [x] **フェーズ 2: ローカライズ基盤（辞書・切り替えエンジン）の実装**
  - [x] `Source/General/Localization.vb` モジュールの新規作成
    - 辞書テーブル読み込み機能（組み込みデフォルト辞書 + 外部辞書ファイル読み込み対応）
    - 翻訳関数 `Translate(text)` / `GetText(text)` の提供
    - 動的言語切り替え対応（`SetLanguage` / `CurrentLanguage` プロパティ）
  - [x] `ApplicationSettings.vb` (`s`) への `Language` 設定項目追加（"en", "ja"）
  - [x] 設定画面（Tools > Settings / `MainForm_ShowSettings.vb`）への言語選択 UI 追加
  - [x] 日本語環境に適したフォールバック調整

- [x] **フェーズ 3: メイン画面およびメニューシステムの日本語化**
  - [x] `Menu.vb` の表示テキスト翻訳連携（メニューバーおよびコンテキストメニュー）
  - [x] メインメニュー項目（ファイル、クロップ、プレビュー、プロジェクト、ツール、アプリ、ヘルプ等）の翻訳辞書作成
  - [x] `MainForm.vb` の主要コントロール（ソース、ターゲット、サイズ、音声、アシスタント、フィルター等）の翻訳
  - [x] 各種コンテキストメニューの翻訳

- [x] **フェーズ 4: エンコーダー・フィルター・設定画面の網羅的ローカライズ**
  - [x] `CommandLineForm.vb` におけるタブ名（`param.Path`）、オプション名（`param.Text` / `param.Label`）、ヘルプ説明（`param.Help`）、選択肢項目（`oParam.Options`）の翻訳適用
  - [x] エンコーダー画面（`CommandLineVideoEncoderForm.vb`, `x264Control.vb`, `x265Control.vb`）の各項目・ボタン・リストビューの日本語化
  - [x] 主要エンコーダー（x264, x265, NVEnc, QSVEnc, VCEEnc 等）の基本オプション・ヘルプの日本語辞書作成
  - [x] 設定画面（`MainForm_ShowSettings.vb`）の主要項目翻訳

- [x] **フェーズ 5: 主要ダイアログおよびメッセージのローカライズ**
  - [x] ダイアログ・コントロールの共通翻訳辞書登録
  - [x] README.md に非公式・開発中・オリジナル無関係・動作無保証・使用非推奨の免責事項を英語と日本語で追記

- [x] **フェーズ 6: ビルド・動作検証および最終確認**
  - [x] MSBuild による完全ビルド検証（0警告 0エラー）
  - [x] ヘッドレス自動テストスクリプト（`run_headless_tests.ps1`）作成・実行
  - [x] ローカライズコアエンジンおよび日英切り替え動作のヘッドレス検証（All Passed）
  - [x] 複数オプション（CRF 23 fast, CRF 28 veryfast resize 640x360, 2pass 500k, Tune film / Profile high）での動画エンコード検証（All Passed）
  - [x] テスト生成動画ファイルのクリーンアップ（動画全削除）の検証確認
  - [x] ドキュメント更新（`task.md`, `implementation_plan.md`, `walkthrough.md`, `handover.md`）
