# 引き継ぎ情報 (Handover): StaxRip 日本語化プロジェクト

## 1. 目的とプロジェクト概要
本プロジェクトは、Windows 向け動画/音声エンコード GUI ツール「StaxRip」の非公式日本語化フォーク（リポジトリ: `engawa00/staxrip_jp`）です。
設定（Tools > Settings > General）から言語（日本語 / English）を切り替え可能な多言語辞書アーキテクチャを導入し、メイン画面、メニューバー、各種ダイアログ、エンコーダー詳細設定（x264, x265, NVEnc, QSVEnc, VCEEnc, SVT-AV1 各種バリアント）、および AviSynth+/VapourSynth フィルター設定まで網羅的に日本語ローカライズを行うことを目的としています。

---

## 2. 決定事項と実装アーキテクチャ

1. **言語切り替え・辞書方式**:
   - [Source/General/Localization.vb](Source/General/Localization.vb) モジュールにより、`Translate(text)` / `GetText(text)` による自動辞書引き翻訳を提供。
   - 内部組み込みの標準日本語辞書（UI、メニュー、x264/x265/NVEnc/QSVEnc/VCEEnc/SVT-AV1 のタブ・パラメータ・選択肢、AviSynth/VapourSynth フィルター名・マクロプロンプト等）を保持。
   - 外部辞書ファイル（`Settings\Languages\ja.json` または `ja.txt`）からの動的拡張・上書きに対応。
   - `CurrentLanguage` プロパティおよび `SetLanguage(lang)` により即時切り替え可能（"ja" / "en"）。
2. **設定保存**:
   - [Source/General/ApplicationSettings.vb](Source/General/ApplicationSettings.vb) の `ApplicationSettings` クラス（シングルトン `s`）に `Public Language As String = "ja"` を追加。
   - [Source/Forms/MainForm_ShowSettings.vb](Source/Forms/MainForm_ShowSettings.vb) に「表示言語 (要再起動)」ドロップダウンを配置。
3. **ローカライズ適用範囲**:
   - メインメニュー & 全コンテキストメニュー: [Source/UI/Menu.vb](Source/UI/Menu.vb)（MenuItemEx 内部で自動翻訳）
   - メイン画面主要 UI: [Source/Forms/MainForm.vb](Source/Forms/MainForm.vb) の `ApplyLocalization()`
   - エンコーダー詳細設定画面: [Source/Forms/CommandLineForm.vb](Source/Forms/CommandLineForm.vb)（タブ名、オプション名、ヘルプ、ドロップダウン選択肢）
   - エンコーダー別コントロール:
     - [Source/Forms/CommandLineVideoEncoderForm.vb](Source/Forms/CommandLineVideoEncoderForm.vb)
     - [Source/Controls/x264Control.vb](Source/Controls/x264Control.vb)
     - [Source/Controls/x265Control.vb](Source/Controls/x265Control.vb)
     - [Source/Controls/NVEncControl.vb](Source/Controls/NVEncControl.vb)
     - [Source/Controls/SvtAv1EncAppControl.vb](Source/Controls/SvtAv1EncAppControl.vb)
     - [Source/Controls/SvtAv1EncAppEssentialControl.vb](Source/Controls/SvtAv1EncAppEssentialControl.vb)
     - [Source/Controls/SvtAv1EncAppHdrControl.vb](Source/Controls/SvtAv1EncAppHdrControl.vb)
     - [Source/Controls/SvtAv1EncAppPsyexControl.vb](Source/Controls/SvtAv1EncAppPsyexControl.vb)
     - [Source/Controls/SvtAv1EncAppTritiumControl.vb](Source/Controls/SvtAv1EncAppTritiumControl.vb)
   - フィルター管理・対話ダイアログ:
     - フィルターリストビュー: [Source/Controls/FiltersListView.vb](Source/Controls/FiltersListView.vb)（ヘッダー、カテゴリ、メニュー、ツールチップ説明）
     - ダイアログマクロ: [Source/General/Macro.vb](Source/General/Macro.vb)（`$enter_text:` の入力プロンプト、`$select:` のタイトル・選択肢ラベルを翻訳）
