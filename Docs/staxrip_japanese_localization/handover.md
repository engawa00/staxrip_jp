# 引き継ぎ情報 (Handover): StaxRip 日本語化プロジェクト

## 1. 目的とプロジェクト概要
本プロジェクトは、Windows 向け動画/音声エンコード GUI ツール「StaxRip」の非公式日本語化フォーク（リポジトリ: `engawa00/staxrip_jp`）です。
設定（Tools > Settings > General）から言語（日本語 / English）を切り替え可能な多言語辞書アーキテクチャを導入し、メイン画面、メニューバー、各種ダイアログ、および各エンコーダー詳細設定（x264, x265, NVEnc 等）まで網羅的に日本語ローカライズを行うことを目的としています。

---

## 2. 決定事項と実装アーキテクチャ

1. **言語切り替え・辞書方式**:
   - [Source/General/Localization.vb](Source/General/Localization.vb) モジュールにより、`Translate(text)` / `GetText(text)` による自動辞書引き翻訳を提供。
   - 内部組み込みの標準日本語辞書（数百ワードの UI、メニュー、エンコーダー用語、ヘルプ）を保持し、外部辞書ファイル（テキスト/JSON）からの動的拡張にも対応。
   - `CurrentLanguage` プロパティおよび `SetLanguage(lang)` により即時切り替え可能（"ja" / "en"）。
2. **設定保存**:
   - [Source/General/ApplicationSettings.vb](Source/General/ApplicationSettings.vb) の `ApplicationSettings` クラス（シングルトン `s`）に `Public Language As String = "ja"` を追加。
   - [Source/Forms/MainForm_ShowSettings.vb](Source/Forms/MainForm_ShowSettings.vb) に「表示言語 (要再起動)」ドロップダウンを追加。
3. **ローカライズ適用範囲**:
   - メインメニュー & 全コンテキストメニュー: [Source/UI/Menu.vb](Source/UI/Menu.vb)
   - メイン画面主要 UI: [Source/Forms/MainForm.vb](Source/Forms/MainForm.vb) の `ApplyLocalization()`
   - エンコーダー詳細設定画面: [Source/Forms/CommandLineForm.vb](Source/Forms/CommandLineForm.vb)（タブ名、オプション名、ヘルプ、ドロップダウン選択肢）
   - エンコーダー別コントロール: [Source/Forms/CommandLineVideoEncoderForm.vb](Source/Forms/CommandLineVideoEncoderForm.vb)、[Source/Controls/x264Control.vb](Source/Controls/x264Control.vb)、[Source/Controls/x265Control.vb](Source/Controls/x265Control.vb)
4. **ビルド環境の自律性**:
   - VS Community 2026 環境でも MSBuild 単体でビルドできるよう、`Microsoft.NETFramework.ReferenceAssemblies.net48` を [Source/packages.config](Source/packages.config) および [Source/StaxRip.vbproj](Source/StaxRip.vbproj) に正式統合済み。
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

---

## 4. 現在のステータス
- **コード変更 & バイナリ**: 実装完了・コンパイル確認済み（Release / x64、0警告 0エラー）。
  - 実行可能バイナリ: `Source/bin/StaxRip.exe`（依存 DLL 等も同ディレクトリに出力）
- **テスト**: ヘッドレス自動テストスイート [run_headless_tests.ps1](run_headless_tests.ps1) により、
  1. ローカライズ辞書引きおよび日英切り替え動作検証: 正常パス
  2. ヘッドレスソース動画生成: 正常パス
  3. 4種類のエンコードオプションパターン（CRF 23 fast / CRF 28 veryfast resize / 2pass 500k / Tune film Profile high）のエンコード実行: 全て正常パス
  4. テスト用動画ファイルの完全クリーンアップ: 完了確認済み

---

## 5. 次に行うべきこと（今後の拡張・発展作業）
1. **追加エンコーダー・個別フィルターの辞書拡充**:
   - 必要に応じて NVEnc, QSVEnc, VCEEnc, SVT-AV1 や AviSynth+/VapourSynth 各種フィルター固有の詳細パラメータ・ヘルプテキストの日本語訳エントリーを追加・拡充する。
2. **外部辞書ファイル（JSON）の同梱**:
   - `Settings/Languages/ja.json` のような外部辞書テンプレートファイルをリポジトリに配置し、ユーザーがアプリ外から独自に翻訳を追加・上書きできるようにする。
3. **リモートへのプッシュおよびプルリクエスト作成**:
   - ローカルコミット完了後、必要に応じてリモートへのプッシュやPRの作成を行う。
