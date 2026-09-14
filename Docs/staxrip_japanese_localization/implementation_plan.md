# 実装計画: エンコーダー（NVEnc/QSVEnc/VCEEnc/SVT-AV1）およびフィルター詳細設定の日本語化拡充

handover.md (Line 58) に記載された次フェーズ作業に基づき、NVEnc, QSVEnc, VCEEnc, SVT-AV1（各バリアント含む）および AviSynth+/VapourSynth フィルター固有の詳細設定・UI・ヘルプテキスト・対話プロンプトの包括的な日本語化を実施します。

---

## ユーザー確認・承認が必要な事項

> [!IMPORTANT]
> - **テスト方針**: 既存のルールに従い、検証は**完全ヘッドレス**（GUI ウィンドウを表示させず、.NET リフレクションおよび CLI 実行）で行います。また、テスト中に生成された動画ファイルはテスト終了時に**必ず全てクリーンアップ（削除）**します。
> - **辞書管理方針**: 基本辞書は `Source/General/Localization.vb` に内蔵し、ユーザーカスタマイズ可能な外部ファイル `Source/Settings/Languages/ja.json` にも同期して出力・配置します。

---

## 提案する変更内容

### 1. エンコーダー固有設定・UI のローカライズ拡充

#### [MODIFY] [Source/General/Localization.vb](Source/General/Localization.vb)
- **タブ名・カテゴリ階層の日本語訳**:
  - `Input/Output`（入出力）、`Basic`（基本設定）、`Rate Control`（レート制御）、`Slice Decision`（スライス判定）、`Analysis`（分析・予測）、`Motion Search`（動き探索）、`Performance`（パフォーマンス）、`Statistic`（統計情報）
  - `VPP`（VPPフィルター）、`Colorspace`（色空間）、`HDR2SDR`（HDR→SDR変換）、`Ngx-TrueHDR`（Ngx-TrueHDR）、`Deband`（バンディング低減）、`LibPlacebo`（LibPlacebo）、`Tonemapping`（トーンマッピング）、`Resize`（リサイズ）、`Sharpness`（鮮鋭化）、`Deinterlace`（インターレース解除）、`AFS 2`（AFS 自動フィールドシフト 2）
  - `GOP size/type`（GOP構造・サイズ）、`AV1 Specific 1/2`（AV1固有設定 1/2）、`Color Description`（色情報記述）、`Variance Boost Options`（分散ブースト設定）、`Codec Specific`（コーデック固有設定）など
- **エンコーダー詳細パラメータ名（約490項目）の日本語訳**:
  - レート制御（CBR, VBR, CQP, QVBR, CRF, 目標ビットレート, 最大ビットレート, VBVバッファサイズ, 初期QP, 最小/最大QP等）
  - フレーム構造・予測（GOP長, 最小GOP長, Bフレーム数, 参照フレーム数, 適応量子化 AQ-Mode / AQ Strength / CAQ, 動き探索範囲等）
  - 色空間・VUI（Color Primaries, Transfer, Color Matrix, Range, Mastering Display, MaxCLL/MaxFALL, Dolby Vision RPU 等）
  - VPP フィルター各パラメータ（Denoise 強度, Deband 閾値, Libplacebo 各種シェーダー/トーンマッピングパラメータ等）
- **ドロップダウン選択肢（`.Options`）の日本語訳**:
  - 各エンコーダーのモード、プリセット（P1〜P7、Quality、Performance等）、チューン（HQ, Low Latency, Lossless等）、プロファイル（Main, High, Main 10等）
- **コントロール共通項目**:
  - `Output Depth`（出力色深度 / 8-Bit, 10-Bit）、`Fast Decode`（高速デコード優先）、`Lookahead`（先行探索フレーム数）、`Film Grain`（フィルムグレイン生成/除去）等

#### [MODIFY] [Source/Controls/NVEncControl.vb](Source/Controls/NVEncControl.vb)
- メイン画面の NVEnc コントロール内リスト項目（Quality, Mode, Preset, Tune, Output Depth, DV Profile, Color Range）および各ボタン（"Name Override", "Options", "Container Options", "Run Compressibility Check"）の `Localization.Translate` 適用。

