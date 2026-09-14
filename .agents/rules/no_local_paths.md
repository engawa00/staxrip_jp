# ドキュメント内ローカルパス排除ルール

## 基本方針
本プロジェクトで作成・更新するすべてのドキュメント（`task.md`、`implementation_plan.md`、`walkthrough.md`、`handover.md`、アーティファクト、およびその他ドキュメント全般）において、**ユーザー環境固有のローカルフォルダパス（`C:\Users\...`、`file:///...`、ドライブレター `C:\` 等）を一切含めてはならない**。

## 具体的な記載ルール
1. **ファイルリンク・パス参照**:
   - 必ずリポジトリルートからの相対パス（例: `Source/General/Localization.vb`、`[Source/General/Localization.vb](Source/General/Localization.vb)`）を使用すること。
   - `file:///` スキームを用いたローカル絶対パスリンクは厳禁とする。
2. **コマンド・実行環境表記**:
   - 環境固有の絶対パス（例: `C:\Program Files\Microsoft Visual Studio\...`）は記載せず、環境非依存の実行ファイル名や相対パス（例: `MSBuild.exe Source\StaxRip.vbproj ...`）で記載すること。
