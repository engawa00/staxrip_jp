# ウォークスルー: エンコーダー（NVEnc/QSVEnc/VCEEnc/SVT-AV1）およびフィルター詳細設定の日本語化拡充

handover.md (Line 58) に記載されたタスク「NVEnc, QSVEnc, VCEEnc, SVT-AV1 や AviSynth+/VapourSynth 各種フィルター固有の詳細パラメータ・ヘルプテキストの日本語訳エントリーを追加・拡充する」を実施・完了しました。

---

## 変更内容の概要

### 1. エンコーダー固有設定・UI のローカライズ拡充
- **タブ名・カテゴリ階層の日本語訳**:
  - `Input/Output`（入出力）、`Slice Decision`（スライス判定）、`Motion Search`（動き探索）、`GOP size/type`（GOP構造・サイズ）、`AV1 Specific 1/2`（AV1固有設定 1/2）、`Color Description`（色情報記述）、`Variance Boost Options`（分散ブースト設定）
  - `VPP`（VPPフィルター）、`Colorspace`（色空間）、`HDR2SDR`（HDR→SDR変換）、`Ngx-TrueHDR`、`Deband`（バンディング低減）、`LibPlacebo`、`Tonemapping`（トーンマッピング）、`Sharpness`（鮮鋭化）、`Deinterlace 2`、`Denoise 2/3` など
- **エンコーダー詳細パラメータ名・選択肢の日本語訳**:
  - レート制御（CBR, VBR, CQP, QVBR, CRF, 目標ビットレート, 最大ビットレート, VBVバッファサイズ, 初期QP, 最小/最大QP等）
  - フレーム構造・予測（GOP長, 最小GOP長, Bフレーム数, 参照フレーム数, 適応量子化 AQ-Mode / AQ Strength / CAQ, 動き探索範囲等）
  - 色空間・VUI（原色色度, 伝達特性, マトリックス係数, カラーレンジ, Mastering Display, MaxCLL/MaxFALL, Dolby Vision RPU 等）
  - VPP フィルターパラメータ（KNN/PMD/Smooth/FFT3D/NLMeans ノイズ除去, Deband 閾値/範囲/ディザー, パディング, 回転, 反転など）
  - 選択肢（各モード、NVEnc/QSVEnc/FFmpeg ハードウェアデコード、P1〜P7 プリセット、低遅延/超低遅延、8/10/12ビット、Main 10、Main 444等）
- **エンコーダーコントロール画面のローカライズ**:
  - [Source/Controls/NVEncControl.vb](Source/Controls/NVEncControl.vb)
  - [Source/Controls/SvtAv1EncAppControl.vb](Source/Controls/SvtAv1EncAppControl.vb)
  - [Source/Controls/SvtAv1EncAppEssentialControl.vb](Source/Controls/SvtAv1EncAppEssentialControl.vb)
  - [Source/Controls/SvtAv1EncAppHdrControl.vb](Source/Controls/SvtAv1EncAppHdrControl.vb)
  - [Source/Controls/SvtAv1EncAppPsyexControl.vb](Source/Controls/SvtAv1EncAppPsyexControl.vb)
  - [Source/Controls/SvtAv1EncAppTritiumControl.vb](Source/Controls/SvtAv1EncAppTritiumControl.vb)
  - メイン画面のリスト表示項目（品質, モード, プリセット, チューン, 出力色深度, DV プロファイル, カラーレンジ, 速度, 高速デコード, 先行探索フレーム数, フィルムグレイン）およびボタン（設定, コンテナ設定, 圧縮率チェックを実行, 出力ファイル名を上書き）に `Localization.Translate` を適用。

### 2. AviSynth+ / VapourSynth フィルター関連のローカライズ拡充
- **フィルターリストビュー ([Source/Controls/FiltersListView.vb](Source/Controls/FiltersListView.vb))**:
  - カテゴリ列表示、ヘッダー（種別, フィルター名）、メニュー項目（アクティブ, 置換, 挿入, 追加, 削除, コードを編集..., コードをプレビュー..., 情報..., 再生, プロファイル..., 上へ移動, 下へ移動, フィルターセットアップ）およびツールチップ説明文を日本語化。
- **対話プロンプトマクロ ([Source/General/Macro.vb](Source/General/Macro.vb))**:
  - `$enter_text:...$` の入力プロンプトテキスト、および `$select:...$` のダイアログタイトル・選択肢ラベルに `Localization.Translate` を適用（スクリプトに代入される実コードはそのまま保持しつつ、UI 表示のみを日本語化）。
- **フィルタープロファイル・対話プロンプトの辞書登録**:
  - 「入力はTVレンジですか？」「出力をTVレンジにしますか？」「自動ゲインを有効にしますか？」「変換先の色深度を選択してください」「カラーマトリックスを選択してください」「HDR最大マスタリング輝度レベル」等の日本語訳を追加。

