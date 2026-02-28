# リポジトリ構造定義書作成ガイド - Unity

## 基本原則

### 1. Unityの規約に従う

Unityはプロジェクト構造に独自の規約があります。`Assets/`・`Packages/`・`ProjectSettings/` はUnityが必要とする固定ディレクトリであり、変更しないでください。

**Unityプロジェクトの固定構造**:
```
project-root/
├── Assets/           ← ゲームアセット・スクリプト（Git管理）
├── Packages/         ← Package Manager設定（Git管理）
├── ProjectSettings/  ← Unity設定（Git管理）
├── Library/          ← 自動生成（Git管理外）
└── Temp/             ← 自動生成（Git管理外）
```

### 2. アセンブリ定義でレイヤーを表現する

Unityでは`src/`のようなディレクトリ分割ではなく、`.asmdef`（アセンブリ定義ファイル）でモジュールを分離します。

**CLAUDE.mdに定義されたアセンブリ構成に必ず準拠する**:
```
Assets/Scripts/Sample/    → Sample.asmdef       (プロダクションコード)
Assets/Tests/             → Tests.EditMode.asmdef (EditModeテスト)
```

### 3. Assets/Scripts/ 内でレイヤーを表現する

`Assets/Scripts/Sample/` 内のフォルダ分けでPresentation / Domain / Infrastructure層を区別します。

**悪い例**:
```
Assets/Scripts/Sample/
├── HpSystem.cs           # Domain？Presentation？不明
├── PlayerController.cs   # 分類不明
└── SaveData.cs           # 分類不明
```

**良い例**:
```
Assets/Scripts/Sample/
├── Domain/               # 純粋C#クラス（UnityEngine非依存）
│   ├── HpSystem.cs
│   └── ScoreManager.cs
├── Presentation/         # MonoBehaviourクラス
│   ├── PlayerController.cs
│   └── HpView.cs
└── Infrastructure/       # データ保存・外部連携
    └── SaveDataRepository.cs
```

---

## ディレクトリ設計の詳細

### Assets/ 内のディレクトリ構成

**スクリプト（.cs）**:
```
Assets/Scripts/
└── Sample/               # Sample.asmdef が配置される
    ├── Domain/           # 純粋C# (UnityEngine非依存)
    ├── Presentation/     # MonoBehaviour (UnityEngine依存)
    └── Infrastructure/   # データ保存
```

**アセット種別ごとの分類**:
```
Assets/
├── Scripts/              # C# スクリプト
├── Scenes/               # Unity シーン（.unity）
├── Prefabs/              # プレハブ（.prefab）
├── Settings/             # URP設定・ScriptableObjectアセット
├── Sprites/              # 画像（スプライト）
├── Audio/                # 音声ファイル
├── Animations/           # アニメーション・Animator Controller
├── Fonts/                # フォント
├── Materials/            # マテリアル
└── Tests/                # EditModeテスト（Tests.EditMode.asmdef）
```

**理由**: Unity Editorはフォルダ名で特殊な処理をするものがある（`Editor/`・`Resources/`・`StreamingAssets/`）ため、意図せず使わないようにする。

---

### ファイル命名規則

#### C# スクリプト

| 種別 | 命名規則 | 例 |
|---|---|---|
| Domain層クラス | PascalCase + 役割なし | `HpSystem.cs`, `ScoreManager.cs` |
| Presentation層（MonoBehaviour） | PascalCase + Controller / View | `PlayerController.cs`, `HpView.cs` |
| Infrastructure層 | PascalCase + Repository / Service | `SaveDataRepository.cs` |
| インターフェース | I + PascalCase | `IHpSystem.cs` |
| ScriptableObject | PascalCase + Settings / Data | `PlayerSettings.cs`, `EnemyData.cs` |
| 列挙型 | PascalCase | `GameState.cs` |

#### テストファイル

| 種別 | 命名規則 | 例 |
|---|---|---|
| EditModeテスト | [対象クラス名]Tests.cs | `HpSystemTests.cs` |

#### Unity アセット

| アセット種別 | 命名規則 | 例 |
|---|---|---|
| シーン (.unity) | PascalCase | `GameScene.unity`, `TitleScene.unity` |
| プレハブ (.prefab) | PascalCase | `PlayerPrefab.prefab`, `EnemyPrefab.prefab` |
| ScriptableObject (.asset) | PascalCase + Settings / Data | `PlayerSettings.asset` |
| アニメーション (.anim) | PascalCase + 動作名 | `PlayerWalk.anim`, `EnemyDeath.anim` |
| スプライト | PascalCase or kebab-case | `PlayerSprite.png`, `player-idle.png` |

---

