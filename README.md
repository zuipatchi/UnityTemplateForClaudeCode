# TemplateForClaude

Claude Code と連携することを前提としたUnityプロジェクトテンプレートです。

## 動作環境

- Unity 6 (URP)
- Render Pipeline: Universal Render Pipeline (URP) 17.3.0

## 主なパッケージ

| パッケージ | バージョン |
|---|---|
| Universal Render Pipeline | 17.3.0 |
| Input System | 1.18.0 |
| AI Navigation | 2.0.10 |
| Visual Scripting | 1.9.9 |
| Timeline | 1.8.10 |
| Test Framework | 1.6.0 |
| Multiplayer Center | 1.0.1 |

## プロジェクト構成

```
Assets/
├── Scenes/        # シーンファイル
├── Scripts/       # スクリプト (Sample アセンブリ)
├── Settings/      # URP設定など
└── Tests/         # テストコード (EditMode テストのみ対応)
```

## Claude Code 設定

`.claude/settings.json` にてコマンド権限を管理しています。

**許可コマンド:** `git`, `find`, `grep`, `cat`, `ls`, `cp`, `mv`, `mkdir`, `echo`, `Unity`

**禁止コマンド:** `rm`, `sudo`, `curl`, `wget`, `chmod`, `kill`
