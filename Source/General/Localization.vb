Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports StaxRip.UI

Namespace Global.StaxRip
    Public Module Localization
        Private Initialized As Boolean = False
        Private Dict As New Dictionary(Of String, String)(StringComparer.Ordinal)

        Public Event CurrentLanguageChanged As Action(Of String)

        Private _CurrentLanguage As String = "ja"

        Public Property CurrentLanguage As String
            Get
                Return _CurrentLanguage
            End Get
            Set(value As String)
                If String.IsNullOrEmpty(value) Then value = "ja"
                If _CurrentLanguage <> value OrElse Not Initialized Then
                    _CurrentLanguage = value
                    LoadDictionary(value)
                    RaiseEvent CurrentLanguageChanged(value)
                End If
            End Set
        End Property

        Public ReadOnly Property AvailableLanguages As String()
            Get
                Return {"ja", "en"}
            End Get
        End Property

        Public Function Translate(text As String) As String
            If String.IsNullOrEmpty(text) Then Return text
            If CurrentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase) Then Return text

            If Not Initialized Then
                Init()
            End If

            Dim translated As String = Nothing
            If Dict.TryGetValue(text, translated) Then
                Return translated
            End If

            '末尾のコロン付き文字列（例: "Bitrate:" -> "ビットレート:"）
            If text.EndsWith(":") Then
                Dim baseText = text.Substring(0, text.Length - 1)
                If Dict.TryGetValue(baseText, translated) Then
                    Return translated + ":"
                End If
            End If

            '末尾の "..." 付き文字列（例: "Open..." -> "開く..."）
            If text.EndsWith("...") Then
                Dim baseText = text.Substring(0, text.Length - 3)
                If Dict.TryGetValue(baseText, translated) Then
                    Return translated + "..."
                End If
            End If

            Return text
        End Function

        Public Function Translate(format As String, ParamArray args As Object()) As String
            Dim pattern = Translate(format)
            Try
                Return String.Format(pattern, args)
            Catch
                Return pattern
            End Try
        End Function

        Public Function GetText(text As String) As String
            Return Translate(text)
        End Function

        Public Function GetText(format As String, ParamArray args As Object()) As String
            Return Translate(format, args)
        End Function

        Public Sub Init()
            If Initialized Then Return
            Initialized = True
            LoadDictionary(CurrentLanguage)
        End Sub

        Public Sub LoadDictionary(lang As String)
            Dict.Clear()

            If lang.Equals("en", StringComparison.OrdinalIgnoreCase) Then
                Exit Sub
            End If

            '既定の内蔵日本語辞書を登録
            LoadDefaultJapaneseDictionary()

            '外部辞書ファイル（Settings\Languages\ja.txt または ja.json）があれば追加・上書き
            Try
                Dim langDir As String = Nothing
                If g.SettingsFolderExists Then
                    langDir = Path.Combine(Folder.Settings, "Languages")
                Else
                    Dim localSettings = Path.Combine(Folder.Startup, "Settings", "Languages")
                    If Directory.Exists(localSettings) Then langDir = localSettings
                End If

                If Not String.IsNullOrEmpty(langDir) AndAlso Directory.Exists(langDir) Then
                    Dim customTxt = Path.Combine(langDir, lang + ".txt")
                    If File.Exists(customTxt) Then
                        LoadFromTextFile(customTxt)
                    End If

                    Dim customJson = Path.Combine(langDir, lang + ".json")
                    If File.Exists(customJson) Then
                        LoadFromJsonFile(customJson)
                    End If
                End If
            Catch ex As Exception
                'ログ記録
            End Try
        End Sub

        Private Sub LoadFromTextFile(filePath As String)
            For Each line In File.ReadAllLines(filePath, Encoding.UTF8)
                Dim trimmed = line.Trim()
                If trimmed = "" OrElse trimmed.StartsWith("#") OrElse trimmed.StartsWith("//") Then Continue For

                Dim eqIdx = trimmed.IndexOf("="c)
                If eqIdx > 0 Then
                    Dim key = trimmed.Substring(0, eqIdx).Trim()
                    Dim val = trimmed.Substring(eqIdx + 1).Trim()
                    If key <> "" Then
                        Dict(key) = val
                    End If
                End If
            Next
        End Sub

        Private Sub LoadFromJsonFile(filePath As String)
            Dim content = File.ReadAllText(filePath, Encoding.UTF8)
            Dim matches = Regex.Matches(content, """((?:\\.|[^""\\])*)""\s*:\s*""((?:\\.|[^""\\])*)""")
            For Each m As Match In matches
                Dim key = Regex.Unescape(m.Groups(1).Value)
                Dim val = Regex.Unescape(m.Groups(2).Value)
                If key <> "" Then
                    Dict(key) = val
                End If
            Next
        End Sub

        Private Sub LoadDefaultJapaneseDictionary()
            '=== メインメニュー: File ===
            Dict("File") = "ファイル"
            Dict("Open Video Source File(s)...") = "動画ソースファイルを開く..."
            Dict("Single File") = "単一ファイル"
            Dict("Multiple Files") = "複数ファイル"
            Dict("Batch Multiple Files") = "複数ファイルの一括処理 (バッチ)"
            Dict("Merge Files") = "ファイルの結合"
            Dict("Blu-ray Image File") = "Blu-ray イメージファイル (ISO)"
            Dict("Blu-ray Folder") = "Blu-ray フォルダ"
            Dict("Demux...") = "ストリーム分離 (Demux)..."
            Dict("Video Comparison...") = "動画の比較・検証..."
            Dict("Open Project...") = "プロジェクトを開く..."
            Dict("Save Project") = "プロジェクトを保存"
            Dict("Save Project As...") = "名前を付けてプロジェクトを保存..."
            Dict("Save Project As Template...") = "プロジェクトをテンプレートとして保存..."
            Dict("Close Project") = "プロジェクトを閉じる"
            Dict("Project Templates") = "プロジェクトテンプレート"
            Dict("Recent Projects") = "最近開いたプロジェクト"
            Dict("Launch New Instance") = "新しいウィンドウを開く"
            Dict("Exit") = "終了"

            '=== メインメニュー: Crop, Preview ===
            Dict("Crop") = "クロップ (切り抜き)"
            Dict("Preview") = "プレビュー"

            '=== メインメニュー: Project ===
            Dict("Project") = "プロジェクト"
            Dict("Add Hardcoded Subtitle...") = "焼き込み字幕の追加..."
            Dict("Script Info...") = "スクリプト情報..."
            Dict("Advanced Script Info...") = "高度なスクリプト情報..."
            Dict("Log File") = "ログファイル"
            Dict("Folders") = "フォルダ"
            Dict("Source") = "ソース"
            Dict("Target") = "ターゲット (出力)"
            Dict("Temp") = "一時フォルダ (Temp)"
            Dict("Working") = "作業フォルダ"
            Dict("Options") = "プロジェクト設定"

            '=== メインメニュー: Tools ===
            Dict("Tools") = "ツール"
            Dict("Jobs...") = "ジョブ一覧..."
            Dict("Log Files") = "ログファイル"
            Dict("Plugins") = "プラグイン"
            Dict("Programs") = "外部プログラム"
            Dict("Scripts") = "スクリプト"
            Dict("Settings") = "設定"
            Dict("Startup") = "スタートアップ"
            Dict("System") = "システム"
            Dict("Templates") = "テンプレート"
            Dict("Advanced") = "高度なツール"
            Dict("Event Commands...") = "イベントコマンド..."
            Dict("Macros...") = "マクロ管理..."
            Dict("Reset Settings...") = "設定のリセット..."
            Dict("Command Prompt") = "コマンドプロンプト"
            Dict("PowerShell Terminal") = "PowerShell ターミナル"
            Dict("Windows Terminal") = "Windows ターミナル"
            Dict("Generate Wiki Content") = "Wiki用コンテンツを生成"
            Dict("Ingest HDR") = "HDR情報の取り込み"
            Dict("Edit Menu...") = "メニューの編集..."
            Dict("Settings...") = "環境設定..."

            '=== メインメニュー: Apps ===
            Dict("Apps") = "外部アプリ"
            Dict("Subtitles") = "字幕ツール"
            Dict("Media Info") = "メディア情報"
            Dict("Players") = "プレイヤー"
            Dict("Indexing") = "インデックス"
            Dict("Thumbnails") = "サムネイル"
            Dict("Animation") = "アニメーション"
            Dict("Other") = "その他"
            Dict("Manage...") = "外部ツールの管理..."
            Dict("Test...") = "テスト..."

            '=== メインメニュー: Help ===
            Dict("Help") = "ヘルプ"
            Dict("Website") = "公式サイト"
            Dict("Documentation") = "ドキュメント (公式)"
            Dict("Changelog") = "更新履歴 (Changelog)"
            Dict("Support") = "サポート・寄付"
            Dict("Report an issue") = "問題を報告 (Issue)"
            Dict("Discord Server") = "公式 Discord サーバー"
            Dict("Check for Updates") = "アップデートを確認"
            Dict("What's new...") = "更新内容..."
            Dict("Info...") = "バージョン情報..."

            '=== メイン画面: グループ・コントロール ===
            Dict("Assistant") = "アシスタント"
            Dict("Audio") = "音声"
            Dict("Size") = "サイズ"
            Dict("Filters") = "フィルター"
            Dict("Encoder") = "エンコーダー"
            Dict("Next") = "次へ"
            Dict("Bitrate") = "ビットレート"
            Dict("Pixel") = "ピクセル"
            Dict("Zoom") = "ズーム"
            Dict("Quality") = "品質"
            Dict("Mode") = "モード"
            Dict("Preset") = "プリセット"
            Dict("Tune") = "チューン"
            Dict("Profile") = "プロファイル"
            Dict("Level") = "レベル"
            Dict("Container") = "コンテナ"

            '=== コンテキストメニュー・ボタン ===
            Dict("Add to top and open Jobs") = "ジョブ一覧の先頭に追加して開く"
            Dict("Add to bottom and open Jobs") = "ジョブ一覧の末尾に追加して開く"
            Dict("Add to top w/o opening Jobs") = "ジョブ一覧の先頭に追加 (開かない)"
            Dict("Add to bottom w/o opening Jobs") = "ジョブ一覧の末尾に追加 (開かない)"
            Dict("Options") = "設定"
            Dict("Name Override") = "名前の上書き"
            Dict("Codec Configuration") = "コーデック詳細設定"
            Dict("Container Configuration") = "コンテナ詳細設定"
            Dict("Run Compressibility Check") = "圧縮率チェックを実行"
            Dict("Compressibility Check") = "圧縮率チェック"
            Dict("Aimed Quality (%):") = "目標品質 (%):"
            Dict("Output File Type:") = "出力ファイル形式:"
            Dict("Command Line") = "コマンドライン"
            Dict("Execute Command Line") = "コマンドラインを実行"
            Dict("Copy Command Line") = "コマンドラインをコピー"
            Dict("Help about this dialog") = "このダイアログのヘルプ"

            '=== 共通ボタン・UI ===
            Dict("OK") = "OK"
            Dict("Cancel") = "キャンセル"
            Dict("Close") = "閉じる"
            Dict("Apply") = "適用"
            Dict("Add") = "追加"
            Dict("Remove") = "削除"
            Dict("Delete") = "削除"
            Dict("Edit") = "編集"
            Dict("Up") = "上へ"
            Dict("Down") = "下へ"
            Dict("Start") = "開始"
            Dict("Stop") = "停止"
            Dict("Pause") = "一時停止"
            Dict("Resume") = "再開"
            Dict("Browse...") = "参照..."
            Dict("Save") = "保存"
            Dict("Reset") = "リセット"
            Dict("Defaults") = "初期値に戻す"
            Dict("Search") = "検索"
            Dict("Clear") = "クリア"
            Dict("Select All") = "すべて選択"
            Dict("Select None") = "選択を解除"

            '=== エンコーダー / パラメータタブ・カテゴリ ===
            Dict("General") = "全般"
            Dict("Basic") = "基本設定"
            Dict("Rate Control") = "レート制御"
            Dict("Performance") = "パフォーマンス"
            Dict("Frame Type") = "フレーム構造"
            Dict("Analysis") = "分析・予測"
            Dict("Audio / Video") = "音声 / 映像"
            Dict("Color") = "色空間・カラー"
            Dict("VUI") = "VUI設定"
            Dict("Input / Output") = "入出力"
            Dict("Input/Output") = "入出力"
            Dict("Misc") = "その他"
            Dict("Advanced") = "詳細設定"
            Dict("Override Target File Name") = "出力ファイル名を上書きする"
            Dict("Target File Name") = "出力ファイル名"
            Dict("Quantizer") = "固定量子化値 (QP)"
            Dict("Two Pass") = "2パス"
            Dict("Three Pass") = "3パス"

            '=== 設定（Settings）カテゴリ・項目 ===
            Dict("Logs") = "ログ"
            Dict("Startup") = "スタートアップ"
            Dict("Folders") = "フォルダ"
            Dict("Paths") = "パス設定"
            Dict("Subtitles") = "字幕"
            Dict("Frame Server") = "フレームサーバー"
            Dict("Save projects automatically") = "プロジェクトを自動保存する"
            Dict("In addition save video encoder profiles separately") = "映像エンコーダープロファイルを個別に保存する"
            Dict("In addition save audio profiles separately") = "音声プロファイルを個別に保存する"
            Dict("In addition save events separately") = "イベントを個別に保存する"
            Dict("Reverse mouse wheel video seek direction") = "マウスホイールのシーク方向を反転する"
            Dict("Number of most recently used projects to keep") = "最近使ったプロジェクトの履歴保持数"
            Dict("Maximum number of parallel processes") = "最大並列処理プロセス数"
            Dict("Timeout error messages on job processing") = "ジョブ処理中のエラーメッセージ表示タイムアウト (秒)"
            Dict("Number of log files to keep") = "保持するログファイル数"
            Dict("Write Event Commands to log file") = "イベントコマンドをログファイルに記録する"
            Dict("Enable debug logging") = "デバッグログを有効にする"
            Dict("Startup Template:") = "起動時テンプレート:"
            Dict("Language:") = "言語 (Language):"
            Dict("Language (requires restart):") = "表示言語 (要再起動):"

            '=== ダイアログ ===
            Dict("Jobs") = "ジョブ一覧"
            Dict("Crop") = "クロップ"
            Dict("Audio Configuration") = "音声設定"
            Dict("Muxer Configuration") = "コンテナ・多重化設定"
            Dict("Apps Management") = "外部ツールの管理"
            Dict("Video Comparison") = "動画の比較"
            Dict("Source Files") = "ソースファイル一覧"
            Dict("Add Folder") = "フォルダを追加"
            Dict("Add Files") = "ファイルを追加"

            '=== エンコーダー固有設定・品質・プリセット ===
            Dict("AQ-Mode") = "AQモード (適応量子化)"
            Dict("DV Profile") = "Dolby Vision プロファイル"
            Dict("Range") = "カラーレンジ"
            Dict("Container Options") = "コンテナ設定"
            Dict("Lossless") = "可逆圧縮 (Lossless)"
            Dict("Super High") = "最高品質"
            Dict("Very High") = "非常に高い品質"
            Dict("Higher") = "より高い品質"
            Dict("High") = "高品質"
            Dict("Medium") = "標準"
            Dict("Low") = "低品質"
            Dict("Lower") = "より低い品質"
            Dict("Very Low") = "非常に低い品質"
            Dict("Super Low") = "最低品質"

            Dict("Ultra Fast") = "超高速 (Ultra Fast)"
            Dict("Super Fast") = "最高速 (Super Fast)"
            Dict("Very Fast") = "非常に高速 (Very Fast)"
            Dict("Faster") = "高速 (Faster)"
            Dict("Fast") = "やや高速 (Fast)"
            Dict("Slow") = "やや低速・高圧縮 (Slow)"
            Dict("Slower") = "低速・高品質 (Slower)"
            Dict("Very Slow") = "非常に低速・最高品質 (Very Slow)"
            Dict("Placebo") = "プラセボ (Placebo)"

            Dict("Film") = "映画実写 (Film)"
            Dict("Animation") = "アニメ (Animation)"
            Dict("Grain") = "グレイン維持 (Grain)"
            Dict("Still Image") = "静止画 (Still Image)"
            Dict("Fast Decode") = "高速デコード優先 (Fast Decode)"
            Dict("Zero Latency") = "低遅延・リアルタイム (Zero Latency)"

            Dict("Auto") = "自動"
            Dict("None") = "なし"
            Dict("Disabled") = "無効"
            Dict("Enabled") = "有効"
            Dict("Default") = "デフォルト"

            '=== フィルター ===
            Dict("Source Filter") = "ソースフィルター"
            Dict("Deinterlace") = "インターレース解除"
            Dict("Field") = "フィールド処理"
            Dict("Denoise") = "ノイズ除去"
            Dict("Sharpen") = "鮮鋭化 (シャープ)"
            Dict("Subtitle") = "字幕"

            '=== 通知メッセージ・ヒント ===
            Dict("No subtitles found.") = "字幕が見つかりませんでした。"
            Dict("Command Line was copied.") = "コマンドラインをクリップボードにコピーしました。"
            Dict("Select application display language. Requires restarting StaxRip.") = "アプリケーションの表示言語を選択します。変更にはStaxRipの再起動が必要です。"

            '=== エンコーダー: タブ名・階層カテゴリ ===
            Dict("Slice Decision") = "スライス判定"
            Dict("Motion Search") = "動き探索"
            Dict("VPP") = "VPPフィルター"
            Dict("VPP | Misc") = "VPP | その他"
            Dict("VPP | Misc 2") = "VPP | その他 2"
            Dict("VPP | Misc 3") = "VPP | その他 3"
            Dict("VPP | Misc 4") = "VPP | その他 4"
            Dict("Colorspace") = "色空間"
            Dict("HDR2SDR") = "HDR→SDR変換"
            Dict("Ngx-TrueHDR") = "Ngx-TrueHDR"
            Dict("Deband") = "バンディング低減 (Deband)"
            Dict("LibPlacebo") = "LibPlacebo"
            Dict("Shader") = "シェーダー"
            Dict("Tonemapping") = "トーンマッピング"
            Dict("Tonemapping 2") = "トーンマッピング 2"
            Dict("Resize") = "リサイズ"
            Dict("Sharpness") = "鮮鋭化 (シャープ)"
            Dict("Statistic") = "統計情報"
            Dict("Misc 2") = "その他 2"
            Dict("Misc 3") = "その他 3"
            Dict("Misc 4") = "その他 4"
            Dict("Other") = "その他 (Other)"
            Dict("Pre...") = "事前処理..."
            Dict("Codec Specific") = "コーデック固有設定"
            Dict("GOP size/type") = "GOP構造・サイズ"
            Dict("AV1 Specific 1") = "AV1固有設定 1"
            Dict("AV1 Specific 2") = "AV1固有設定 2"
            Dict("Color Description") = "色情報記述"
            Dict("Variance Boost Options") = "分散ブースト設定"
            Dict("Custom") = "カスタム設定"
            Dict("Rate Control 2") = "レート制御 2"
            Dict("VUI 2") = "VUI設定 2"
            Dict("AFS 2") = "AFS 2"
            Dict("Deinterlace 2") = "インターレース解除 2"
            Dict("Denoise 2") = "ノイズ除去 2"
            Dict("Denoise 3") = "ノイズ除去 3"

            '=== エンコーダー: 詳細パラメータ名 ===
            Dict("Decoder") = "デコーダー"
            Dict("Codec") = "コーデック"
            Dict("Output Depth") = "出力色深度"
            Dict("Max Bitrate") = "最大ビットレート"
            Dict("Target Bitrate") = "目標ビットレート"
            Dict("VBV Buffer Size") = "VBVバッファサイズ"
            Dict("VBV Initial Capacity") = "VBV初期容量"
            Dict("VBR Quality") = "VBR品質"
            Dict("Constrained Quality") = "制限付き品質 (CQ)"
            Dict("QP") = "固定量子化値 (QP)"
            Dict("QP I") = "QP (Iフレーム)"
            Dict("QP P") = "QP (Pフレーム)"
            Dict("QP B") = "QP (Bフレーム)"
            Dict("QP Min") = "最小QP"
            Dict("QP Max") = "最大QP"
            Dict("QP Init") = "初期QP"
            Dict("Show advanced QP settings") = "詳細なQP設定を表示"
            Dict("Adaptive Quantization") = "適応量子化 (AQ)"
            Dict("AQ Strength") = "AQ強度"
            Dict("AQ Mode") = "AQモード"
            Dict("Spatial AQ") = "空間適応量子化 (Spatial AQ)"
            Dict("Temporal AQ") = "時間適応量子化 (Temporal AQ)"
            Dict("Content Adaptive Quantization (CAQ) strength") = "コンテンツ適応量子化 (CAQ) 強度"
            Dict("High motion quality boost mode") = "激しい動き品質ブーストモード"
            Dict("Lookahead") = "先行探索フレーム数 (Lookahead)"
            Dict("Lookahead Level") = "Lookaheadレベル"
            Dict("Film Grain") = "フィルムグレイン (Film Grain)"
            Dict("Film Grain Denoise") = "フィルムグレインノイズ除去"
            Dict("Speed") = "エンコード速度"
            Dict("GOP Length") = "GOP長 (最大フレーム間隔)"
            Dict("Min GOP Length") = "最小GOP長"
            Dict("B-Frames") = "Bフレーム数"
            Dict("B-Frame Reference") = "Bフレーム参照モード"
            Dict("Ref Frames") = "参照フレーム数"
            Dict("Ref Frames L0") = "参照フレーム数 (L0)"
            Dict("Ref Frames L1") = "参照フレーム数 (L1)"
            Dict("Intra Refresh") = "イントラリフレッシュ"
            Dict("Scene Change Detection") = "シーンチェンジ検出"
            Dict("Strict GOP") = "固定GOP長 (Strict GOP)"
            Dict("Open GOP") = "オープンGOP"
            Dict("Keyint") = "キーフレーム間隔 (Keyint)"
            Dict("Min Keyint") = "最小キーフレーム間隔"
            Dict("Slices") = "スライス数"
            Dict("Weight P") = "重み付きPフレーム予測"
            Dict("Weight B") = "重み付きBフレーム予測"
            Dict("Color Primaries") = "原色色度 (Color Primaries)"
            Dict("Color Transfer") = "伝達特性 (Color Transfer)"
            Dict("Color Matrix") = "マトリックス係数 (Color Matrix)"
            Dict("Chromaloc") = "色差サンプル位置 (Chromaloc)"
            Dict("Videoformat") = "映像フォーマット (Videoformat)"
            Dict("Mastering Display") = "マスタリングディスプレイ色度"
            Dict("Max CLL") = "最大コンテンツ輝度 (MaxCLL)"
            Dict("Max FALL") = "最大フレーム平均輝度 (MaxFALL)"
            Dict("Dolby Vision RPU") = "Dolby Vision RPU メタデータ"
            Dict("Dolby Vision Profile") = "Dolby Vision プロファイル"
            Dict("HDR10plus Info") = "HDR10+ メタデータファイル"
            Dict("Dynamic Peak Detection") = "動的ピーク輝度検出"
            Dict("Gamut Mapping") = "広色域マッピング (Gamut Mapping)"
            Dict("Tonemapping Function") = "トーンマッピング関数"
            Dict("Contrast Recovery") = "コントラスト復元"
            Dict("Brightness") = "明るさ (輝度)"
            Dict("Contrast") = "コントラスト"
            Dict("Saturation") = "彩度"
            Dict("Gamma") = "ガンマ"
            Dict("Hue") = "色相"
            Dict("Denoise (Knn)") = "KNN ノイズ除去"
            Dict("Denoise (Pmd)") = "PMD ノイズ除去"
            Dict("Denoise (Smooth)") = "Smooth ノイズ除去"
            Dict("Denoise (FFT3D)") = "FFT3D ノイズ除去"
            Dict("Denoise (NLMeans)") = "NLMeans ノイズ除去"
            Dict("Deband Threshold") = "Deband 閾値"
            Dict("Deband Range") = "Deband 範囲"
            Dict("Deband Dither") = "Deband ディザリング"
            Dict("Edge level") = "輪郭強調 (Edge Level)"
            Dict("Unsharp") = "アンシャープマスク"
            Dict("Warpsharp") = "WarpSharp 輪郭先鋭化"
            Dict("Deinterlacer") = "インターレース解除方式"
            Dict("AFS Preset") = "AFS プリセット"
            Dict("Pad") = "パディング (余白追加)"
            Dict("Rotate") = "回転"
            Dict("Transpose") = "反転・転置"

            '=== エンコーダー: ドロップダウン選択肢 ===
            Dict("QVBR: Constant Quality Mode") = "QVBR: 固定品質モード"
            Dict("CQP: Constant QP") = "CQP: 固定量子化値 (QP)"
            Dict("CBR: Constant Bitrate") = "CBR: 固定ビットレート"
            Dict("VBR: Variable Bitrate") = "VBR: 可変ビットレート"
            Dict("CRF: Constant Rate Factor") = "CRF: 固定レート係数"
            Dict("VBR (Quality): Variable Bitrate") = "VBR (品質優先): 可変ビットレート"
            Dict("ICQ: Intelligent Constant Quality") = "ICQ: インテリジェント固定品質"
            Dict("LA-ICQ: Lookahead Intelligent Constant Quality") = "LA-ICQ: 先行探索インテリジェント固定品質"
            Dict("VCM: Video Conference Mode") = "VCM: ビデオ会議モード"
            Dict("AviSynth/VapourSynth") = "AviSynth/VapourSynth (フレームサーバー)"
            Dict("NVEnc Hardware") = "NVEnc ハードウェアデコード"
            Dict("NVEnc Software") = "NVEnc ソフトウェアデコード"
            Dict("QSVEnc (Intel)") = "QSVEnc (Intel ハードウェア)"
            Dict("ffmpeg (Intel)") = "FFmpeg (Intel QSV)"
            Dict("ffmpeg (DXVA2)") = "FFmpeg (DXVA2 ハードウェア)"
            Dict("8-Bit") = "8ビット (8-Bit)"
            Dict("10-Bit") = "10ビット (10-Bit)"
            Dict("12-Bit") = "12ビット (12-Bit)"
            Dict("Main 10") = "Main 10 (10ビット)"
            Dict("Main 444") = "Main 444 (色差4:4:4)"
            Dict("High 444") = "High 444 (色差4:4:4)"
            Dict("Undefined (Default)") = "未定義 (デフォルト)"
            Dict("Low Latency") = "低遅延 (Low Latency)"
            Dict("Ultra Low Latency") = "超低遅延 (Ultra Low Latency)"
            Dict("P1 (Performance)") = "P1 (最速・低負荷)"
            Dict("P2") = "P2 (より高速)"
            Dict("P3") = "P3 (高速)"
            Dict("P4 (Default)") = "P4 (標準・デフォルト)"
            Dict("P5") = "P5 (やや高品質)"
            Dict("P6") = "P6 (高品質)"
            Dict("P7 (Quality)") = "P7 (最高品質・高負荷)"

            '=== エンコーダー コントロール画面・ボタン ===
            Dict("Target Name Override") = "出力ファイル名を上書き"
            Dict("Override") = "上書き"
            Dict("Super high quality and file size") = "最高品質（ファイルサイズ大）"
            Dict("Very high quality and file size") = "非常に高い品質（ファイルサイズ大）"
            Dict("Higher quality and file size") = "より高い品質"
            Dict("High quality and file size") = "高品質"
            Dict("Medium quality and file size") = "標準品質"
            Dict("Low quality and file size") = "低品質"
            Dict("Lower quality and file size") = "より低い品質"
            Dict("Very low quality and file size") = "非常に低い品質"
            Dict("Super low quality and file size") = "最低品質"
            Dict("Giga Low") = "ギガ低品質"
            Dict("Extreme Low") = "極低品質"
            Dict("Ultra Low") = "超低品質"
            Dict("Giga low quality and file size") = "極めて低い品質"
            Dict("Extreme low quality and file size") = "極端に低い品質"
            Dict("Ultra low quality and file size") = "限界まで低い品質"
            Dict("default") = "デフォルト"

            '=== フィルター: UI・メニュー・リスト ===
            Dict("AVS Filters") = "AviSynth フィルター"
            Dict("VS Filters") = "VapourSynth フィルター"
            Dict("Type") = "種別"
            Dict("Name") = "フィルター名"
            Dict("active") = "アクティブ"
            Dict("Replace") = "置換"
            Dict("Insert") = "挿入"
            Dict("Edit Code...") = "コードを編集..."
            Dict("Preview Code...") = "コードをプレビュー..."
            Dict("Script Code Preview") = "スクリプトコードのプレビュー"
            Dict("Info...") = "スクリプト情報..."
            Dict("Play") = "再生"
            Dict("Profiles...") = "プロファイル..."
            Dict("Move Up") = "上へ移動"
            Dict("Move Down") = "下へ移動"
            Dict("Filter Setup") = "フィルターセットアップ"
            Dict("Removes the selected filter.") = "選択したフィルターを削除します。"
            Dict("Dialog to edit filters.") = "フィルターコードを直接編集します。"
            Dict("Script code preview.") = "スクリプトコードのプレビューを表示します。"
            Dict("Shows script parameters.") = "スクリプトパラメータ情報を表示します。"
            Dict("Plays the script with the AVI player.") = "スクリプトを動画プレイヤーで再生します。"
            Dict("Dialog to edit profiles.") = "フィルタープロファイルを管理・編集します。"
            Dict("Moves the selected item up.") = "選択したフィルターを1つ上に移動します。"
            Dict("Moves the selected item down.") = "選択したフィルターを1つ下に移動します。"

            '=== フィルター: カテゴリ名 ===
            Dict("Frame") = "フレーム処理"
            Dict("Resize") = "リサイズ"
            Dict("HDR to SDR") = "HDR→SDR変換"
            Dict("Tonemap") = "トーンマッピング"

            '=== フィルター: 対話ダイアログ・プロンプト ===
            Dict("Please select one of the options.") = "以下の選択肢から1つ選択してください。"
            Dict("Enable Auto Gain?") = "自動ゲイン (Auto Gain) を有効にしますか？"
            Dict("Enable Auto Balance?") = "自動ホワイトバランス (Auto Balance) を有効にしますか？"
            Dict("Is the Input using TV Range?") = "入力はTVレンジ (Limited 16-235) ですか？"
            Dict("Do you want to use TV Range for Output?") = "出力をTVレンジ (Limited 16-235) にしますか？"
            Dict("Use Dither?") = "ディザリングを使用しますか？"
            Dict("Use High Quality Mode?") = "高品質モードを使用しますか？"
            Dict("Use High Bit Depth Mode?") = "高色深度 (High Bit Depth) モードを使用しますか？"
            Dict("How Many Threads do you wish to use?") = "使用するスレッド数を入力してください:"
            Dict("Enable AutoGain?") = "AutoGain を有効にしますか？"
            Dict("Enable AutoWhite?") = "AutoWhite を有効にしますか？"
            Dict("TV to PC") = "TVレンジ → PCレンジ (拡張)"
            Dict("PC to TV") = "PCレンジ → TVレンジ (圧縮)"
            Dict("To Stack") = "スタック形式へ変換"
            Dict("From Stacked") = "スタック形式から復元"
            Dict("Neutral") = "標準 (ニュートラル)"
            Dict("Brighter") = "明るめ"
            Dict("Less Contrast") = "コントラスト低め"
            Dict("Vivid") = "鮮やか (ビビッド)"
            Dict("Less Color") = "彩度低め"
            Dict("Selective Blue") = "ブルー強調"
            Dict("Selective Yellow Red") = "イエロー/レッド強調"
            Dict("Desaturate") = "モノクロ (彩度ゼロ)"
            Dict("Select the Bit Depth you want to convert to") = "変換先の色深度 (ビット数) を選択してください:"
            Dict("Select the Bit Depth") = "色深度 (ビット数) を選択してください:"
            Dict("Enter the Format you wish to convert to") = "変換先のピクセルフォーマットを入力してください:"
            Dict("Select Input Color Matrix") = "入力カラーマトリックスを選択してください:"
            Dict("Select Input Color Transfer") = "入力伝達特性 (Transfer) を選択してください:"
            Dict("Select Input Color Primaries") = "入力原色色度 (Primaries) を選択してください:"
            Dict("Select Pixel Range") = "ピクセルレンジを選択してください:"
            Dict("Select Output Color Matrix") = "出力カラーマトリックスを選択してください:"
            Dict("Select Output Color Transfer") = "出力伝達特性 (Transfer) を選択してください:"
            Dict("Select Output Color Primaries") = "出力原色色度 (Primaries) を選択してください:"
            Dict("Select Dither Type") = "ディザー形式を選択してください:"
            Dict("Gamma to Linear") = "ガンマ → リニア変換"
            Dict("Linear to Gamma") = "リニア → ガンマ変換"
            Dict("Select the Color Curve") = "カラーカーブを選択してください:"
            Dict("Sigmoid Direct") = "シグモイド変換 (Direct)"
            Dict("Sigmoid Inverse") = "シグモイド逆変換 (Inverse)"
            Dict("RGB to YUV") = "RGB → YUV変換"
            Dict("YUV to RGB") = "YUV → RGB変換"
            Dict("HDR max mastering luminance level (in cd/m2)? (Default: 1000.0)") = "HDR最大マスタリング輝度レベル (cd/m2) を入力してください (デフォルト: 1000.0):"
            Dict("HDR max mastering luminance level (in cd/m2)? (Default: 10000.0)") = "HDR最大マスタリング輝度レベル (cd/m2) を入力してください (デフォルト: 10000.0):"
            Dict("FrameServer.dll was not found in the application directory. Please make sure all StaxRip dependencies are installed properly.") = "アプリケーションディレクトリに FrameServer.dll が見つかりませんでした。StaxRip の依存コンポーネントが正しく配置されているか確認してください。"

            '=== ダイアログボタン・定型アクション ===
            Dict("Yes") = "はい"
            Dict("No") = "いいえ"
            Dict("Cancel") = "キャンセル"
            Dict("Retry") = "再試行"
            Dict("Close") = "閉じる"
            Dict("Abort") = "中止"
            Dict("Ignore") = "無視"
            Dict("Copy Message") = "メッセージをコピー"
            Dict("Message was copied to clipboard.") = "メッセージをクリップボードにコピーしました。"
            Dict("Choose an option") = "選択肢を選択してください"
            Dict("Restore defaults?") = "初期設定に戻しますか？"
            Dict("Restore the default templates?") = "デフォルトのテンプレートに戻しますか？"
            Dict("Defaults were restored.") = "初期設定に戻しました。"
            Dict("Are you sure?") = "本当に実行しますか？"
            Dict("Confirm") = "確認"
            Dict("Select a template") = "テンプレートの選択"
            Dict("Please select a template you want to use:") = "使用するテンプレートを選択してください:"
            Dict("Menu Editor") = "メニューエディター"
            Dict("Restore Defaults...") = "初期設定に戻す..."
            Dict("Import Command Line...") = "コマンドラインをインポート..."

            '=== エラーメッセージ (MsgError / 例外) ===
            Dict("The first filter must be a source filter.") = "最初のフィルターはソースフィルターである必要があります。"
            Dict("Only idx, srt and ass file types are supported.") = "対応している字幕形式は idx, srt, ass のみです。"
            Dict("Only idx, srt, ass and sup file types are supported.") = "対応している字幕形式は idx, srt, ass, sup のみです。"
            Dict("Downloaded file is missing.") = "ダウンロードされたファイルが見つかりません。"
            Dict("File missing after extraction.") = "展開後にファイルが見つかりません。"
            Dict("Source file not found!") = "ソースファイルが見つかりません！"
            Dict("Project file not found!") = "プロジェクトファイルが見つかりません！"
            Dict("Project file not found.") = "プロジェクトファイルが見つかりません。"
            Dict("Unable to play audio.") = "音声を再生できません。"
            Dict("Script Error") = "スクリプトエラー"
            Dict("Something went wrong, the Long Path Support was not enabled!") = "エラーが発生しました。長いパスのサポート (Long Path Support) を有効にできませんでした！"
            Dict("3D demuxing isn't supported.") = "3D デマックスには対応していません。"
            Dict("No chapter file found.") = "チャプターファイルが見つかりませんでした。"
            Dict("Follow assistant message in main dialog.") = "メイン画面のアシスタントのメッセージに従ってください。"
            Dict("The temp folder could not be created.") = "一時フォルダを作成できませんでした。"
            Dict("Only fixed local drives are supported as temp dir.") = "一時フォルダには固定ローカルドライブのみ指定可能です。"
            Dict("Source filter returned invalid parameters") = "ソースフィルターが無効なパラメータを返しました"
            Dict("AviSynth installation not found,{BR}using portable mode instead.") = "AviSynth のインストールが見つかりません。{BR}代わりにポータブル版を使用します。"
            Dict("VapourSynth installation not found,{BR}using portable mode instead.") = "VapourSynth のインストールが見つかりません。{BR}代わりにポータブル版を使用します。"
            Dict("AviSynth installation not found, using portable mode instead.") = "AviSynth のインストールが見つかりません。代わりにポータブル版を使用します。"
            Dict("VapourSynth installation not found, using portable mode instead.") = "VapourSynth のインストールが見つかりません。代わりにポータブル版を使用します。"
            Dict("Custom paths within the startup folder are not permitted") = "スタートアップフォルダ内のカスタムパスは許可されていません"
            Dict("Custom paths within the startup folder are not permitted.") = "スタートアップフォルダ内のカスタムパスは許可されていません。"
            Dict("The auto update feature does currently not support MediaFire.") = "自動更新機能は現在 MediaFire に対応していません。"
            Dict("Folder is already in PATH") = "フォルダはすでに PATH に登録されています"
            Dict("Folder is not in PATH") = "フォルダは PATH に登録されていません"
            Dict("File not found") = "ファイルが見つかりません"
            Dict("An error occured") = "エラーが発生しました"
            Dict("Bug Report") = "バグ報告"
            Dict("Do you want to open the log file?") = "ログファイルを開きますか？"
            Dict("Do you want to report an issue or bug?") = "問題やバグを報告しますか？"
            Dict("Failed to archive log file") = "ログファイルのアーカイブに失敗しました"
            Dict("Failed to load source.") = "ソースの読み込みに失敗しました。"
            Dict("Failed to create a temp directory.") = "一時フォルダの作成に失敗しました。"

            '=== 警告メッセージ (MsgWarn) ===
            Dict("Correct font was not found, using default instead!") = "指定されたフォントが見つからなかったため、デフォルトフォントを使用します！"
            Dict("Functionality is no longer available.") = "この機能は利用できなくなりました。"
            Dict("Filename contains invalid characters.") = "ファイル名に無効な文字が含まれています。"
            Dict("No active filter of category 'Source' found.") = "有効な 'Source' カテゴリのフィルターが見つかりません。"
            Dict("Windows Terminal not found!") = "Windows Terminal が見つかりません！"
            Dict("A template cannot be created after a source file was opened.") = "ソースファイルを開いた後はテンプレートを作成できません。"
            Dict("No playlist directory found.") = "プレイリストフォルダが見つかりませんでした。"
            Dict("Recovery not saved!") = "リカバリプロジェクトを保存できませんでした！"
            Dict("The file VIDEO_TS.VOB can't be opened.") = "VIDEO_TS.VOB ファイルを開くことができません。"
            Dict("Opening files from an optical drive requires to set a temp files folder in the options.") = "光学ドライブからファイルを開くには、オプションで一時ファイルフォルダを設定する必要があります。"
            Dict("Assistant warning cannot be skipped.") = "アシスタントの警告をスキップすることはできません。"
            Dict("Please follow the assistant.") = "アシスタントの指示に従ってください。"
            Dict("Invalid format") = "無効なフォーマットです"
            Dict("Compatibility problem!") = "互換性の問題"
            Dict("Source file is missing!") = "ソースファイルが見つかりません！"
            Dict("Be aware!") = "ご注意ください！"
            Dict("Every not selected, listed or identified file type will be deleted!") = "選択・一覧表示・識別されていないすべてのファイル形式が削除されます！"
            Dict("The input format isn't supported by the current encoder, convert to WAV or FLAC first or enable piping in the options.") = "入力形式は現在のエンコーダーでサポートされていません。事前にWAVまたはFLACに変換するか、オプションでパイピングを有効にしてください。"
            Dict("eac3to output was empty") = "eac3to の出力が空でした"

            '=== 情報メッセージ (MsgInfo) ===
            Dict("All Good!") = "すべて正常です！"
            Dict("No update available.") = "利用可能なアップデートはありません。"
            Dict("Download was canceled or failed.") = "ダウンロードがキャンセルされたか、失敗しました。"
            Dict("Update was canceled.") = "アップデートはキャンセルされました。"
            Dict("Tip") = "ヒント"
            Dict("Job added") = "ジョブを追加しました"
            Dict("Manual Merging") = "手動結合"
            Dict("Download Complete") = "ダウンロードが完了しました"
            Dict("Already Running the Latest Version!") = "すでに最新バージョンを実行しています！"
            Dict("Already Running the Latest Version") = "すでに最新バージョンを実行しています"
            Dict("Please restart StaxRip.") = "StaxRip を再起動してください。"
            Dict("The profile was saved.") = "プロファイルを保存しました。"
            Dict("OK!") = "OK!"
            Dict("All tools have OK status!") = "すべてのツールが正常な状態です！"
            Dict("All required tools have OK status!") = "すべての必須ツールが正常な状態です！"
            Dict("No custom paths defined.") = "カスタムパスは設定されていません。"
            Dict("Nothing found.") = "見つかりませんでした。"
            Dict("The path was copied to the clipboard.") = "パスをクリップボードにコピーしました。"
            Dict("Folder was added to PATH") = "フォルダを PATH に追加しました"
            Dict("Folder was removed from PATH") = "フォルダを PATH から削除しました"

            '=== 質問・確認メッセージ (MsgQuestion) ===
            Dict("Remove Selection?") = "選択項目を削除しますか？"
            Dict("Delete current files?") = "現在のファイルを削除しますか？"
            Dict("Copy new files?") = "新しいファイルをコピーしますか？"
            Dict("Are you sure you want to reset your settings? Your current settings will be lost!") = "設定をリセットしてもよろしいですか？現在の設定は失われます！"
            Dict("Saving outside Template Folder") = "テンプレートフォルダ外への保存"
            Dict("Confirm to process the track.") = "このトラックの処理を実行しますか？"
            Dict("Confirm to process ALL audio tracks.") = "すべての音声トラックの処理を実行しますか？"
            Dict("Include sub folders?") = "サブフォルダも含めますか？"
            Dict("Import command line from clipboard?") = "クリップボードからコマンドラインをインポートしますか？"
            Dict("Remove?") = "削除しますか？"
            Dict("Experimental feature not working for all tools, continue?") = "すべてのツールで動作するとは限らない実験的機能です。続行しますか？"
            Dict("Close while Thumbnailer is running?") = "サムネイル作成中に終了しますか？"
            Dict("This might take a while...") = "少し時間がかかる場合があります..."
            Dict("Thumbnail sheet creation failed!") = "サムネイルシートの作成に失敗しました！"
            Dict("Thumbnail sheet has been created.") = "サムネイルシートが作成されました。"
            Dict("Some thumbnail sheet creation failed!") = "一部のサムネイルシートの作成に失敗しました！"
            Dict("All thumbnail sheet creation failed!") = "すべてのサムネイルシートの作成に失敗しました！"
            Dict("All thumbnail sheets have been created.") = "すべてのサムネイルシートが作成されました。"

            '=== サブメニュー・ダイアログ UI 項目 ===
            Dict("Apps Management") = "外部ツールの管理"
            Dict("Edit Path") = "パスを編集"
            Dict("Find Path") = "パスを検索"
            Dict("Clear Paths") = "パスをクリア"
            Dict("Copy Path") = "パスをコピー"
            Dict("Show Grid") = "グリッド表示"
            Dict("Check All") = "すべての状態を確認"
            Dict("Check Required Only") = "必須ツールのみ確認"
            Dict("Auto Update") = "自動アップデート"
            Dict("Update Request") = "アップデート要求"
            Dict("Explore") = "フォルダを開く"
            Dict("Launch") = "起動"
            Dict("Edit Version") = "バージョンを編集"
            Dict("Edit Changelog") = "更新履歴を編集"
            Dict("Status") = "状態"
            Dict("Version") = "バージョン"
            Dict("Required") = "必須"
            Dict("Path") = "パス"
            Dict("Location") = "場所"
            Dict("Job") = "ジョブ"
            Dict("State") = "状態"
            Dict("Start Time") = "開始時刻"
            Dict("End Time") = "終了時刻"
            Dict("Duration") = "所要時間"
            Dict("Log") = "ログ"
            Dict("Add Audio Track") = "音声トラックを追加"
            Dict("Audio Streams") = "音声ストリーム"
            Dict("Channels") = "チャンネル数"
            Dict("Sampling Rate") = "サンプリングレート"
            Dict("Language") = "言語"
            Dict("Delay") = "遅延 (ms)"
            Dict("Gain") = "ゲイン"
            Dict("Normalize") = "ノーマライズ"
        End Sub

        ''' <summary>
        ''' フォームおよび配置されているコントロール群の表示テキストを辞書に基づいて再帰的にローカライズします。
        ''' </summary>
        Public Sub ApplyLocalization(c As Control)
            If c Is Nothing OrElse CurrentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase) Then Return

            If TypeOf c Is Form Then
                If Not String.IsNullOrEmpty(c.Text) Then
                    c.Text = Translate(c.Text)
                End If
            End If

            ApplyLocalizationInternal(c)
        End Sub

        Private Sub ApplyLocalizationInternal(c As Control)
            If c Is Nothing Then Return

            'ユーザー入力コントロールはテキスト翻訳を除外
            Dim skipText = TypeOf c Is TextBox OrElse
                           TypeOf c Is RichTextBox OrElse
                           TypeOf c Is NumericUpDown OrElse
                           TypeOf c Is ProgressBar

            If Not skipText AndAlso Not String.IsNullOrEmpty(c.Text) Then
                Dim isEditableCombo = False
                If TypeOf c Is ComboBox Then
                    Dim cb = DirectCast(c, ComboBox)
                    If cb.DropDownStyle = ComboBoxStyle.DropDown OrElse cb.DropDownStyle = ComboBoxStyle.Simple Then
                        isEditableCombo = True
                    End If
                End If

                If Not isEditableCombo Then
                    c.Text = Translate(c.Text)
                End If
            End If

            'ListViewの列ヘッダー
            If TypeOf c Is ListView Then
                Dim lv = DirectCast(c, ListView)
                For Each col As ColumnHeader In lv.Columns
                    If Not String.IsNullOrEmpty(col.Text) Then
                        col.Text = Translate(col.Text)
                    End If
                Next
            End If

            'TabControlのTabPage
            If TypeOf c Is TabControl Then
                Dim tc = DirectCast(c, TabControl)
                For Each page As TabPage In tc.TabPages
                    If Not String.IsNullOrEmpty(page.Text) Then
                        page.Text = Translate(page.Text)
                    End If
                Next
            End If

            'ToolStrip / MenuStrip / StatusStrip
            If TypeOf c Is ToolStrip Then
                Dim ts = DirectCast(c, ToolStrip)
                For Each item As ToolStripItem In ts.Items
                    ApplyLocalizationToToolStripItem(item)
                Next
            End If

            'ContextMenuStripが設定されている場合
            If c.ContextMenuStrip IsNot Nothing Then
                For Each item As ToolStripItem In c.ContextMenuStrip.Items
                    ApplyLocalizationToToolStripItem(item)
                Next
            End If

            '子コントロールを再帰走査
            For Each child As Control In c.Controls
                ApplyLocalizationInternal(child)
            Next
        End Sub

        Private Sub ApplyLocalizationToToolStripItem(item As ToolStripItem)
            If item Is Nothing Then Return

            If Not String.IsNullOrEmpty(item.Text) Then
                item.Text = Translate(item.Text)
            End If

            If Not String.IsNullOrEmpty(item.ToolTipText) Then
                item.ToolTipText = Translate(item.ToolTipText)
            End If

            If TypeOf item Is ToolStripDropDownItem Then
                Dim ddi = DirectCast(item, ToolStripDropDownItem)
                For Each subItem As ToolStripItem In ddi.DropDownItems
                    ApplyLocalizationToToolStripItem(subItem)
                Next
            End If
        End Sub
    End Module
End Namespace
