# プロセスガイド (Process Guide) - Unity

## 基本原則

### 1. 具体例を豊富に含める

抽象的なルールだけでなく、具体的なC#コード例を提示します。

**悪い例**:
```
変数名は分かりやすくすること
```

**良い例**:
```csharp
// ✅ 良い例: 役割が明確
private HpSystem _hpSystem;
private PlayerSettings _settings;

// ❌ 悪い例: 曖昧
private HpSystem h;
private PlayerSettings s;
```

### 2. 理由を説明する

「なぜそうするのか」を明確にします。

**例**:
```
## GCアロケーションをUpdateで行わない

理由: C#のGCは停止型（Stop-the-world）のため、GC発生時にフレームが
一瞬止まりスパイクとして現れます。
MonoBehaviourのUpdateは毎秒60回呼ばれるため、ここでheapアロケーションが
起きると積み重なってGCを頻発させ、フレームレートが不安定になります。
```

### 3. 測定可能な基準を設定

曖昧な表現を避け、具体的な数値を示します。

**悪い例**:
```
パフォーマンスに注意すること
```

**良い例**:
```
パフォーマンス目標:
- フレームレート: 60fps安定（Profilerでフレーム時間16.7ms以内）
- GCアロケーション: 通常プレイ中0KB/フレーム（Profilerで計測）
- ドローコール: シーン最大負荷時100以下（Frame Debuggerで確認）
```

---

## Git運用ルール

### ブランチ戦略（Git Flow採用）

```
main (リリース済み安定版)
└── develop (開発・統合)
    ├── feature/[機能名]   (新機能開発)
    ├── fix/[修正内容]     (バグ修正)
    └── refactor/[対象]   (リファクタリング)
```

**運用ルール**:
- **main**: リリース済みの安定版のみ。直接コミット禁止
- **develop**: 次期リリースに向けた開発コードを統合
- **feature/\*、fix/\***: developから分岐し、PRでdevelopへマージ
- **マージ方針**: feature → develop はsquash merge推奨

### コミットメッセージ規約（Conventional Commits）

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Unityゲーム向けTypeと使い方**:

| Type | 使用場面 |
|---|---|
| `feat` | 新機能（ゲームメカニクス、システム追加） |
| `fix` | バグ修正 |
| `test` | EditModeテスト追加・修正 |
| `refactor` | 動作を変えないリファクタリング |
| `perf` | パフォーマンス改善（GC削減、ドローコール削減など） |
| `assets` | アセット追加・更新（スプライト、サウンドなど） |
| `docs` | ドキュメント更新 |
| `chore` | ビルド設定、パッケージ更新など |

**良いコミットメッセージの例**:

```
feat(hp): HpSystemクラスとEditModeテストを追加

プレイヤーのHP管理をDomain層の純粋C#クラスとして実装。
MonoBehaviour非依存にしてEditModeテストを可能にした。

実装内容:
- HpSystem.cs: TakeDamage, Recover, IsDead プロパティ
- HpSystemTests.cs: 正常系・境界値・異常系テストを追加

Closes #12
```

```
perf(enemy): Update内のListnewをフィールドキャッシュに変更

毎フレームnew List<Enemy>() が発生しGCスパイクの原因になっていたため、
フィールドに保持してClear()で再利用する方式に変更。

ProfilerでGCアロケーション0KB/フレームを確認済み。
```

### プルリクエストテンプレート

```markdown
## 変更の種類
- [ ] 新機能 (feat)
- [ ] バグ修正 (fix)
- [ ] テスト (test)
- [ ] リファクタリング (refactor)
- [ ] パフォーマンス改善 (perf)
- [ ] アセット (assets)
- [ ] その他 (chore/docs)

## 変更内容
### 何を変更したか
[簡潔な説明]

### なぜ変更したか
[背景・理由]

### どのように変更したか
- [変更点1]
- [変更点2]

## テスト
### EditModeテスト
- [ ] 新規テスト追加
- [ ] 既存テストがパス

### 手動確認
- [ ] Unity Editorで動作確認
- [ ] [確認した内容を記述]

## パフォーマンス（該当する場合）
- Profilerで確認: [ ] GCアロケーション 0KB/フレーム
- Frame Debuggerで確認: [ ] ドローコール [件数]

## 関連Issue
Closes #[番号]
```

---

## テスト戦略（EditMode / PlayMode）

### テストの対象

| レイヤー | テスト種別 | 理由 |
|---|---|---|
| Domain層（純粋C#） | ✅ EditMode | UnityEngine非依存のため |
| Presentation層（MonoBehaviour） | ✅ PlayMode | ゲームループ・ライフサイクルが必要 |
| UniTask 非同期処理 | ✅ PlayMode | 実際のフレーム進行が必要 |
| R3 ReactiveProperty | ✅ EditMode | Pure C# で検証可能 |
| Infrastructure層 | ⚠️ 限定的 | モック化で対応可能 |

### テストの方針

**EditMode（Domain層・R3）**:
- ゲームロジック（HP計算、スコア管理、ステートマシンなど）は必ずEditModeテストを書く
- R3の `ReactiveProperty` の購読・発火はEditModeで検証できる
- テストファーストを推奨（実装前にテストケースを書く）

**PlayMode（MonoBehaviour・UniTask）**:
- MonoBehaviourのAwake/Start/OnDestroyの動作はPlayModeで検証する
- UniTask非同期テストは `UniTask.ToCoroutine()` でラップして `[UnityTest]` に変換する
- CancellationTokenのキャンセル動作（GameObject破棄時など）はPlayModeで検証する

