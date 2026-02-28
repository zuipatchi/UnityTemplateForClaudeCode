# Unity アーキテクチャ設計ガイド

## 基本原則

### 1. 技術選定には理由を明記

**悪い例**:
```
- UniTask
- DOTween
```

**良い例**:
```
- UniTask 2.x
  - UnityのコルーチンをC# async/awaitで置き換え、GCアロケーションを削減
  - CancellationTokenによるキャンセル処理が直感的に記述できる
  - UniTaskVoidによりMonoBehaviour.Startの非同期化が可能

- DOTween 1.x
  - Transform・UI要素のアニメーションをコードで簡潔に記述できる
  - Sequenceによる複数アニメーションの連結管理が可能
  - オブジェクトプールを内部で使用しGCを抑制している
```

### 2. レイヤー分離の原則

Unityゲームでは以下の3層分離を基本とします:

```
Presentation Layer (MonoBehaviour, UI)
    ↓ 呼び出す
Domain Layer (純粋C#, ゲームロジック)
    ↓ 呼び出す
Infrastructure Layer (データ保存, 外部サービス)
```

**依存の方向は必ず上から下へ。逆方向はNG。**

```csharp
// ✅ OK: Presentation層がDomain層を呼び出す
public class PlayerController : MonoBehaviour
{
    private PlayerModel _player; // Domain層のクラス

    void Update()
    {
        _player.Move(GetInput());
    }
}

// ❌ NG: Domain層にUnityEngineが混入している
public class PlayerModel // Domain層のはずのクラス
{
    public Transform transform; // UnityEngine依存 → テスト不可能
}
```

### 3. Domain層はUnityEngine非依存に保つ

Domain層（ゲームロジック）はMonoBehaviourを継承せず、純粋なC#クラスとして実装します。
これによりEditModeテストが可能になります。

**悪い例**:
```csharp
// Domain層なのにMonoBehaviour継承
public class HpSystem : MonoBehaviour
{
    public int CurrentHp { get; private set; }
}
```

**良い例**:
```csharp
// 純粋C#クラス → EditModeでテスト可能
public class HpSystem
{
    public int CurrentHp { get; private set; }
    public int MaxHp { get; }

    public HpSystem(int maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
    }
}
```

### 4. 測定可能なパフォーマンス要件

すべてのパフォーマンス要件は測定可能な形で記述します。

```
フレームレート: 60fps安定（iPhone 12 / GTX 1060以上）
└─ 測定方法: Unity ProfilerのGPU/CPU使用率を計測
└─ 基準: フレーム時間が16.7ms以内に収まること

メモリ使用量: 512MB以下
└─ 測定方法: Unity Profilerのメモリスナップショット
└─ 基準: 通常プレイ中のGCアロケーションが1フレームあたり0KB

ドローコール: 100以下（シーン最大負荷時）
└─ 測定方法: Frame Debuggerで確認
└─ 対策: SpritePacker / GPU Instancingを使用
```

---

## レイヤードアーキテクチャの設計

### Presentation Layer（MonoBehaviour層）

```csharp
// 責務: 入力受付、Domain層の呼び出し、表示更新
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerView _view;
    private PlayerModel _model;

    void Start()
    {
        _model = new PlayerModel(maxHp: 100);
        _view.UpdateHp(_model.CurrentHp, _model.MaxHp);
    }

    void Update()
    {
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _model.Move(input);
        _view.UpdatePosition(_model.Position);
    }
}
```

### Domain Layer（純粋C#層）

```csharp
// 責務: ゲームロジックの実装（UnityEngine非依存）
public class PlayerModel
{
    public Vector2 Position { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxHp { get; }

    public PlayerModel(int maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public void Move(Vector2 direction)
    {
        Position += direction * Speed;
    }

    public void TakeDamage(int damage)
    {
        CurrentHp = Math.Max(0, CurrentHp - damage);
    }
}
```

### Infrastructure Layer（データ層）

```csharp
// 責務: データの保存・読み込み（UnityのPlayerPrefs / JSONなど）
public class SaveDataRepository
{
    private const string SaveKey = "SaveData";

    public void Save(SaveData data)
    {
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public SaveData Load()
    {
        var json = PlayerPrefs.GetString(SaveKey, "{}");
        return JsonUtility.FromJson<SaveData>(json);
    }
}
```

---

## アセンブリ定義（.asmdef）の設計

CLAUDE.mdに定義されたアセンブリ構成に準拠します:

```
Assets/Scripts/          → Scripts.asmdef          (プロダクションコード)
Assets/Tests/EditMode/   → Tests.EditMode.asmdef   (EditModeテスト)
Assets/Tests/PlayMode/   → Tests.PlayMode.asmdef   (PlayModeテスト)
```

### 依存関係の方向

```
Tests.EditMode.asmdef
    └─ 参照: Scripts.asmdef, UniTask, R3.Unity

Tests.PlayMode.asmdef
    └─ 参照: Scripts.asmdef, UniTask, R3.Unity

Scripts.asmdef
    └─ 参照: UniTask, R3.Unity, VContainer（必要に応じて）
```

