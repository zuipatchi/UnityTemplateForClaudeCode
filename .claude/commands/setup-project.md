---
description: 初回セットアップ: 6つの永続ドキュメントを対話的に作成する
---

# 初回プロジェクトセットアップ (Unity)

このコマンドは、Unityゲームプロジェクトの6つの永続ドキュメントを対話的に作成します。

## 実行方法

```bash
claude
> /setup-project
```

## 実行前の確認

`docs/ideas/` ディレクトリ内のファイルを確認します。

- ファイルが存在する場合: その内容を元にPRDを作成する
- ファイルが存在しない場合: 対話形式でPRDを作成する

## 手順

### ステップ0: インプットの読み込み

1. `docs/ideas/` 内のマークダウンファイルを全て読む
2. 内容を理解し、PRD作成の参考にする
3. ファイルがなければスキップして対話形式に切り替える

### ステップ1: プロダクト要求定義書の作成

1. **prd-writingスキル**をロード (`Skill` tool で `prd-writing` を呼び出す)
2. `docs/ideas/` の内容または対話を元に `docs/product-requirements.md` を作成
3. Unity特有の観点を含める:
   - ターゲットプラットフォーム (PC / Mobile / Console など)
   - Unityバージョン・レンダーパイプライン (URP / HDRP / Built-in)
   - 想定プレイヤー数・ジャンル
   - 主要なゲームループとコアメカニクス
4. ユーザーに確認を求め、**承認されるまで待機**

**以降のステップはプロダクト要求定義書の内容を元にするため、自動的に作成する**

### ステップ2: 機能設計書の作成

1. **functional-designスキル**をロード (`Skill` tool で `functional-design` を呼び出す)
2. `docs/product-requirements.md` を読む
3. スキルのテンプレートとガイドに従って `docs/functional-design.md` を作成
4. Unity特有の観点を含める:
   - ゲームシステム一覧 (戦闘・UI・セーブ・サウンドなど)
   - シーン遷移フロー
   - 主要なMonoBehaviour / ScriptableObjectの役割

### ステップ3: アーキテクチャ設計書の作成

1. **architecture-designスキル**をロード (`Skill` tool で `architecture-design` を呼び出す)
2. 既存のドキュメントを読む
3. スキルのテンプレートとガイドに従って `docs/architecture.md` を作成
4. Unity特有の観点を含める:
   - アーキテクチャパターン (MVC / MVP / ECS など)
   - DI方式 (VContainer / Zenject / 手動DIなど)
   - 非同期処理方針 (UniTask / Coroutineなど)
   - ScriptableObjectを使ったデータ設計

### ステップ4: リポジトリ構造定義書の作成

1. **repository-structureスキル**をロード (`Skill` tool で `repository-structure` を呼び出す)
2. 既存のドキュメントを読む
3. スキルのテンプレートに従って `docs/repository-structure.md` を作成
4. Unity特有のフォルダ構成を含める:
   - `Assets/` 以下のディレクトリ構成
   - `Scripts/` の名前空間とフォルダの対応
   - `Resources/` / `AddressableAssets/` の使い方
   - エディタ拡張・テストコードの配置

### ステップ5: 開発ガイドラインの作成

1. **development-guidelinesスキル**をロード (`Skill` tool で `development-guidelines` を呼び出す)
2. 既存のドキュメントを読む
3. スキルのテンプレートに従って `docs/development-guidelines.md` を作成
4. Unity特有の規約を含める:
   - C#コーディング規約 (命名・namespace・partial classなど)
   - MonoBehaviour利用ルール (Awake / Start / OnDestroy の使い分け)
   - UniTask/非同期処理規約
   - テスト方針 (EditMode / PlayMode)
   - Gitコミット・ブランチ戦略

### ステップ6: 用語集の作成

1. **glossary-creationスキル**をロード (`Skill` tool で `glossary-creation` を呼び出す)
2. 既存のドキュメントを読む
3. スキルのテンプレートに従って `docs/glossary.md` を作成
4. Unity・ゲーム固有の用語を含める:
   - ゲームドメイン用語 (HP・スコア・エネミーなどゲーム固有の概念)
   - Unityコンポーネント・Packageの略語
   - プロジェクト固有のシステム名・略称

## 完了条件

- 6つの永続ドキュメントが全て作成されていること

完了時のメッセージ:
```
初回セットアップが完了しました!

作成したドキュメント:
✅ docs/product-requirements.md
✅ docs/functional-design.md
✅ docs/architecture.md
✅ docs/repository-structure.md
✅ docs/development-guidelines.md
✅ docs/glossary.md

これで開発を開始する準備が整いました。

今後の使い方:
- ドキュメントの編集: 普通に会話で依頼してください
  例: 「PRDに新機能を追加して」「architecture.mdを見直して」

- 機能の追加: /add-feature [機能名] を実行してください
  例: /add-feature ユーザー認証

- ドキュメントレビュー: /review-docs [パス] を実行してください
  例: /review-docs docs/product-requirements.md
```
