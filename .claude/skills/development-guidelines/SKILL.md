---
name: development-guidelines
description: Unityゲーム開発における統一されたコーディング規約と開発プロセスを確立するための包括的なガイドとテンプレート。開発ガイドライン作成時、コード実装時に使用する。
allowed-tools: Read, Write, Edit
---

# Unity 開発ガイドラインスキル

Unityゲーム開発に必要な2つの要素をカバーします:
1. 実装時のコーディング規約 (guides/implementation.md)
2. 開発プロセスの標準化 (guides/process.md)

## 前提条件

開発ガイドライン作成を開始する前に、以下を確認してください:

### 推奨ドキュメント

1. `docs/architecture.md` (アーキテクチャ設計書) - レイヤー構成とアセンブリ構成の確認
2. `docs/functional-design.md` (機能設計書) - 機能一覧の確認
3. `CLAUDE.md` - プロジェクト固有の制約（禁止コマンド、テスト方針）の確認

開発ガイドラインは、Unityプロジェクトの技術スタックとアセンブリ構成に
基づいた具体的なC#コーディング規約と開発プロセスを定義します。

## 既存ドキュメントの優先順位

**重要**: `docs/development-guidelines.md` に既存の開発ガイドラインがある場合、
以下の優先順位に従ってください:

1. **既存の開発ガイドライン (`docs/development-guidelines.md`)** - 最優先
   - プロジェクト固有の規約とプロセスが記載されている
   - このスキルのガイドより優先する

2. **このスキルのガイド** - 参考資料
   - ./guides/implementation.md: Unity/C#コーディング規約
   - ./guides/process.md: Git運用・テスト・レビュープロセス
   - 既存ガイドラインがない場合、または補足として使用

**新規作成時**: このスキルのガイドとテンプレートを参照
**更新時**: 既存ガイドラインの構造と内容を維持しながら更新

## 出力先

作成した開発ガイドラインは以下に保存してください:

```
docs/development-guidelines.md
```

## クイックリファレンス

### コード実装時
Unity/C#のコーディング規約: ./guides/implementation.md

含まれる内容:
- C#命名規則（クラス・メソッド・フィールド・定数）
- MonoBehaviour設計パターン（Awake/Start/Update の使い分け）
- Domain層の設計（UnityEngine非依存）
- エラーハンドリングと例外設計
- GCアロケーション削減テクニック
- NUnitテストコード実装（EditModeテスト）
- コメント規約（XML doc）

### 開発プロセスの参照／策定時
Git運用・テスト戦略・コードレビュー: ./guides/process.md

含まれる内容:
- 基本原則（具体例の重要性、理由説明）
- Git運用ルール（Git Flowブランチ戦略）
- コミットメッセージとPRプロセス
- Unityテスト戦略（EditModeテストのみ、CLAUDE.md準拠）
- コードレビュープロセス

### テンプレート
開発ガイドライン作成時: ./template.md

## 使用シーン別ガイド

### 新規開発時
1. `CLAUDE.md` でプロジェクト制約（禁止コマンド、テスト方針）を確認
2. `docs/architecture.md` でレイヤー構成・アセンブリ定義を確認
3. ./guides/implementation.md でC#命名規則・コーディング規約を確認
4. ./guides/process.md でブランチ戦略・PR処理を確認
5. Domain層の実装 → EditModeテストを書く（テストファースト推奨）

### コードレビュー時
- ./guides/process.md の「コードレビュープロセス」を参照
- ./guides/implementation.md でUnity固有の規約違反がないか確認
  - MonoBehaviourのUpdate内でのnew使用（GC問題）
  - Domain層へのUnityEngine名前空間の混入

### テスト設計時
- ./guides/process.md の「テスト戦略」（EditModeテストの対象と方針）
- ./guides/implementation.md の「テストコード」（NUnitによる実装パターン）
- CLAUDE.md: EditModeテストのみ対応、PlayModeテストは対象外

### リリース準備時
- ./guides/process.md の「Git運用ルール」（main へのマージ方針）
- コミットメッセージがConventional Commitsに従っているか確認

## チェックリスト

- [ ] C#命名規則が具体例付きで定義されている
- [ ] MonoBehaviourの設計方針が明確である（Awake/Start/Update の責務）
- [ ] Domain層がUnityEngine非依存になっているか確認する方針がある
- [ ] エラーハンドリングの方針が定義されている
- [ ] GCアロケーション削減の方針が定義されている
- [ ] ブランチ戦略が決まっている
- [ ] コミットメッセージ規約が明確である
- [ ] PRテンプレートが用意されている
- [ ] EditModeテストの対象と命名規則が定義されている（CLAUDE.md準拠）
- [ ] コードレビュープロセスが定義されている
