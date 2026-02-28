# 実装ガイド (Implementation Guide) - Unity / C#

## C# 命名規則

### 基本ルール

| 対象 | 形式 | 例 |
|---|---|---|
| クラス・構造体 | PascalCase | `PlayerModel`, `HpSystem` |
| インターフェース | I + PascalCase | `IHpSystem`, `ISaveRepository` |
| メソッド | PascalCase | `TakeDamage()`, `GetCurrentHp()` |
| プロパティ | PascalCase | `CurrentHp`, `MaxHp` |
| publicフィールド | PascalCase | （原則禁止・プロパティを使用） |
| privateフィールド | _camelCase | `_currentHp`, `_moveSpeed` |
| 定数 | PascalCase | `MaxRetryCount`, `DefaultSpeed` |
| 列挙型・値 | PascalCase | `enum GameState { Playing, Paused }` |
| bool変数・プロパティ | Is/Has/Can/Should + PascalCase | `IsDead`, `HasKey`, `CanJump` |

### 良い例・悪い例

```csharp
// ✅ 良い例
public class HpSystem
{
    private int _currentHp;
    private int _maxHp;

    public int CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;

    public void TakeDamage(int damage) { }
    public void Recover(int amount) { }
}

// ❌ 悪い例
public class hpsystem
{
    public int hp;          // publicフィールド直接公開
    public int maxhp;       // camelCase（C#ではNG）

    public void takedamage(int d) { }  // camelCase（C#メソッドはNG）
}
```

---

## MonoBehaviour 設計パターン

### Awake / Start / Update の責務

```csharp
public class PlayerController : MonoBehaviour
{
    // ✅ Awake: 自分自身の初期化（他のオブジェクト参照なし）
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();   // コンポーネントのキャッシュ
        _model = new PlayerModel(maxHp: 100); // Domain層の初期化
    }

    // ✅ Start: 他のオブジェクトへの参照が必要な初期化
    void Start()
    {
        _hpView.UpdateHp(_model.CurrentHp, _model.MaxHp);
    }

    // ✅ Update: 毎フレーム処理（GCアロケーション禁止）
    void Update()
    {
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // ❌ new Vector2 はUpdateで許容（struct はスタック確保なのでGC問題なし）
        // ❌ new List<T>(), new string などヒープ確保はNG
    }
}
```

### SerializeField の使い方

```csharp
// ✅ 良い例: privateフィールドをインスペクターに公開
[SerializeField] private PlayerSettings _settings;
[SerializeField] private HpView _hpView;

// ❌ 悪い例: publicフィールドで公開（外部から変更可能になる）
public PlayerSettings settings;
public HpView hpView;
```

---

## Domain層の設計（UnityEngine非依存）

Domain層はMonoBehaviourを継承せず、UnityEngine名前空間に依存しない純粋C#クラスとして実装します。

```csharp
// ✅ 良い例: Domain層（UnityEngine非依存）
public class HpSystem
{
    public int CurrentHp { get; private set; }
    public int MaxHp { get; }
    public bool IsDead => CurrentHp <= 0;

    public HpSystem(int maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
        CurrentHp = Math.Max(0, CurrentHp - damage);
    }

    public void Recover(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
    }
}

// ❌ 悪い例: Domain層にUnityEngine混入
public class HpSystem : MonoBehaviour  // MonoBehaviour継承 → EditModeテスト不可
{
    public Transform transform;         // UnityEngine依存
    private void Update() { }          // Unity lifecycle → テスト困難
}
```

---

## エラーハンドリング

### 例外の設計方針

```csharp
// ✅ 引数の不正はArgumentExceptionで早期検出
public void TakeDamage(int damage)
{
    if (damage < 0)
        throw new ArgumentOutOfRangeException(nameof(damage), "ダメージ値は0以上である必要があります");

    CurrentHp = Math.Max(0, CurrentHp - damage);
}

// ✅ 状態不正はInvalidOperationException
public void StartBattle()
{
    if (_state != GameState.Idle)
        throw new InvalidOperationException($"バトル開始はIdle状態でのみ可能です。現在: {_state}");

    _state = GameState.Battle;
}
```

### MonoBehaviour層でのエラーハンドリング

```csharp
// ✅ Presentation層（MonoBehaviour）での例外キャッチ
public class PlayerController : MonoBehaviour
{
    public void OnDamageReceived(int damage)
    {
        try
        {
            _model.TakeDamage(damage);
            _hpView.UpdateHp(_model.CurrentHp, _model.MaxHp);
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError($"[PlayerController] 不正なダメージ値: {e.Message}");
        }
    }
}
```

---

## GCアロケーション削減

Unityでは毎フレームのGCアロケーションがパフォーマンス劣化の主原因になります。

### Update内でのnew禁止（参照型のみ）