### アセンブリ定義（.asmdef）の配置ルール

`.asmdef` はディレクトリ直下に1つだけ配置します。そのディレクトリ以下のすべての `.cs` ファイルがそのアセンブリに属します。

```
Assets/Scripts/Sample/
├── Sample.asmdef         ← このディレクトリ以下全部が Sample アセンブリ
├── Domain/
│   └── HpSystem.cs       ← Sample アセンブリに属する
├── Presentation/
│   └── PlayerController.cs ← Sample アセンブリに属する
└── Infrastructure/
    └── SaveDataRepository.cs ← Sample アセンブリに属する

Assets/Tests/
├── Tests.EditMode.asmdef ← このディレクトリ以下全部が Tests.EditMode アセンブリ
└── HpSystemTests.cs      ← Tests.EditMode アセンブリに属する
```

**依存関係**:
```
Tests.EditMode.asmdef → 参照: Sample.asmdef, NUnit
Sample.asmdef → 参照: [使用するパッケージのasmdef]
```

---

### Settings/ の構成

URP設定はCLAUDE.mdに準拠してPC / Mobile向けに分離します。

```
Assets/Settings/
├── UniversalRenderPipelineAsset_PC.asset      # PC向けURP設定
├── UniversalRenderPipelineAsset_Mobile.asset  # Mobile向けURP設定
├── UniversalRendererData.asset                # 共通レンダラーデータ
└── [ゲームパラメータ].asset                   # ScriptableObjectアセット
```

---

### .gitignore の設定

Unityプロジェクト固有の除外設定。

**必須の除外対象**:
```gitignore
# Unityが自動生成するディレクトリ
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Uu]ser[Ss]ettings/

# Visual Studio / Rider
*.csproj
*.sln
*.suo
*.user
*.userprefs
*.pidb
*.booproj

# OS
.DS_Store
Thumbs.db

# テスト結果
*.xml
```

---

## 依存関係のルール

### レイヤー間の依存（コード上）

```
Presentation層（MonoBehaviour）
    ↓ 参照OK
Domain層（純粋C#）
    ↓ 参照OK
Infrastructure層

❌ Domain層 → Presentation層（禁止）
❌ Domain層 → UnityEngine名前空間（禁止）
```

### アセンブリ間の依存（.asmdef上）

```
Tests.EditMode.asmdef
    → Sample.asmdef（参照OK）

Sample.asmdef
    → [外部パッケージ].asmdef（参照OK）
    → Tests.EditMode.asmdef（参照禁止）
```

---

## スケーリング戦略

### ゲームシステムが増えた場合のフォルダ整理

**小規模（初期）**:
```
Assets/Scripts/Sample/
├── Domain/
│   ├── HpSystem.cs
│   └── ScoreManager.cs
├── Presentation/
│   └── PlayerController.cs
└── Infrastructure/
    └── SaveDataRepository.cs
```

**中規模（システムが増えた場合）**:
```
Assets/Scripts/Sample/
├── Domain/
│   ├── Player/          # プレイヤー関連
│   │   ├── HpSystem.cs
│   │   └── MoveSystem.cs
│   └── Enemy/           # 敵関連
│       └── EnemyAI.cs
├── Presentation/
│   ├── Player/
│   └── Enemy/
└── Infrastructure/
```

**分割のタイミング**:
- 同一レイヤーのフォルダ内に10ファイル以上になったとき
- 明確に独立したゲームシステムがまとまったとき

---

## ドキュメントの配置

```
project-root/
├── README.md             # プロジェクト概要・セットアップ手順
├── CLAUDE.md             # Claude Code連携設定
└── docs/
    ├── product-requirements.md  # PRD
    ├── functional-design.md     # 機能設計書
    ├── architecture.md          # アーキテクチャ設計書
    ├── repository-structure.md  # 本ドキュメント
    ├── development-guidelines.md # 開発ガイドライン
    └── glossary.md              # 用語集
```

---

## チェックリスト

- [ ] CLAUDE.mdのアセンブリ構成（Sample / Tests.EditMode）と一致している
- [ ] Assets/Scripts/Sample/ 内がPresentation / Domain / Infrastructureで分類されている
- [ ] テストファイルがAssets/Tests/ 以下に配置されている
- [ ] Assets/Settings/ にPC / Mobile向けURP設定が分離されている
- [ ] .gitignoreにUnity固有の除外設定が含まれている
- [ ] ファイル命名規則が定義されている（C#クラス・テスト・アセット）
- [ ] アセンブリ間の依存関係のルールが明確になっている
- [ ] スケーリング戦略が考慮されている
- [ ] ドキュメントの配置場所が明確になっている
