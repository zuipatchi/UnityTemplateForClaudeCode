# 技術仕様書 (Architecture Design Document)

## テクノロジースタック

### Unity 基本構成

| 技術 | バージョン | 選定理由 |
|---|---|---|
| Unity | [バージョン] | [理由] |
| Render Pipeline | URP | 軽量・クロスプラットフォーム対応、`Assets/Settings/`にPC/Mobile向け設定を分離 |
| C# | .NET Standard 2.1 | Unityデフォルト、async/await対応 |

### 主要パッケージ（Unity Package Manager）

| パッケージ | バージョン | 用途 | 選定理由 |
|---|---|---|---|
| [名称] | [バージョン] | [用途] | [理由] |
| [名称] | [バージョン] | [用途] | [理由] |

### 外部ライブラリ

| ライブラリ | バージョン | 用途 | 選定理由 |
|---|---|---|---|
| [名称] | [バージョン] | [用途] | [理由] |

---

## アーキテクチャパターン

### レイヤードアーキテクチャ

```
┌──────────────────────────────────────┐
│  Presentation Layer (MonoBehaviour)  │ ← 入力受付・表示更新・シーン管理
├──────────────────────────────────────┤
│  Domain Layer (純粋C#)               │ ← ゲームロジック（UnityEngine非依存）
├──────────────────────────────────────┤
│  Infrastructure Layer                │ ← データ保存・外部サービス
└──────────────────────────────────────┘
```

#### Presentation Layer
- **責務**: プレイヤー入力の受付、Domain層の呼び出し、表示の更新、シーン遷移
- **実装**: MonoBehaviour継承クラス、UI（uGUI / UI Toolkit）
- **許可される操作**: Domain層・Infrastructure層の呼び出し
- **禁止される操作**: ゲームロジックの直接実装

#### Domain Layer
- **責務**: ゲームロジックの実装（HP計算、スコア管理、ステート管理など）
- **実装**: 純粋C#クラス（MonoBehaviour継承なし、UnityEngine名前空間の使用禁止）
- **許可される操作**: Infrastructure層の呼び出し
- **禁止される操作**: UnityEngine依存のコード（MonoBehaviour, Transform, Mathfなど）

#### Infrastructure Layer
- **責務**: データの永続化（セーブ・ロード）、外部サービスとの連携
- **実装**: PlayerPrefs、JSON（Application.persistentDataPath）、ScriptableObject
- **禁止される操作**: ゲームロジックの実装

---

## アセンブリ定義（.asmdef）

CLAUDE.mdに定義されたアセンブリ構成に従います:

```
Assets/Scripts/Sample/     → Sample.asmdef         (プロダクションコード)
Assets/Tests/              → Tests.EditMode.asmdef  (EditModeテスト)
```

### 依存関係

```
Tests.EditMode.asmdef
    └─ 参照: Sample.asmdef, NUnit

Sample.asmdef
    └─ 参照: [使用するパッケージのasmdef]
```

### アセンブリ設計方針
- Presentation / Domain / Infrastructure はすべて `Sample.asmdef` 内に配置（規模に応じて分割を検討）
- テストコードは `Tests.EditMode.asmdef` に分離
- PlayModeテストは対象外（CLAUDE.md参照）

---

## URP設定

```
Assets/Settings/
├── UniversalRenderPipelineAsset_PC.asset      ← PC向け設定
├── UniversalRenderPipelineAsset_Mobile.asset  ← Mobile向け設定
└── UniversalRendererData.asset                ← 共通レンダラーデータ
```

| 設定項目 | PC | Mobile |
|---|---|---|
| Shadow Distance | [m] | [m] |
| MSAA | [4x / 2x / Off] | [Off / 2x] |
| Post Processing | [有効/無効] | [有効/無効] |
| HDR | [有効/無効] | [有効/無効] |

---

## データ永続化戦略

### ストレージ方式

| データ種別 | 保存方式 | 保存先 | 理由 |
|---|---|---|---|
| セーブデータ | [PlayerPrefs / JSON] | [保存先] | [理由] |
| ゲーム設定 | [ScriptableObject / PlayerPrefs] | [保存先] | [理由] |
| [データ名] | [方式] | [保存先] | [理由] |

### セーブデータ構造

```csharp
[Serializable]
public class SaveData
{
    // [フィールド定義]
}
```

### バックアップ・復元方針
- **バックアップ**: [方式・タイミング]
- **復元**: [手順]
- **データ損失対策**: [自動セーブのタイミングなど]

---

## ScriptableObject 活用方針

| ScriptableObject名 | 管理する設定 | 保存場所 |
|---|---|---|
| [名称]Settings | [設定内容] | Assets/Settings/ |
| [名称]Settings | [設定内容] | Assets/Settings/ |

**方針**: マジックナンバーを排除し、すべてのゲームパラメータをScriptableObjectで管理する

---

## パフォーマンス要件

### フレームレート目標

| プラットフォーム | 目標fps | 測定環境 |
|---|---|---|
| PC | 60fps | [CPU/GPU スペック] |
| iOS | 60fps | [最小動作モデル] |
| Android | 60fps | [最小動作スペック] |

### リソース使用量

| リソース | 上限 | 測定方法 |
|---|---|---|
| メモリ | [MB] | Unity Profilerスナップショット |
| ドローコール | [件] | Frame Debugger |
| GCアロケーション（毎フレーム） | 0KB | Unity Profiler |
| 初回起動ロード時間 | [秒] | 実機計測 |

### パフォーマンス対策

- **GC削減**: [キャッシュ戦略・ListのClear再利用など]
- **オブジェクトプール使用箇所**: [弾・エフェクト・UIなど]
- **バッチング**: [Static Batching / GPU Instancing の使用方針]

---

## テスト戦略

### EditModeテスト（対象）

- **フレームワーク**: NUnit (Unity Test Framework)
- **対象**: Domain層の純粋C#クラス
- **カバレッジ目標**: Domain層 [%]以上
- **実行コマンド**:
  ```bash
  Unity -batchmode -runTests -testPlatform EditMode -projectPath . -testResults results.xml
  ```

### テスト対象の分類

| レイヤー | テスト可否 | 理由 |
|---|---|---|
| Domain Layer（純粋C#） | ✅ EditModeでテスト | UnityEngine非依存 |
| Presentation Layer（MonoBehaviour） | ❌ 対象外 | PlayMode必要（非対応） |
| Infrastructure Layer | ⚠️ 限定的 | モック化で対応可能 |

---

## 技術的制約

### 環境要件

| プラットフォーム | 最小環境 |
|---|---|
| PC (Windows/Mac) | [スペック] |
| iOS | [最小モデル / OSバージョン] |
| Android | [最小スペック / APIレベル] |

### Unity固有の制約

- PlayModeテストは非対応（EditModeテストのみ）
- ファイル削除は手動操作（`rm` コマンド禁止: CLAUDE.md参照）
- ビルドはUnity Editorから実施（CLIビルドは要確認）

### パフォーマンス制約

- [制約1: 例 - WebGLビルドではファイルI/Oが使用不可]
- [制約2]