### 3. 外部辞書の同期
- [Source/Settings/Languages/ja.json](Source/Settings/Languages/ja.json) を更新し、今回追加した主要なタブ・パラメータ・選択肢・プロンプトを反映。

---

## 検証結果

### 1. ビルド検証
- **コンパイラ**: Visual Studio 2026 MSBuild (`MSBuild.exe` v18.9.1)
- **ビルドコマンド**: `MSBuild.exe Source\StaxRip.vbproj /p:Configuration=Release /p:Platform=x64`
- **結果**: **0 警告、0 エラー** でビルド成功（`Source\bin\StaxRip.exe` を正常出力）。

### 2. ヘッドレス自動テストスイート ([run_headless_tests.ps1](run_headless_tests.ps1))
PowerShell 7 (`pwsh`) および Windows PowerShell 5.1 の双方で完全ヘッドレス実行：
- **テスト 1 (ローカライズエンジン)**:
  - 基本メニュー・UI 項目（File, Quality, Preset, Tune, Assistant 等）: **全て一致**
  - エンコーダー タブ名（スライス判定, 動き探索, GOP構造・サイズ, 色情報記述, Ngx-TrueHDR, トーンマッピング等）: **全て一致**
  - エンコーダー パラメータ名（デコーダー, 目標ビットレート, VBVバッファサイズ, 制限付き品質, 適応量子化, Dolby Vision RPU, 先行探索フレーム数, フィルムグレイン等）: **全て一致**
  - エンコーダー 選択肢（QVBR固定品質モード, NVEncハードウェアデコード, QSVEncハードウェア, P1〜P7, 8ビット, 10ビット等）: **全て一致**
  - エンコーダー コントロール画面（出力ファイル名を上書き, コンテナ設定, 圧縮率チェックを実行, 最高品質等）: **全て一致**
  - フィルター UI・メニュー（AviSynth/VapourSynthフィルター, 置換, 挿入, コードを編集, 選択したフィルターを削除します等）: **全て一致**
  - フィルター 対話プロンプト（以下の選択肢から1つ選択してください, 自動ゲインを有効にしますか, TVレンジですか, 入力カラーマトリックス等）: **全て一致**
  - 英語モード切り替え: 原文英語がそのまま返ることを確認
- **テスト 2 (ヘッドレスソース動画生成)**:
  - FFmpeg lavfi testsrc による 1 秒テスト動画生成: **正常作成**
- **テスト 3 (動画エンコード検証 - 4パターン)**:
  - CRF 23, Preset fast: **成功**
  - CRF 28, Preset veryfast, リサイズ 640x360: **成功**
  - 2パス 固定ビットレート 500kbps: **成功**
  - Tune film, Profile high, CRF 20: **成功**
- **クリーンアップ**:
  - テスト生成された動画ファイルは全自動で完全削除（リポジトリ内に一切残留なし）。

---

### 3. 起動時エラー対応と防衛ガード実装・実行環境セットアップ
ユーザーから報告された起動時エラー2件への対応を実施：

1. **エラーの原因調査と特定**:
   - `DirectoryNotFoundException`（`Apps\Conf`）: Git 管理外の大容量外部バイナリフォルダ（`Apps`）が存在しないため。
   - `EntryPointNotFoundException`（`CreateVapourSynthServer`）: 実行ディレクトリに `FrameServer.dll` が無いため、Windows が誤ってシステム標準の `C:\Windows\System32\FrameServer.dll`（カメラ用）をロードしたため。
2. **防衛ガードの実装**:
   - [Source/General/Package.vb](Source/General/Package.vb): `LoadConfAll()` で `Apps\Conf` のディレクトリ存在チェックを追加し、`GetConf()` で `ConfPath` のファイル存在チェックを追加。未配置環境でも例外クラッシュしないよう防護。
   - [Source/Video/FrameServer.vb](Source/Video/FrameServer.vb) & [Source/Tools/AutoCrop/Main.vb](Source/Tools/AutoCrop/Main.vb): ネイティブ DLL 呼び出し前に `Folder.Startup\FrameServer.dll` の存在チェックを追加。不在時は親切なエラーメッセージを表示し、System32 の誤ロードを防止。
   - [Source/General/Localization.vb](Source/General/Localization.vb) & [Source/Settings/Languages/ja.json](Source/Settings/Languages/ja.json): `FrameServer.dll` 不足メッセージの日本語訳を追加。
   - [.gitignore](.gitignore): `*.7z`, `*.zip`, `scratch/` を追記。
3. **実行環境セットアップ（方法B）**:
   - 公式 `StaxRip-v2.52.5-x64.7z` をダウンロード・展開し、`Apps`、`Fonts`、`Icons`、`FrameServer.dll` を `Source\bin` に配置。
   - 最新の日本語化版 `StaxRip.exe` および `ja.json` を再ビルド・配置。
   - ヘッドレス自動テストを実行して全パス（All Passed）を確認。
   - 確認完了後、ダウンロードした大容量一時アーカイブ（約704MB）を完全に削除。
- **総合結果**: **All Passed!**