```csharp
// ❌ 悪い例: UpdateでList/string/classをnew
void Update()
{
    var enemies = new List<Enemy>();  // 毎フレームGCプレッシャー
    var message = $"HP: {_hp}";      // string生成もGC対象
}

// ✅ 良い例: フィールドにキャッシュしてClear再利用
private readonly List<Enemy> _nearbyEnemies = new List<Enemy>();

void Update()
{
    _nearbyEnemies.Clear();
    GetNearbyEnemies(_nearbyEnemies); // リストを再利用
}
```

**注**: `new Vector2()`, `new Vector3()` などのstructは値型のためスタック確保。Updateでも問題なし。

### コンポーネント参照のキャッシュ

```csharp
// ❌ 悪い例: UpdateでGetComponent（毎フレーム検索コスト）
void Update()
{
    GetComponent<Rigidbody2D>().velocity = _velocity;
}

// ✅ 良い例: Awakeでキャッシュ
private Rigidbody2D _rb;

void Awake()
{
    _rb = GetComponent<Rigidbody2D>();
}

void Update()
{
    _rb.velocity = _velocity;
}
```

### オブジェクトプール（弾・エフェクトなど）

```csharp
// ✅ Unity 2021以降: UnityEngine.Pool.ObjectPool を使用
private ObjectPool<Bullet> _bulletPool;

void Awake()
{
    _bulletPool = new ObjectPool<Bullet>(
        createFunc: () => Instantiate(_bulletPrefab),
        actionOnGet: b => b.gameObject.SetActive(true),
        actionOnRelease: b => b.gameObject.SetActive(false),
        defaultCapacity: 20
    );
}

public void FireBullet(Vector2 position)
{
    var bullet = _bulletPool.Get();
    bullet.transform.position = position;
    bullet.Initialize(OnBulletExpired);
}

private void OnBulletExpired(Bullet bullet)
{
    _bulletPool.Release(bullet);
}
```

---

## コメント規約

### XML docコメント（public API・Domain層必須）

```csharp
/// <summary>
/// プレイヤーのHPを管理するクラス。
/// UnityEngine非依存のDomain層クラスとして実装。
/// </summary>
public class HpSystem
{
    /// <summary>現在のHP。0以上MaxHp以下の値を取る。</summary>
    public int CurrentHp { get; private set; }

    /// <summary>
    /// ダメージを受けてHPを減少させる。
    /// </summary>
    /// <param name="damage">受けるダメージ量（0以上）</param>
    /// <exception cref="ArgumentOutOfRangeException">damageが負の場合</exception>
    public void TakeDamage(int damage) { }
}
```

### インラインコメント

```csharp
// ✅ 理由を説明するコメント
// UniTaskのCancellationTokenを使い、シーン遷移時に非同期処理を確実にキャンセル
await LoadSceneAsync(token);

// ✅ 複雑なロジックの説明
// 敵の優先度: プレイヤーとの距離 × 0.7 + 現在のHP割合 × 0.3 で計算
float priority = distance * 0.7f + hpRatio * 0.3f;

// ✅ TODO・FIXME
// TODO: オブジェクトプールを適用する (Issue #45)
// FIXME: 敵が20体以上になるとFPS低下。最適化必要 (Issue #62)

// ❌ コードを読めば分かることを書かない
// HPを減らす
_currentHp -= damage;
```

---

## マジックナンバーの排除

数値パラメータは定数またはScriptableObjectで管理します。

```csharp
// ❌ 悪い例: マジックナンバー
void Update()
{
    if (distanceToPlayer < 5f)
    {
        _speed = 8f;
    }
}

// ✅ 良い例①: 定数で定義
private const float DetectionRange = 5f;
private const float ChaseSpeed = 8f;

// ✅ 良い例②: ScriptableObjectで管理（インスペクターで調整可能）
[SerializeField] private EnemySettings _settings;

void Update()
{
    if (distanceToPlayer < _settings.DetectionRange)
    {
        _speed = _settings.ChaseSpeed;
    }
}
```

---

## 非同期処理（UniTask）

```csharp
// ✅ 良い例: UniTask + CancellationToken
private async UniTaskVoid LoadGameAsync(CancellationToken token)
{
    try
    {
        await LoadResourcesAsync(token);
        await InitializeSystemsAsync(token);
    }
    catch (OperationCanceledException)
    {
        // キャンセルは正常フロー
        Debug.Log("ロードがキャンセルされました");
    }
}

// ✅ CancellationTokenSource の管理
private CancellationTokenSource _cts;

void Awake()
{
    _cts = new CancellationTokenSource();
}

void OnDestroy()
{
    _cts.Cancel();
    _cts.Dispose();
}

// ❌ 禁止: async void（例外がキャッチできない）
private async void BadAsync() { }

// ❌ 禁止: CancellationToken なしの fire-and-forget（Destroy後も動き続ける）
private void BadFireAndForget()
{
    SomeAsync().Forget(); // CancellationToken なし → Destroy後も動く
}
```

