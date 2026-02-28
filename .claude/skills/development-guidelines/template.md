# 開発ガイドライン (Development Guidelines) - Unity

## コーディング規約

### 命名規則

#### クラス・インターフェース・構造体

```csharp
// クラス: PascalCase + 名詞
public class HpSystem { }
public class PlayerController : MonoBehaviour { }
public class SaveDataRepository { }

// インターフェース: I + PascalCase
public interface IHpSystem { }
public interface ISaveRepository { }

// ScriptableObject: PascalCase + Settings / Data
[CreateAssetMenu]
public class PlayerSettings : ScriptableObject { }
```

#### メソッド・プロパティ・フィールド

```csharp
public class PlayerModel
{
    // privateフィールド: _camelCase
    private int _currentHp;
    private float _moveSpeed;

    // プロパティ: PascalCase
    public int CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;

    // メソッド: PascalCase + 動詞で始める
    public void TakeDamage(int damage) { }
    public void Recover(int amount) { }
    public int GetTotalScore() { }
}
```

#### 定数・列挙型

```csharp
// 定数: PascalCase（Unityでは慣習上PascalCase）
private const int MaxRetryCount = 3;
private const float DefaultMoveSpeed = 5f;

// 列挙型: PascalCase
public enum GameState
{
    Idle,
    Playing,
    Paused,
    GameOver
}
```

### MonoBehaviour設計方針

```csharp
public class [クラス名] : MonoBehaviour
{
    // インスペクター公開: SerializeField + privateフィールド
    [SerializeField] private [型] _[フィールド名];

    // キャッシュするコンポーネント
    private Rigidbody2D _rb;

    // Domain層への参照
    private [ドメインクラス] _model;

    // Awake: 自分自身の初期化（他オブジェクト参照なし）
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _model = new [ドメインクラス]();
    }

    // Start: 他オブジェクト参照が必要な初期化
    void Start()
    {
        // 初期表示の更新など
    }

    // Update: GCアロケーション禁止（new class/List/stringを書かない）
    void Update()
    {
        // 毎フレーム処理
    }
}
```

### コメント規約

**public APIへのXML docコメント（必須）**:
```csharp
/// <summary>
/// [クラスの説明]
/// </summary>
public class [クラス名]
{
    /// <summary>[プロパティの説明]</summary>
    public int [プロパティ名] { get; private set; }

    /// <summary>
    /// [メソッドの説明]
    /// </summary>
    /// <param name="[引数名]">[引数の説明]（[制約]）</param>
    /// <exception cref="ArgumentOutOfRangeException">[例外が発生する条件]</exception>
    public void [メソッド名]([型] [引数名]) { }
}
```

**インラインコメント（理由を説明）**:
```csharp
// ✅ 良い例: なぜそうするかを説明
// CancellationTokenでシーン遷移時の非同期処理を確実にキャンセル
await LoadAsync(_cts.Token);

// ❌ 悪い例: 何をしているか（コードを読めば分かる）
// awaitでロード
await LoadAsync(_cts.Token);
```

---

## Git運用ルール

### ブランチ戦略

```
main
└── develop
    ├── feature/[機能名]   例: feature/hp-system
    ├── fix/[修正内容]     例: fix/enemy-collision
    └── refactor/[対象]   例: refactor/player-controller
```

### コミットメッセージ規約

**フォーマット**:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Typeの選択肢**:
- `feat`: 新機能
- `fix`: バグ修正
- `test`: EditModeテスト追加・修正
- `refactor`: リファクタリング
- `perf`: パフォーマンス改善
- `assets`: アセット追加・更新
- `docs`: ドキュメント更新
- `chore`: ビルド設定・パッケージ更新

**例**:
```
feat(hp): HpSystemクラスとEditModeテストを追加

[変更内容]

Closes #[Issue番号]
```

### プルリクエストプロセス

**作成前のチェック**:
- [ ] EditModeテストがパスする
- [ ] Unity Editorでの動作確認
- [ ] Domain層にUnityEngine依存がないか確認
- [ ] Update内のGCアロケーションがないか確認

**PRテンプレート**:
```markdown
## 概要
[変更内容の簡潔な説明]

## 変更理由
[なぜこの変更が必要か]

## 変更内容
- [変更点1]
- [変更点2]

## テスト
- [ ] EditModeテスト追加
- [ ] Unity Editorで動作確認

## 関連Issue
Closes #[Issue番号]
```

---

## テスト戦略

### 方針（CLAUDE.md準拠）

**EditModeテストのみ対応。PlayModeテストは対象外。**

### テスト対象

| レイヤー | テスト | 理由 |
|---|---|---|
| Domain層（純粋C#） | ✅ EditModeテスト対象 | UnityEngine非依存 |
| Presentation層（MonoBehaviour） | ❌ 対象外 | PlayMode必要 |
| Infrastructure層 | ⚠️ 限定的に対応 | モック化で対応可能 |

### カバレッジ目標

- **Domain層**: [%]以上

### テスト命名規則

`[メソッド名]_[条件]_[期待結果]`

**例**:
```csharp
[Test] public void TakeDamage_ValidDamage_ReducesCurrentHp() { }
[Test] public void TakeDamage_DamageExceedsHp_CurrentHpBecomesZero() { }
[Test] public void TakeDamage_NegativeDamage_ThrowsArgumentOutOfRange() { }
```

### テスト実行方法

- **Unity Test Runner**: Window > General > Test Runner > EditMode
- **CLI**:
  ```bash
  Unity -batchmode -runTests -testPlatform EditMode -projectPath . -testResults results.xml
  ```

---

## GCアロケーション削減方針

- Update内でヒープアロケーション（new class/List/string）を行わない
- GetComponentはAwakeでキャッシュする
- 頻繁に生成・破棄するオブジェクトにはObjectPoolを使用する

---

## コードレビュー基準

### Unity固有チェック
- [ ] Domain層にUnityEngine名前空間が混入していないか
- [ ] Update内でヒープアロケーションがないか
- [ ] GetComponentがAwakeでキャッシュされているか
- [ ] SerializeFieldを使ってpublicフィールドを避けているか

### コード全般
- [ ] 命名規則が統一されているか（PascalCase for methods）
- [ ] マジックナンバーがないか（定数またはScriptableObject）
- [ ] エラーハンドリングが適切か
- [ ] Domain層のEditModeテストが追加されているか

### レビューコメントの優先度
- `[必須]`: 修正必須（バグ・GC問題・規約違反）
- `[推奨]`: 修正推奨（パフォーマンス・可読性）
- `[提案]`: 検討してほしい
- `[質問]`: 理解のための質問

---

## 開発環境

### 必要なツール

| ツール | バージョン | 用途 |
|---|---|---|
| Unity Hub | 最新版 | Unityバージョン管理 |
| Unity | [バージョン] | ゲームエンジン |
| [IDE] | [バージョン] | コードエディタ |
| Git | [バージョン] | バージョン管理 |

### セットアップ手順

```bash
# 1. リポジトリのクローン
git clone [URL]
cd [project-name]

# 2. Unity Hubでプロジェクトを開く
# 必ずCLAUDE.mdに記載のUnityバージョンを使用すること

# 3. パッケージのインポート
# Unity起動後、Package Managerが自動で解決する
```

### 推奨IDEプラグイン

- [プラグイン1]: [説明]
- [プラグイン2]: [説明]
