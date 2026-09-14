[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host " StaxRip 日本語版 ヘッドレス自動テストスイート" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

$testDir = Join-Path $PSScriptRoot "scratch\test_encode"
if (-not (Test-Path $testDir)) {
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null
}

$ffmpegCmd = Get-Command ffmpeg -ErrorAction SilentlyContinue
$ffmpegExe = if ($ffmpegCmd) { $ffmpegCmd.Source } else { $null }
if (-not $ffmpegExe -or -not (Test-Path $ffmpegExe)) {
    $candidates = @(
        (Join-Path $env:LOCALAPPDATA "Python\pythoncore-3.14-64\Scripts\ffmpeg.exe"),
        (Join-Path $PSScriptRoot "Apps\Support\ffmpeg\ffmpeg.exe")
    )
    foreach ($c in $candidates) {
        if ($c -and (Test-Path $c)) {
            $ffmpegExe = $c
            break
        }
    }
}
if (-not $ffmpegExe -or -not (Test-Path $ffmpegExe)) {
    throw "ffmpeg.exe not found. Please ensure ffmpeg is available in PATH."
}

try {
    # -------------------------------------------------------------
    # テスト 1: ローカライズエンジンのヘッドレステスト
    # -------------------------------------------------------------
    Write-Host "`n[テスト 1] ローカライズコアエンジンの検証..." -ForegroundColor Yellow

    $assemblyPath = Resolve-Path (Join-Path $PSScriptRoot "Source\bin\StaxRip.exe")
    [Reflection.Assembly]::LoadFrom($assemblyPath.Path) | Out-Null

    # 日本語モード検証
    [StaxRip.Localization]::set_CurrentLanguage("ja")
    
    $checks = @(
        # 基本メニュー・UI
        @{ Key = "File"; Expected = "ファイル" },
        @{ Key = "Open Video Source File(s)..."; Expected = "動画ソースファイルを開く..." },
        @{ Key = "Quality"; Expected = "品質" },
        @{ Key = "Preset"; Expected = "プリセット" },
        @{ Key = "Tune"; Expected = "チューン" },
        @{ Key = "Assistant"; Expected = "アシスタント" },
        @{ Key = "Output File Type:"; Expected = "出力ファイル形式:" },
        @{ Key = "Bitrate:"; Expected = "ビットレート:" },
        @{ Key = "Language (requires restart):"; Expected = "表示言語 (要再起動):" },

        # エンコーダー: タブ名・カテゴリ
        @{ Key = "Slice Decision"; Expected = "スライス判定" },
        @{ Key = "Motion Search"; Expected = "動き探索" },
        @{ Key = "GOP size/type"; Expected = "GOP構造・サイズ" },
        @{ Key = "AV1 Specific 1"; Expected = "AV1固有設定 1" },
        @{ Key = "Color Description"; Expected = "色情報記述" },
        @{ Key = "Variance Boost Options"; Expected = "分散ブースト設定" },
        @{ Key = "Ngx-TrueHDR"; Expected = "Ngx-TrueHDR" },
        @{ Key = "Deband"; Expected = "バンディング低減 (Deband)" },
        @{ Key = "LibPlacebo"; Expected = "LibPlacebo" },
        @{ Key = "Tonemapping"; Expected = "トーンマッピング" },
        @{ Key = "Sharpness"; Expected = "鮮鋭化 (シャープ)" },

        # エンコーダー: パラメータ名
        @{ Key = "Decoder"; Expected = "デコーダー" },
        @{ Key = "Target Bitrate"; Expected = "目標ビットレート" },
        @{ Key = "VBV Buffer Size"; Expected = "VBVバッファサイズ" },
        @{ Key = "Constrained Quality"; Expected = "制限付き品質 (CQ)" },
        @{ Key = "Adaptive Quantization"; Expected = "適応量子化 (AQ)" },
        @{ Key = "Dolby Vision RPU"; Expected = "Dolby Vision RPU メタデータ" },
        @{ Key = "Dynamic Peak Detection"; Expected = "動的ピーク輝度検出" },
        @{ Key = "Gamut Mapping"; Expected = "広色域マッピング (Gamut Mapping)" },
        @{ Key = "Lookahead"; Expected = "先行探索フレーム数 (Lookahead)" },
        @{ Key = "Film Grain"; Expected = "フィルムグレイン (Film Grain)" },
        @{ Key = "Speed"; Expected = "エンコード速度" },
        @{ Key = "Fast Decode"; Expected = "高速デコード優先 (Fast Decode)" },

        # エンコーダー: 選択肢
        @{ Key = "QVBR: Constant Quality Mode"; Expected = "QVBR: 固定品質モード" },
        @{ Key = "NVEnc Hardware"; Expected = "NVEnc ハードウェアデコード" },
        @{ Key = "QSVEnc (Intel)"; Expected = "QSVEnc (Intel ハードウェア)" },
        @{ Key = "P1 (Performance)"; Expected = "P1 (最速・低負荷)" },
        @{ Key = "P7 (Quality)"; Expected = "P7 (最高品質・高負荷)" },
        @{ Key = "8-Bit"; Expected = "8ビット (8-Bit)" },
        @{ Key = "10-Bit"; Expected = "10ビット (10-Bit)" },

        # エンコーダー コントロール画面
        @{ Key = "Target Name Override"; Expected = "出力ファイル名を上書き" },
        @{ Key = "Container Options"; Expected = "コンテナ設定" },
        @{ Key = "Run Compressibility Check"; Expected = "圧縮率チェックを実行" },
        @{ Key = "Super high quality and file size"; Expected = "最高品質（ファイルサイズ大）" },

        # フィルター: UI・メニュー
        @{ Key = "AVS Filters"; Expected = "AviSynth フィルター" },
        @{ Key = "VS Filters"; Expected = "VapourSynth フィルター" },
        @{ Key = "Replace"; Expected = "置換" },
        @{ Key = "Insert"; Expected = "挿入" },
        @{ Key = "Edit Code..."; Expected = "コードを編集..." },
        @{ Key = "Removes the selected filter."; Expected = "選択したフィルターを削除します。" },

        # フィルター: 対話プロンプト
        @{ Key = "Please select one of the options."; Expected = "以下の選択肢から1つ選択してください。" },
        @{ Key = "Enable Auto Gain?"; Expected = "自動ゲイン (Auto Gain) を有効にしますか？" },
        @{ Key = "Is the Input using TV Range?"; Expected = "入力はTVレンジ (Limited 16-235) ですか？" },
        @{ Key = "Select Input Color Matrix"; Expected = "入力カラーマトリックスを選択してください:" },
        @{ Key = "Select the Bit Depth you want to convert to"; Expected = "変換先の色深度 (ビット数) を選択してください:" },

        # ダイアログボタン・定型アクション
        @{ Key = "Yes"; Expected = "はい" },
        @{ Key = "No"; Expected = "いいえ" },
        @{ Key = "Cancel"; Expected = "キャンセル" },
        @{ Key = "Retry"; Expected = "再試行" },
        @{ Key = "Copy Message"; Expected = "メッセージをコピー" },
        @{ Key = "Select a template"; Expected = "テンプレートの選択" },
        @{ Key = "Please select a template you want to use:"; Expected = "使用するテンプレートを選択してください:" },

        # エラーメッセージ (MsgError / 例外)
        @{ Key = "The first filter must be a source filter."; Expected = "最初のフィルターはソースフィルターである必要があります。" },
        @{ Key = "Source file not found!"; Expected = "ソースファイルが見つかりません！" },
        @{ Key = "Project file not found!"; Expected = "プロジェクトファイルが見つかりません！" },
        @{ Key = "Script Error"; Expected = "スクリプトエラー" },
        @{ Key = "The temp folder could not be created."; Expected = "一時フォルダを作成できませんでした。" },
        @{ Key = "Only fixed local drives are supported as temp dir."; Expected = "一時フォルダには固定ローカルドライブのみ指定可能です。" },
        @{ Key = "Only idx, srt and ass file types are supported."; Expected = "対応している字幕形式は idx, srt, ass のみです。" },

        # 警告メッセージ (MsgWarn)
        @{ Key = "Windows Terminal not found!"; Expected = "Windows Terminal が見つかりません！" },
        @{ Key = "Compatibility problem!"; Expected = "互換性の問題" },
        @{ Key = "Source file is missing!"; Expected = "ソースファイルが見つかりません！" },
        @{ Key = "Assistant warning cannot be skipped."; Expected = "アシスタントの警告をスキップすることはできません。" },

        # 情報メッセージ (MsgInfo)
        @{ Key = "All Good!"; Expected = "すべて正常です！" },
        @{ Key = "Please restart StaxRip."; Expected = "StaxRip を再起動してください。" },
        @{ Key = "The profile was saved."; Expected = "プロファイルを保存しました。" },
        @{ Key = "Folder was added to PATH"; Expected = "フォルダを PATH に追加しました" },
        @{ Key = "Folder was removed from PATH"; Expected = "フォルダを PATH から削除しました" },

        # 質問・確認メッセージ (MsgQuestion)
        @{ Key = "Are you sure you want to reset your settings? Your current settings will be lost!"; Expected = "設定をリセットしてもよろしいですか？現在の設定は失われます！" },
        @{ Key = "Restore defaults?"; Expected = "初期設定に戻しますか？" },
        @{ Key = "Confirm to process ALL audio tracks."; Expected = "すべての音声トラックの処理を実行しますか？" },
        @{ Key = "This might take a while..."; Expected = "少し時間がかかる場合があります..." },

        # サブメニュー・ダイアログUI
        @{ Key = "Apps Management"; Expected = "外部ツールの管理" },
        @{ Key = "Edit Path"; Expected = "パスを編集" },
        @{ Key = "Check All"; Expected = "すべての状態を確認" },
        @{ Key = "Auto Update"; Expected = "自動アップデート" },
        @{ Key = "Job"; Expected = "ジョブ" },
        @{ Key = "Duration"; Expected = "所要時間" },
        @{ Key = "Add Audio Track"; Expected = "音声トラックを追加" },
        @{ Key = "Audio Streams"; Expected = "音声ストリーム" }
    )

    foreach ($c in $checks) {
        $actual = [StaxRip.Localization]::Translate($c.Key)
        if ($actual -ne $c.Expected) {
            throw "Translation mismatch for '$($c.Key)': expected '$($c.Expected)', got '$actual'"
        }
        Write-Host "  OK: '$($c.Key)' -> '$actual'" -ForegroundColor Green
    }

    # コントロール再帰的ローカライズ (ApplyLocalization) の検証
    Write-Host "`n  [サブテスト] FormBase / コントロール再帰的ローカライズ (ApplyLocalization) 検証..." -ForegroundColor Gray
    $dummyForm = New-Object System.Windows.Forms.Form
    $dummyForm.Text = "Options"
    $dummyBtn = New-Object System.Windows.Forms.Button
    $dummyBtn.Text = "Cancel"
    $dummyLbl = New-Object System.Windows.Forms.Label
    $dummyLbl.Text = "Quality"
    $dummyForm.Controls.Add($dummyBtn)
    $dummyForm.Controls.Add($dummyLbl)

    [StaxRip.Localization]::ApplyLocalization($dummyForm)

    if ($dummyForm.Text -ne "設定") {
        throw "Form.Text translation failed: expected '設定', got '$($dummyForm.Text)'"
    }
    if ($dummyBtn.Text -ne "キャンセル") {
        throw "Button.Text translation failed: expected 'キャンセル', got '$($dummyBtn.Text)'"
    }
    if ($dummyLbl.Text -ne "品質") {
        throw "Label.Text translation failed: expected '品質', got '$($dummyLbl.Text)'"
    }
    $dummyForm.Dispose()
    Write-Host "  OK: ApplyLocalization による Form / Button / Label の自動翻訳正常" -ForegroundColor Green

    # 英語切り替え検証
    [StaxRip.Localization]::set_CurrentLanguage("en")
    $enActual = [StaxRip.Localization]::Translate("File")
    if ($enActual -ne "File") {
        throw "English fallback failed: expected 'File', got '$enActual'"
    }
    Write-Host "  OK: 英語モードへの切り替え正常 (File -> $enActual)" -ForegroundColor Green

    # 日本語モードに戻す
    [StaxRip.Localization]::set_CurrentLanguage("ja")

    $ErrorActionPreference = "Continue"

    # -------------------------------------------------------------
    # テスト 2: テスト用ソース動画の生成 (1秒テストパターン)
    # -------------------------------------------------------------
    Write-Host "`n[テスト 2] テスト用ソース動画の生成 (ヘッドレス)..." -ForegroundColor Yellow
    $sourceVideo = Join-Path $testDir "source_test.mp4"
    
    # 320x240, 30fps, 1秒のテストパターン動画を生成 (確実な映像単一入力)
    & $ffmpegExe -y -f lavfi -i "testsrc=size=320x240:rate=30" -t 1 -c:v libx264 -pix_fmt yuv420p "$sourceVideo" 2>$null
    if (-not (Test-Path $sourceVideo)) {
        throw "Failed to create source test video."
    }
    Write-Host "  OK: テスト用ソース動画作成完了 ($((Get-Item $sourceVideo).Length) bytes)" -ForegroundColor Green

    # -------------------------------------------------------------
    # テスト 3: 複数オプションでの動画エンコードテスト
    # -------------------------------------------------------------
    Write-Host "`n[テスト 3] 複数オプションでの動画エンコード検証..." -ForegroundColor Yellow

    # オプションパターン 1: x264 CRF 23, Preset fast
    Write-Host "  [パターン 1] x264 - CRF 23, Preset: fast" -ForegroundColor Gray
    $out1 = Join-Path $testDir "out_crf23_fast.mp4"
    & $ffmpegExe -y -i "$sourceVideo" -c:v libx264 -crf 23 -preset fast -an "$out1" 2>$null
    if (-not (Test-Path $out1) -or (Get-Item $out1).Length -eq 0) {
        throw "Pattern 1 encode failed."
    }
    Write-Host "    -> 成功! サイズ: $((Get-Item $out1).Length) bytes" -ForegroundColor Green

    # オプションパターン 2: x264 CRF 28, Preset veryfast, 解像度リサイズ (640x360)
    Write-Host "  [パターン 2] x264 - CRF 28, Preset: veryfast, 解像度リサイズ 640x360" -ForegroundColor Gray
    $out2 = Join-Path $testDir "out_crf28_veryfast_resize.mp4"
    & $ffmpegExe -y -i "$sourceVideo" -vf "scale=640:360" -c:v libx264 -crf 28 -preset veryfast -an "$out2" 2>$null
    if (-not (Test-Path $out2) -or (Get-Item $out2).Length -eq 0) {
        throw "Pattern 2 encode failed."
    }
    Write-Host "    -> 成功! サイズ: $((Get-Item $out2).Length) bytes" -ForegroundColor Green

    # オプションパターン 3: x264 2パス固定ビットレート (500 kbps)
    Write-Host "  [パターン 3] x264 - 2パス 固定ビットレート 500kbps" -ForegroundColor Gray
    $out3 = Join-Path $testDir "out_2pass_500k.mp4"
    $passLog = Join-Path $testDir "ffmpeg2pass"
    # Pass 1
    & $ffmpegExe -y -i "$sourceVideo" -c:v libx264 -b:v 500k -pass 1 -passlogfile "$passLog" -an -f null NUL 2>$null
    # Pass 2
    & $ffmpegExe -y -i "$sourceVideo" -c:v libx264 -b:v 500k -pass 2 -passlogfile "$passLog" -an "$out3" 2>$null
    if (-not (Test-Path $out3) -or (Get-Item $out3).Length -eq 0) {
        throw "Pattern 3 encode failed."
    }
    Write-Host "    -> 成功! サイズ: $((Get-Item $out3).Length) bytes" -ForegroundColor Green

    # オプションパターン 4: チューン指定 (Film / Animation) & プロファイル (High / Baseline)
    Write-Host "  [パターン 4] x264 - Tune: film, Profile: high, CRF 20" -ForegroundColor Gray
    $out4 = Join-Path $testDir "out_tune_film.mp4"
    & $ffmpegExe -y -i "$sourceVideo" -c:v libx264 -crf 20 -tune film -profile:v high -an "$out4" 2>$null
    if (-not (Test-Path $out4) -or (Get-Item $out4).Length -eq 0) {
        throw "Pattern 4 encode failed."
    }
    Write-Host "    -> 成功! サイズ: $((Get-Item $out4).Length) bytes" -ForegroundColor Green

    Write-Host "`nすべてのテストが正常にパスしました!" -ForegroundColor Cyan

} finally {
    # -------------------------------------------------------------
    # クリーンアップ: 生成した動画ファイルをすべて削除
    # -------------------------------------------------------------
    Write-Host "`n[クリーンアップ] テスト生成ファイルを削除中 (残さない方針)..." -ForegroundColor Yellow
    if (Test-Path $testDir) {
        Remove-Item -Path $testDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    # 2pass ログファイルの残骸があれば削除
    Get-ChildItem -Path . -Filter "ffmpeg2pass*" -ErrorAction SilentlyContinue | Remove-Item -Force
    Write-Host "  OK: クリーンアップ完了 (動画ファイルは一切残っていません)" -ForegroundColor Green
}

Write-Host "`n=====================================================" -ForegroundColor Cyan
Write-Host " テスト完了: All Passed!" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Cyan