---

## リアクティブプログラミング（R3）

R3の `ReactiveProperty<T>` で状態変化を通知します。

```csharp
using R3;

// ✅ 良い例: ReactiveProperty で状態変化を通知
public class PlayerModel : IDisposable
{
    public ReactiveProperty<int> Hp { get; } = new ReactiveProperty<int>(100);

    public void TakeDamage(int damage)
    {
        Hp.Value = Math.Max(0, Hp.Value - damage);
    }

    public void Dispose() => Hp.Dispose();
}

// ✅ 良い例: MonoBehaviour 側での購読と解除
public class HpView : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    private IDisposable _subscription;

    public void Initialize(PlayerModel model, int maxHp)
    {
        _subscription = model.Hp.Subscribe(hp =>
            _hpSlider.value = (float)hp / maxHp);
    }

    private void OnDestroy() => _subscription?.Dispose();
}

// ❌ 悪い例: Dispose を忘れるとメモリリーク
public class BadView : MonoBehaviour
{
    public void Initialize(PlayerModel model)
    {
        model.Hp.Subscribe(hp => Debug.Log(hp)); // Dispose なし → メモリリーク
    }
}
```

---

## テストコード（EditMode / PlayMode / NUnit）

### テスト命名規則

**パターン**: `[メソッド名]_[条件]_[期待結果]`

```csharp
// ✅ 良い例
[Test] public void TakeDamage_ValidDamage_ReducesCurrentHp() { }
[Test] public void TakeDamage_DamageExceedsHp_CurrentHpBecomesZero() { }
[Test] public void TakeDamage_NegativeDamage_ThrowsArgumentOutOfRange() { }

// ❌ 悪い例
[Test] public void Test1() { }
[Test] public void DamageTest() { }
[Test] public void Works() { }
```

### テストの構造（Arrange-Act-Assert）

```csharp
[Test]
public void TakeDamage_ValidDamage_ReducesCurrentHp()
{
    // Arrange: 準備
    var hp = new HpSystem(maxHp: 100);

    // Act: 実行
    hp.TakeDamage(30);

    // Assert: 検証
    Assert.AreEqual(70, hp.CurrentHp);
}

[Test]
public void TakeDamage_DamageExceedsHp_CurrentHpBecomesZero()
{
    // Arrange
    var hp = new HpSystem(maxHp: 100);

    // Act
    hp.TakeDamage(999);

    // Assert
    Assert.AreEqual(0, hp.CurrentHp);
}

[Test]
public void TakeDamage_NegativeDamage_ThrowsArgumentOutOfRange()
{
    // Arrange
    var hp = new HpSystem(maxHp: 100);

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => hp.TakeDamage(-1));
}
```

### 複数ケースのパラメータ化

```csharp
[TestCase(30, 70)]
[TestCase(100, 0)]
[TestCase(0, 100)]
public void TakeDamage_ReturnsExpectedHp(int damage, int expectedHp)
{
    var hp = new HpSystem(maxHp: 100);
    hp.TakeDamage(damage);
    Assert.AreEqual(expectedHp, hp.CurrentHp);
}
```

---

## 実装完了前チェックリスト

### コード品質
- [ ] 命名規則がC# / Unity規約に従っている（PascalCase for methods）
- [ ] privateフィールドが `_camelCase` になっている
- [ ] publicフィールドを使わずプロパティを使用している
- [ ] マジックナンバーがなく定数またはScriptableObjectで管理されている
- [ ] メソッドが単一の責務を持っている

### Unity固有
- [ ] Update内でヒープアロケーション（new class/List/string）をしていない
- [ ] GetComponentがAwakeでキャッシュされている
- [ ] 頻繁に生成・破棄するオブジェクトにObjectPoolを使用している
- [ ] Domain層にUnityEngine名前空間が混入していない

### テスト
- [ ] Domain層のEditModeテストが書かれている
- [ ] MonoBehaviour / UniTask非同期のPlayModeテストが書かれている（該当する場合）
- [ ] テスト命名が `[メソッド]_[条件]_[期待結果]` になっている
- [ ] Arrange-Act-Assertが明確に分かれている
- [ ] 正常系・境界値・異常系がテストされている
- [ ] PlayModeテストのUniTask非同期は `UniTask.ToCoroutine()` でラップされている
- [ ] R3 Subscribe の購読解除が `TearDown` または `OnDestroy` で行われている

### ドキュメント
- [ ] public APIにXML docコメントがある
- [ ] 複雑なロジックにインラインコメントがある（「なぜ」を説明）
- [ ] TODOやFIXMEが記載されている（該当する場合）
