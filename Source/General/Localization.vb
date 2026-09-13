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
        End Sub
    End Module
End Namespace