4. **ビルド環境の自律性**:
   - VS Community 2026 環境でも MSBuild 単体でビルドできるよう、`Microsoft.NETFramework.ReferenceAssemblies.net48` を [Source/packages.config](Source/packages.config) および [Source/StaxRip.vbproj](Source/StaxRip.vbproj) に正式統合済み。
   - ビルド実行コマンド: `MSBuild.exe "Source\StaxRip.vbproj" /p:Configuration=Release /p:Platform=x64`（または Developer Command Prompt から実行）
5. **免責事項（MIT License 準拠）の明記**:
   - [README.md](README.md) 先頭に、MIT License に基づく「現状有姿（AS IS）」での提供、無保証条項、非公式フォークである旨およびオリジナル側への問い合わせ禁止、開発途上の自己責任利用に関する免責事項を英語と日本語で記載済み。

---

## 3. 制約事項および技術的注意事項（Gotchas）

1. **テスト制約**:
   - **完全ヘッドレス**: 起動テスト・検証時は GUI ウィンドウを一切表示させない（PowerShell / .NET リフレクション経由で検証）。
   - **成果物動画のクリーンアップ**: テスト用に生成した動画ファイル（ソース動画およびエンコード出力）はテスト終了時に**必ず全て削除（クリーンアップ）する**こと。
2. **VB.NET 構文の制約**:
   - `_` 単体は VB.NET の「行継続文字」であるため、メソッド名に `_` を定義・呼び出しすると構文エラー（`BC30203`）になる。必ず `Translate` / `GetText` を使用すること。
3. **フォント管理（FontManager）と初回起動の注意点**:
   - StaxRip は `Fonts` フォルダ内の TTF フォントを参照するが、ビルド直後は存在しない場合があるため、`FontManager.vb` でディレクトリ存在チェックおよびシステムフォント（`Yu Gothic UI` 等）への自動フォールバックを実装済み。
   - `Folder.Settings` プロパティは未設定時に設定フォルダ選択モーダルダイアログ（GUI）を表示するため、ヘッドレス環境や事前辞書ロード時には `g.SettingsFolderExists` をチェックすること。
4. **PowerShell 互換性**:
   - `run_headless_tests.ps1` は PowerShell 7 (`pwsh`) および Windows PowerShell 5.1 の双方に対応。UTF-8 with BOM で保存し、NativeCommandError を防止するため外部 CLI コマンド実行前に `$ErrorActionPreference = "Continue"` を設定している。

---

## 4. 現在のステータス
- **コード変更 & バイナリ**: 実装完了・コンパイル確認済み（Release / x64、0警告 0エラー）。
  - 実行可能バイナリ: `Source/bin/StaxRip.exe`（依存 DLL 等も同ディレクトリに出力）
- **テスト**: ヘッドレス自動テストスイート [run_headless_tests.ps1](run_headless_tests.ps1) により、
  1. ローカライズ辞書引きおよび日英切り替え動作検証（UI、メニュー、x264/x265/NVEnc/QSVEnc/VCEEnc/SVT-AV1 のタブ・パラメータ・選択肢、フィルターUI、マクロプロンプト等 全55項目）: 正常パス
  2. ヘッドレスソース動画生成: 正常パス
  3. 4種類のエンコードオプションパターン（CRF 23 fast / CRF 28 veryfast resize / 2pass 500k / Tune film Profile high）のエンコード実行: 全て正常パス
  4. テスト用動画ファイルの完全クリーンアップ: 完了確認済み
- **外部辞書ファイル**: `Source/Settings/Languages/ja.json` を拡充・更新済み。

---

## 5. 次に行うべきこと（今後の拡張・発展作業）
1. **実機 GUI での操作・表示確認**:
   - ユーザーの実機環境で `Source/bin/StaxRip.exe` を起動し、各エンコーダー（NVEnc, QSVEnc, VCEEnc, SVT-AV1）の設定ダイアログやフィルター設定画面の表示バランス（文字の収まり、レイアウト）を確認する。
2. **特殊な外部プラグイン・個別スクリプトの追加翻訳**:
   - ユーザー独自の VapourSynth/AviSynth 外部スクリプトや新規追加プラグインで未翻訳のパラメータがあれば、`Settings/Languages/ja.json` への追記または `Localization.vb` への登録で随時拡充する。
