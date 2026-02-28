# リポジトリ構造定義書 (Repository Structure Document) - Unity

## プロジェクト構造

```
project-root/
├── Assets/                    # ゲームアセット・スクリプト（Git管理）
│   ├── Scripts/
│   │   └── Sample/            # プロダクションコード (Sample.asmdef)
│   │       ├── Domain/        # 純粋C# (UnityEngine非依存)
│   │       ├── Presentation/  # MonoBehaviour
│   │       └── Infrastructure/ # データ保存
│   ├── Tests/                 # EditModeテスト (Tests.EditMode.asmdef)
│   ├── Scenes/                # Unity シーン
│   ├── Prefabs/               # プレハブ
│   ├── Settings/              # URP設定・ScriptableObjectアセット
│   ├── Sprites/               # 画像（スプライト）
│   ├── Audio/                 # BGM・SE
│   ├── Animations/            # アニメーション
│   └── [その他アセット]/
├── Packages/                  # Package Manager設定（Git管理）
│   └── manifest.json
├── ProjectSettings/           # Unity設定（Git管理）
├── docs/                      # プロジェクトドキュメント
├── CLAUDE.md                  # Claude Code設定
└── README.md
```

**Git管理外（.gitignoreで除外）**:
```
Library/     # Unity自動生成
Temp/        # Unity自動生成
obj/         # ビルド一時ファイル
Build/       # ビルド成果物
UserSettings/ # ユーザー固有設定
```

---

## Assets/ 内のディレクトリ詳細

### Assets/Scripts/Sample/ （プロダクションコード）

**アセンブリ**: `Sample.asmdef`
**役割**: ゲームのすべてのプロダクションコード

```
Assets/Scripts/Sample/
├── Sample.asmdef              # アセンブリ定義
├── Domain/                    # 純粋C#クラス（UnityEngine非依存）
│   ├── [システム名].cs         # ゲームロジック
│   └── [モデル名].cs          # データモデル
├── Presentation/              # MonoBehaviourクラス
│   ├── [コントローラー名].cs
│   └── [View名].cs
└── Infrastructure/            # データ保存・外部連携
    └── [Repository名].cs
```

**配置ルール**:
| ファイル種別 | 配置先 | 例 |
|---|---|---|
| ゲームロジック（純粋C#） | Domain/ | `HpSystem.cs`, `ScoreManager.cs` |
| MonoBehaviour（入力・表示） | Presentation/ | `PlayerController.cs`, `HpView.cs` |
| データ保存・読み込み | Infrastructure/ | `SaveDataRepository.cs` |
| 列挙型・定数 | Domain/ または ルート直下 | `GameState.cs` |
| インターフェース | Domain/ または 各層 | `IHpSystem.cs` |

**依存関係**:
```
Presentation/ → Domain/ → Infrastructure/

❌ Domain/ → Presentation/（禁止）
❌ Domain/ → UnityEngine名前空間（禁止）
```

---

### Assets/Tests/ （EditModeテスト）

**アセンブリ**: `Tests.EditMode.asmdef`（`Sample.asmdef` を参照）
**役割**: Domain層の EditMode テスト（CLAUDE.md準拠）

```
Assets/Tests/
├── Tests.EditMode.asmdef      # アセンブリ定義
└── [対象クラス名]Tests.cs     # テストクラス
```

**命名規則**: `[対象クラス名]Tests.cs`（例: `HpSystemTests.cs`）

**PlayModeテストは対象外**（CLAUDE.md参照）

---

### Assets/Scenes/ （シーン）

```
Assets/Scenes/
├── TitleScene.unity           # タイトル画面
├── GameScene.unity            # ゲームプレイ画面
└── [その他シーン].unity
```

**命名規則**: `[シーン名]Scene.unity`（PascalCase）

---

### Assets/Prefabs/ （プレハブ）

```
Assets/Prefabs/
├── Player/
│   └── PlayerPrefab.prefab
├── Enemy/
│   └── EnemyPrefab.prefab
└── UI/
    └── [UI要素]Prefab.prefab
```

**命名規則**: `[名称]Prefab.prefab`（PascalCase）

---

### Assets/Settings/ （設定ファイル）

URP設定はPC / Mobile向けに分離します（CLAUDE.md準拠）。

