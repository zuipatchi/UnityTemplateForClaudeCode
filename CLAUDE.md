# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

UnityプロジェクトのClaude Code連携用テンプレート。URP (Universal Render Pipeline) ベース。

## テストの実行

**EditMode テストのみ対応。PlayMode テストは未対応。**

Unity Test Runner (Window > General > Test Runner) から EditMode タブで実行する。
CLIで実行する場合:

```bash
Unity -batchmode -runTests -testPlatform EditMode -projectPath . -testResults results.xml
```

アセンブリ構成:
- `Assets/Scripts/Sample/` → `Sample` アセンブリ (プロダクションコード)
- `Assets/Tests/` → `Tests.EditMode` アセンブリ (`Sample` を参照)

## アーキテクチャ

- `Assets/Scripts/Sample/` にプロダクションコードを追加する
- `Assets/Tests/` に対応するEditModeテストを追加する
- URP設定は `Assets/Settings/` に格納 (PC/Mobile 向けに分離)

## コマンド権限 (.claude/settings.json)

**禁止:** `rm`, `sudo`, `curl`, `wget`, `chmod`, `kill`

ファイル削除が必要な場合はユーザーが手動で行う。
