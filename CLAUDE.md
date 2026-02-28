# プロジェクトメモリ

## 技術スタック

- 開発環境: Unity 6 (URP)
- 言語: C# 9
- 非同期処理: UniTask（最新）
- DIコンテナ: VContainer（最新）
- リアクティブ: R3 1.3.x
- レンダーパイプライン: URP 17.x

## スペック駆動開発の基本原則

### 基本フロー

1. **ドキュメント作成**: 永続ドキュメント(`docs/`)で「何を作るか」を定義
2. **作業計画**: ステアリングファイル(`.steering/`)で「今回何をするか」を計画
3. **実装**: tasklist.mdに従って実装し、進捗を随時更新
4. **検証**: テストと動作確認
5. **更新**: 必要に応じてドキュメント更新

### 重要なルール

#### ドキュメント作成時

**1ファイルずつ作成し、必ずユーザーの承認を得てから次に進む**

承認待ちの際は、明確に伝える:
```
「[ドキュメント名]の作成が完了しました。内容を確認してください。
承認いただけたら次のドキュメントに進みます。」
```

#### 実装前の確認

新しい実装を始める前に、必ず以下を確認:

1. CLAUDE.mdを読む
2. 関連する永続ドキュメント(`docs/`)を読む
3. Grepで`Assets/Scripts/`の既存の類似実装を検索
4. 既存パターン（命名規則・DI登録方法・UniTask使い方）を理解してから実装開始

#### ステアリングファイル管理

作業ごとに `.steering/[YYYYMMDD]-[タスク名]/` を作成:

- `requirements.md`: 今回の要求内容
- `design.md`: 実装アプローチ（クラス設計・DI登録・非同期設計）
- `tasklist.md`: 具体的なタスクリスト

命名規則: `20250115-add-player-controller` 形式

#### ステアリングファイルの管理

**作業計画・実装・検証時は`steering`スキルを使用してください。**

- **作業計画時**: `Skill('steering')`でモード1(ステアリングファイル作成)
- **実装時**: `Skill('steering')`でモード2(実装とtasklist.md更新管理)
- **検証時**: `Skill('steering')`でモード3(振り返り)

詳細な手順と更新管理のルールはsteeringスキル内に定義されています。

## ディレクトリ構造

### 永続的ドキュメント(`docs/`)

ゲーム全体の「何を作るか」「どう作るか」を定義:

#### 下書き・アイデア（`docs/ideas/`）
- 壁打ち・ブレインストーミングの成果物
- 技術調査メモ・ゲームコンセプトメモ
- 自由形式（構造化は最小限）
- `/setup-project`実行時に自動的に読み込まれる

#### 正式版ドキュメント
- **product-requirements.md** - プロダクト要求定義書（ゲームコンセプト・ターゲット・KPI）
- **functional-design.md** - 機能設計書（ゲームシステム・シーン遷移・UI）
- **architecture.md** - 技術仕様書（レイヤー構成・DI設計・非同期設計）
- **repository-structure.md** - リポジトリ構造定義書（`Assets/`以下の構成・asmdef）
- **development-guidelines.md** - 開発ガイドライン（C#規約・UniTask/R3/VContainer規約・テスト方針）
- **glossary.md** - ユビキタス言語定義（ゲームドメイン用語・システム名・略語）

### アセンブリ構成

```
Assets/Scripts/          → Scripts.asmdef          (プロダクションコード)
Assets/Tests/EditMode/   → Tests.EditMode.asmdef   (EditModeテスト: Domain層・R3)
Assets/Tests/PlayMode/   → Tests.PlayMode.asmdef   (PlayModeテスト: MonoBehaviour・UniTask非同期)
```

### 作業単位のドキュメント(`.steering/`)

特定の開発作業における「今回何をするか」を定義:

- `requirements.md`: 今回の作業の要求内容
- `design.md`: 変更内容の設計（新規クラス・DI登録・Prefab変更）
- `tasklist.md`: タスクリスト

## 開発プロセス

### 初回セットアップ

1. このテンプレートを使用
2. `/setup-project` で永続的ドキュメント作成（対話的に6つ作成）
3. `/add-feature [機能]` で機能実装

### 日常的な使い方

**基本は普通に会話で依頼してください:**

```
# ドキュメントの編集
> PRDに新機能を追加してください
> architecture.mdのDI設計を見直して
> glossary.mdに新しいゲーム用語を追加

# 機能追加（定型フローはコマンド）
> /add-feature ダッシュ機能

# 詳細レビュー（詳細なレポートが必要なとき）
> /review-docs docs/product-requirements.md
```

**ポイント**: スペック駆動開発の詳細を意識する必要はありません。Claude Codeが適切なスキルを判断してロードします。

## コーディングルール

### 非同期処理（UniTask）

- `async UniTask` / `async UniTaskVoid` を使用（`async void` 禁止）
- `CancellationToken` を必ず引数で受け取る
- `OnDestroy` で `_cts.Cancel()` / `_cts.Dispose()` を呼ぶ
- fire-and-forget は `UniTaskVoid` + `.Forget()` を使用

### リアクティブ処理（R3）

- 状態の変化通知には `ReactiveProperty<T>` を使用
- Subscribe の購読解除は `AddTo(this)` または `Dispose()` で管理
- `OnDestroy` 時に `Dispose()` を呼ぶ

### DI（VContainer）

- コンストラクタインジェクション推奨
- MonoBehaviour への注入は `[Inject]` メソッドインジェクションを使用
- ServiceLocator パターン禁止

### テスト方針

| 種別 | 対象 | ツール |
|---|---|---|
| **EditMode** | Domain層（Pure C#）、R3 ReactiveProperty | NUnit |
| **PlayMode** | MonoBehaviour、UniTask非同期、CancellationToken | NUnit + UniTask.ToCoroutine |

- **Domain層**はMonoBehaviour非依存の純粋C#クラスとして実装 → EditModeで検証
- **PlayModeテスト**ではUniTask非同期処理を `UniTask.ToCoroutine()` でラップして検証
- テスト命名規則: `[メソッド名]_[条件]_[期待結果]`

## ドキュメント管理の原則

### 永続的ドキュメント(`docs/`)

- 基本設計を記述
- 頻繁に更新されない
- プロジェクト全体の「北極星」

### 作業単位のドキュメント(`.steering/`)

- 特定の作業に特化
- 作業ごとに新規作成
- 履歴として保持