**テストカバレッジ目標**:
- Domain層: 80%以上
- Presentation層: 主要なMonoBehaviourのライフサイクルをPlayModeでカバー
- Infrastructure層: 判断可能な範囲でカバー

### テストケース設計

各クラスに対して以下の3種類を書く:

```csharp
// 1. 正常系: 期待通りの動作
[Test]
public void TakeDamage_ValidDamage_ReducesCurrentHp()
{
    var hp = new HpSystem(100);
    hp.TakeDamage(30);
    Assert.AreEqual(70, hp.CurrentHp);
}

// 2. 境界値: 端の値での動作
[Test]
public void TakeDamage_DamageExceedsMaxHp_CurrentHpBecomesZero()
{
    var hp = new HpSystem(100);
    hp.TakeDamage(100);
    Assert.AreEqual(0, hp.CurrentHp);
}

// 3. 異常系: 不正な入力での動作
[Test]
public void TakeDamage_NegativeDamage_ThrowsArgumentOutOfRange()
{
    var hp = new HpSystem(100);
    Assert.Throws<ArgumentOutOfRangeException>(() => hp.TakeDamage(-1));
}
```

### PlayMode テストの書き方（UniTask）

```csharp
// UniTask非同期テストは UniTask.ToCoroutine() でラップする
[UnityTest]
public IEnumerator SomeAsync_条件_期待結果()
    => UniTask.ToCoroutine(async () =>
    {
        await UniTask.Yield(); // MonoBehaviourの初期化を待つ

        // テスト内容
        await _component.SomeAsync();

        Assert.AreEqual(expected, actual);
    });
```

### テスト実行

**Unity Test Runner（推奨）**:
- Window > General > Test Runner
- EditModeタブ: Domain層・R3テスト
- PlayModeタブ: MonoBehaviour・UniTask非同期テスト

**CLI（CI/CD向け）**:
```bash
# EditMode
Unity -batchmode -runTests -testPlatform EditMode -projectPath . -testResults results_edit.xml
# PlayMode
Unity -batchmode -runTests -testPlatform PlayMode -projectPath . -testResults results_play.xml
```

---

## コードレビュープロセス

### レビューの目的

1. **品質保証**: バグの早期発見、GC問題・パフォーマンス問題の検出
2. **Unity規約の統一**: 命名規則・設計パターンの確認
3. **知識共有**: チーム全体でコードベースを理解

### レビューポイント

**Unity固有の確認事項**:
- [ ] Domain層にUnityEngine名前空間が混入していないか
- [ ] Update内でヒープアロケーション（new class / List / string）がないか
- [ ] GetComponentがAwakeでキャッシュされているか
- [ ] SerializeFieldでpublicフィールドが使われていないか

**コード全般**:
- [ ] 命名規則（PascalCase for method, _camelCase for private field）
- [ ] マジックナンバーが定数またはScriptableObjectに移動されているか
- [ ] エラーハンドリングが適切か（ArgumentException, InvalidOperationException）
- [ ] EditModeテストが追加されているか（Domain層の変更時）

### レビューコメントの書き方

**建設的なフィードバック**:
```markdown
## ✅ 良い例
Update内でnew List<Enemy>()が毎フレーム発生しています。
GCスパイクの原因になるため、フィールドに保持してClear()で再利用するか
ObjectPoolを検討してください。

## ❌ 悪い例
この書き方は良くないです。
```

**優先度の明示**:
- `[必須]`: 修正必須（バグ・GC問題・規約違反）
- `[推奨]`: 修正推奨（パフォーマンス・可読性）
- `[提案]`: 検討してほしい（設計改善案）
- `[質問]`: 理解のための質問

**ポジティブなフィードバックも**:
```markdown
✨ Domain層がきれいにUnityEngine非依存で書けています！
👍 テストケースが正常系・境界値・異常系で網羅されていますね
```

### PRのサイズ目安

- 変更ファイル数: 10ファイル以内推奨
- 変更行数: 300行以内推奨
- 1PR = 1機能・1バグ修正

---

## 開発環境セットアップ

### 必要なツール

| ツール | 用途 |
|---|---|
| Unity Hub | Unityバージョン管理 |
| Unity [バージョン] | ゲームエンジン本体 |
| Git | バージョン管理 |
| [IDEの名称] | コードエディタ（Rider / VS 2022など） |

### Unityプロジェクトの初回セットアップ

```bash
# 1. リポジトリのクローン
git clone [URL]
cd [project-name]

# 2. Unity Hubでプロジェクトを開く
# CLAUDE.mdに記載のUnityバージョンを使用すること

# 3. パッケージのインポート
# Unity起動後、Package Managerが自動でパッケージを解決する
```

### Unity .gitignore の確認

Unityプロジェクトでは以下が `.gitignore` に含まれていることを確認:

```
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
*.pidb.meta
*.pdb.meta
*.mdb.meta
```

---

## チェックリスト

- [ ] ブランチ戦略が決まっている
- [ ] コミットメッセージ規約が明確である（Conventional Commits）
- [ ] PRテンプレートが用意されている
- [ ] EditModeテストの対象と命名規則が定義されている
- [ ] テストカバレッジ目標が設定されている（Domain層80%以上）
- [ ] コードレビューのUnity固有チェックポイントが定義されている
- [ ] 開発環境のセットアップ手順が記録されている
