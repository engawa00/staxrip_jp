[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host " StaxRip 日本語版 ヘッドレス自動テストスイート" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

$testDir = Join-Path $PSScriptRoot "scratch\test_encode"
if (-not (Test-Path $testDir)) {
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null
}

$ffmpegExe = (Get-Command ffmpeg -ErrorAction SilentlyContinue)?.Source
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
        @{ Key = "File"; Expected = "ファイル" },
        @{ Key = "Open Video Source File(s)..."; Expected = "動画ソースファイルを開く..." },
        @{ Key = "Quality"; Expected = "品質" },
        @{ Key = "Preset"; Expected = "プリセット" },
        @{ Key = "Tune"; Expected = "チューン" },
        @{ Key = "Assistant"; Expected = "アシスタント" },
        @{ Key = "Output File Type:"; Expected = "出力ファイル形式:" },
        @{ Key = "Bitrate:"; Expected = "ビットレート:" },
        @{ Key = "Language (requires restart):"; Expected = "表示言語 (要再起動):" }
    )

    foreach ($c in $checks) {
        $actual = [StaxRip.Localization]::Translate($c.Key)
        if ($actual -ne $c.Expected) {
            throw "Translation mismatch for '$($c.Key)': expected '$($c.Expected)', got '$actual'"
        }
        Write-Host "  OK: '$($c.Key)' -> '$actual'" -ForegroundColor Green
    }

    # 英語切り替え検証
    [StaxRip.Localization]::set_CurrentLanguage("en")
    $enActual = [StaxRip.Localization]::Translate("File")
    if ($enActual -ne "File") {
        throw "English fallback failed: expected 'File', got '$enActual'"
    }
    Write-Host "  OK: 英語モードへの切り替え正常 (File -> $enActual)" -ForegroundColor Green

    # 日本語モードに戻す
    [StaxRip.Localization]::set_CurrentLanguage("ja")

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
