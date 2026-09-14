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

- [x] **フェーズ 7: ハードウェアエンコーダー（NVEnc, QSVEnc, VCEEnc）および SVT-AV1 詳細設定・UI の日本語辞書拡充**
  - [x] 各種タブ名・カテゴリ（VPP, Colorspace, HDR2SDR, Ngx-TrueHDR, Deband, LibPlacebo, Tonemapping, Sharpness, GOP size/type, AV1 Specific 等）の辞書登録
  - [x] 各種エンコーダー詳細パラメータ名（約500項目）およびヘルプ/ヒントの日本語訳辞書登録
  - [x] ドロップダウン選択肢（モード、プリセット、チューン、プロファイル、レート制御、アルゴリズム等）の日本語訳辞書登録
  - [x] エンコーダーコントロール（`NVEncControl.vb`, `SvtAv1EncAppControl.vb` 等）のメイン画面表示項目およびボタンテキストのローカライズ適用

- [x] **フェーズ 8: AviSynth+ / VapourSynth フィルター関連 UI・プロンプトの日本語化と辞書拡充**
  - [x] フィルターメニュー（`FiltersListView.vb`）の各項目および説明テキストのローカライズ
  - [x] フィルターカテゴリ名（Source, Color, Field, Frame, Denoise, Sharpen, Resize, Misc, Subtitles 等）の辞書登録
  - [x] フィルター追加時の対話ダイアログ（`Macro.vb` の `ExpandGUI` における `$enter_text:` 入力プロンプトおよび `$select:` のダイアログタイトル・選択肢）への `Localization.Translate` 適用
  - [x] フィルター対話プロンプト（`Is the Input using TV Range?`, `Select Input Color Matrix` 等）の日本語訳辞書登録

- [x] **フェーズ 9: 外部辞書ファイル（`ja.json`）の拡充・同期**
  - [x] 内蔵デフォルト辞書と外部辞書（`Source/Settings/Languages/ja.json`）の同期・更新

- [x] **フェーズ 10: ビルド検証・完全ヘッドレステスト**
  - [x] MSBuild によるビルド検証（0警告 0エラー）
  - [x] ヘッドレス自動テスト（`run_headless_tests.ps1` を拡張・実行して辞書引き・エンコード・クリーンアップを検証、全パス）

- [x] **フェーズ 11: ドキュメント更新（walkthrough.md, handover.md 等）**

- [x] **フェーズ 12: 起動時エラー対応・防衛ガード実装および実行環境セットアップ（方法B）**
  - [x] `Package.vb` に `Apps\Conf` および `ConfPath` の存在チェックガードを追加（`DirectoryNotFoundException` 防止）
  - [x] `FrameServer.vb` および `AutoCrop\Main.vb` に `FrameServer.dll` の存在チェックガードを追加（Windows System32 同名 DLL の誤読込による `EntryPointNotFoundException` 防止）
  - [x] `Localization.vb` および `ja.json` に `FrameServer.dll` 不足時のエラーメッセージ日本語訳を追加
  - [x] 公式アーカイブ `StaxRip-v2.52.5-x64.7z` を取得・展開し、`Apps`、`Fonts`、`Icons`、`FrameServer.dll` を `Source\bin` に配置
  - [x] `.gitignore` に `*.7z`, `*.zip`, `scratch/` を追記
  - [x] MSBuild によるリビルド（0警告 0エラー）およびヘッドレス自動テスト（All Passed）の検証確認
  - [x] 正常動作確認後、ダウンロードした大容量一時アーカイブ（約704MB）を完全に削除