```
Assets/Settings/
├── UniversalRenderPipelineAsset_PC.asset      # PC向けURP設定
├── UniversalRenderPipelineAsset_Mobile.asset  # Mobile向けURP設定
├── UniversalRendererData.asset                # 共通レンダラーデータ
└── [パラメータ名]Settings.asset               # ScriptableObjectアセット
```

---

### Assets/[その他アセット]

| ディレクトリ | 用途 | 命名規則 |
|---|---|---|
| `Audio/` | BGM・SE音声ファイル | PascalCase または kebab-case |
| `Sprites/` | スプライト・UI画像 | PascalCase または kebab-case |
| `Animations/` | アニメーション・Animator Controller | `[対象名][動作].anim` |
| `Materials/` | マテリアル | `[名称]Mat.mat` |
| `Fonts/` | フォントファイル | そのまま |

---

## ファイル配置規則

### C# スクリプト

| ファイル種別 | 配置先 | 命名規則 | 例 |
|---|---|---|---|
| Domain層クラス | `Assets/Scripts/Sample/Domain/` | `PascalCase.cs` | `HpSystem.cs` |
| Presentation層クラス | `Assets/Scripts/Sample/Presentation/` | `PascalCase + Controller/View.cs` | `PlayerController.cs` |
| Infrastructure層クラス | `Assets/Scripts/Sample/Infrastructure/` | `PascalCase + Repository.cs` | `SaveDataRepository.cs` |
| ScriptableObject定義 | `Assets/Scripts/Sample/` または `Domain/` | `PascalCase + Settings/Data.cs` | `PlayerSettings.cs` |
| EditModeテスト | `Assets/Tests/` | `[対象]Tests.cs` | `HpSystemTests.cs` |

### Unity アセット

| アセット種別 | 配置先 | 命名規則 | 例 |
|---|---|---|---|
| シーン | `Assets/Scenes/` | `[名称]Scene.unity` | `GameScene.unity` |
| プレハブ | `Assets/Prefabs/[カテゴリ]/` | `[名称]Prefab.prefab` | `PlayerPrefab.prefab` |
| ScriptableObject | `Assets/Settings/` | `[名称]Settings.asset` | `PlayerSettings.asset` |
| アニメーション | `Assets/Animations/` | `[対象][動作].anim` | `PlayerWalk.anim` |

---

## アセンブリ定義（.asmdef）

### 依存関係

```
Tests.EditMode.asmdef
    └─ 参照: Sample.asmdef, NUnit

Sample.asmdef
    └─ 参照: [使用する外部パッケージのasmdef]
    └─ 参照禁止: Tests.EditMode.asmdef
```

### .asmdef ファイルの配置ルール

- `.asmdef` は各アセンブリのルートディレクトリ直下に1つだけ配置する
- そのディレクトリ以下のすべての `.cs` が同一アセンブリに属する
- `Tests.EditMode.asmdef` は「Editor」プラットフォームのみ有効に設定する

---

## 依存関係のルール

### レイヤー間の依存（コード）

```
Presentation層（MonoBehaviour）
    ↓ OK
Domain層（純粋C#）
    ↓ OK
Infrastructure層

禁止:
❌ Domain → Presentation
❌ Domain → UnityEngine名前空間
❌ Sample.asmdef → Tests.EditMode.asmdef
```

---

## スケーリング方針

### ゲームシステムが増えた場合

| ファイル数 | 対応 |
|---|---|
| 1〜9ファイル | フラットに配置（サブフォルダ不要） |
| 10ファイル以上 | システム単位でサブフォルダに分割 |

**分割例（Domain/ が増えた場合）**:
```
Domain/
├── Player/
│   ├── HpSystem.cs
│   └── MoveSystem.cs
├── Enemy/
│   └── EnemyAI.cs
└── Core/
    └── ScoreManager.cs
```

---

## 除外設定（.gitignore）

```gitignore
# Unity 自動生成
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
*.pidb
*.booproj

# OS
.DS_Store
Thumbs.db

# テスト結果
results.xml
```

---

## ドキュメント配置

```
project-root/
├── README.md                        # プロジェクト概要・セットアップ手順
├── CLAUDE.md                        # Claude Code設定（アセンブリ構成・禁止コマンド）
└── docs/
    ├── product-requirements.md      # PRD
    ├── functional-design.md         # 機能設計書
    ├── architecture.md              # アーキテクチャ設計書
    ├── repository-structure.md      # 本ドキュメント
    ├── development-guidelines.md    # 開発ガイドライン
    └── glossary.md                  # 用語集
```