### 注意点

- `Scripts.asmdef` にはテストコードを含めない
- `Tests.EditMode.asmdef` は `includePlatforms: ["Editor"]` に設定する
- `Tests.PlayMode.asmdef` は `includePlatforms: []`（全プラットフォーム）に設定する
- 両テストアセンブリに `UNITY_INCLUDE_TESTS` の `defineConstraints` を設定する

---

## ScriptableObjectの活用

ScriptableObjectはゲームパラメータの定義に積極的に活用します。
マジックナンバーをコードから排除し、インスペクターで調整可能にします。

```csharp
// ゲームパラメータをScriptableObjectで定義
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Game/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("HP")]
    public int MaxHp = 100;
    public float HpRecoveryRate = 5f;

    [Header("移動")]
    public float MoveSpeed = 5f;
    public float JumpForce = 10f;
}
```

**活用ガイドライン**:
- 数値パラメータ → ScriptableObject
- ゲームロジック → 純粋C#クラス（Domain層）
- エディタ拡張 → Editorフォルダに分離

---

## URP設定の管理

本プロジェクトではURP設定を `Assets/Settings/` に格納し、PC / Mobile 向けに分離します。

```
Assets/Settings/
├── UniversalRenderPipelineAsset_PC.asset       ← PC向け高品質設定
├── UniversalRenderPipelineAsset_Mobile.asset   ← Mobile向け軽量設定
└── UniversalRendererData.asset                 ← 共通レンダラーデータ
```

**PC向け設定の目安**:
- Shadow Distance: 50〜100m
- MSAA: 4x
- Post Processing: 有効

**Mobile向け設定の目安**:
- Shadow Distance: 20m以下
- MSAA: 無効（または2x）
- Post Processing: 最小限

---

## データ永続化戦略

### ストレージ方式の選択

| データ種別 | 推奨方式 | 理由 |
|---|---|---|
| セーブデータ（小規模） | PlayerPrefs + JSON | 手軽、クロスプラットフォーム対応 |
| セーブデータ（大規模） | Application.persistentDataPath + JSON | ファイルサイズ制限なし |
| ゲーム設定 | ScriptableObject | インスペクターで編集可能 |
| ランタイム設定 | PlayerPrefs | 軽量、プラットフォーム標準 |

### セーブデータの設計例

```csharp
[Serializable]
public class SaveData
{
    public int PlayerLevel;
    public int TotalScore;
    public float PlayTime;
    public string LastSavedAt;
}
```

---

## パフォーマンス設計

### GCアロケーション削減

```csharp
// ❌ NG: 毎フレームnewが発生
void Update()
{
    var list = new List<Enemy>(); // GCプレッシャーの原因
}

// ✅ OK: フィールドに持ちキャッシュする
private readonly List<Enemy> _enemies = new List<Enemy>();

void Update()
{
    _enemies.Clear();
    // _enemiesを再利用
}
```

### オブジェクトプール

頻繁に生成・破棄されるオブジェクト（弾、エフェクト）はオブジェクトプールを使用します:

```csharp
// Unity 2021以降はUnityEngine.Pool.ObjectPoolを使用
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
```

---

## テスト戦略（EditMode）

### テスト対象の基準

| 対象 | テスト可否 | 理由 |
|---|---|---|
| Domain層（純粋C#） | ✅ テスト可能 | UnityEngine非依存 |
| Presentation層（MonoBehaviour） | ❌ 対象外 | PlayModeが必要 |
| Infrastructure層（保存・読み込み） | ⚠️ 限定的 | モック化で対応可能 |

### テストの書き方

```csharp
// Tests.EditMode アセンブリ内
public class HpSystemTests
{
    [Test]
    public void TakeDamage_ReducesCurrentHp()
    {
        var hp = new HpSystem(maxHp: 100);
        hp.TakeDamage(30);
        Assert.AreEqual(70, hp.CurrentHp);
    }

    [Test]
    public void TakeDamage_DoesNotGoBelowZero()
    {
        var hp = new HpSystem(maxHp: 100);
        hp.TakeDamage(999);
        Assert.AreEqual(0, hp.CurrentHp);
    }
}
```

### テスト実行コマンド（CLIの場合）

```bash
Unity -batchmode -runTests -testPlatform EditMode -projectPath . -testResults results.xml
```

---

## チェックリスト

- [ ] すべての技術選定に理由が記載されている
- [ ] Presentation / Domain / Infrastructure のレイヤー分離が明確に定義されている
- [ ] Domain層がUnityEngine非依存（MonoBehaviourなし）になっている
- [ ] アセンブリ定義がCLAUDE.mdの構成（Sample / Tests.EditMode）と一致している
- [ ] URP設定（PC / Mobile分離）が記載されている
- [ ] パフォーマンス要件がフレームレート・メモリ・ドローコールで数値化されている
- [ ] セーブデータの永続化方式が明記されている
- [ ] EditModeテストの対象（Domain層）が明確になっている
- [ ] ScriptableObjectの活用方針が定義されている
- [ ] オブジェクトプールの使用箇所が検討されている