#### [MODIFY] [Source/Controls/SvtAv1EncAppControl.vb](Source/Controls/SvtAv1EncAppControl.vb)
- メイン画面の SVT-AV1 コントロール内リスト項目（Quality, Preset, Tune, Fast Decode, Lookahead, Film Grain）およびボタン類の `Localization.Translate` 適用。
- 他の SVT-AV1 バリアントコントロール（`SvtAv1EncAppEssentialControl.vb`, `SvtAv1EncAppHdrControl.vb`, `SvtAv1EncAppPsyexControl.vb`, `SvtAv1EncAppTritiumControl.vb`）も必要に応じて同様に対応。

---

### 2. AviSynth+ / VapourSynth フィルター関連のローカライズ拡充

#### [MODIFY] [Source/Controls/FiltersListView.vb](Source/Controls/FiltersListView.vb)
- リストビューのコンテキストメニュー（`active`, `Replace`, `Insert`, `Add`, `Remove`, `Edit Code...`, `Preview Code...`, `Info...`, `Play`, `Profiles...`, `Move Up`, `Move Down`, `Filter Setup`）およびツールチップ説明文の `Localization.Translate` 適用。

#### [MODIFY] [Source/General/Macro.vb](Source/General/Macro.vb)
- フィルタープロファイル等で使われるダイアログマクロ（`$enter_text:...$` や `$select:msg:...$`）のプロンプト表示テキスト、TaskDialog のタイトルおよび選択肢表示ラベルに `Localization.Translate` を適用（スクリプトに代入される実値は変更せず、UI 表示名のみ翻訳）。

#### [MODIFY] [Source/General/Localization.vb](Source/General/Localization.vb)
- フィルターカテゴリ名（`Source`, `Color`, `Field`, `Frame`, `Denoise`, `Sharpen`, `Resize`, `Misc`, `Subtitles`, `HDR to SDR`, `Tonemap` 等）の登録。
- フィルターマクロのプロンプト（`Is the Input using TV Range?`, `Select Input Color Matrix`, `Enable Auto Gain?`, `HDR max mastering luminance level (in cd/m2)?` 等）の日本語訳登録。

---

### 3. 外部辞書ファイルおよびプロジェクトドキュメント

#### [MODIFY] [Source/Settings/Languages/ja.json](Source/Settings/Languages/ja.json)
- 追加した主要エントリーを `ja.json` にも反映。

#### [MODIFY] [docs/staxrip_japanese_localization/task.md](docs/staxrip_japanese_localization/task.md)
- 今回のフェーズ（フェーズ 7〜11）のタスクを追加・進捗管理。

#### [MODIFY] [docs/staxrip_japanese_localization/walkthrough.md](docs/staxrip_japanese_localization/walkthrough.md)
- 実装・検証結果のまとめを追記。

#### [MODIFY] [docs/staxrip_japanese_localization/handover.md](docs/staxrip_japanese_localization/handover.md)
- 本タスク完了後の最新状態に再構成。

---

## 検証計画

### 自動テスト（完全ヘッドレス）
1. **コンパイル検証**:
   - `dotnet msbuild Source\StaxRip.vbproj /p:Configuration=Release /p:Platform=x64`
   - エラー 0、警告 0 を確認。
2. **ヘッドレステスト実行 (`run_headless_tests.ps1`)**:
   - PowerShell スクリプトから .NET リフレクション経由で `Localization.Translate` を呼び出し：
     - NVEnc/QSVEnc/VCEEnc/SVT-AV1 のタブ名、主要パラメータ、選択肢の翻訳が正しく行われるか検証。
     - FiltersListView の各メニュー項目およびマクロプロンプトの翻訳が正しく行われるか検証。
     - 言語を "en" に切り替えた際に原文英語がそのまま返ることを検証。
   - 既存の動画エンコードテスト（FFmpeg 合成ソースを用いたエンコード実行）が全てパスすることを確認。
   - テスト完了後にテスト動画ファイルが完全に削除されていることを確認。
