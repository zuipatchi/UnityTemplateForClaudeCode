# 設計書

## アーキテクチャ概要

{採用するアーキテクチャパターンを記述（Presentation/Domain/Infrastructure層など）}

```
{アーキテクチャ図をテキストやMermaidで記述}

例:
PlayerController (Presentation)
    ↓
HpSystem (Domain)
    ↓
SaveDataRepository (Infrastructure)
```

## コンポーネント設計

### 1. {クラス名1}（{層名: Domain / Presentation / Infrastructure}）

**責務**:
- {責務1}
- {責務2}

**実装の要点**:
- {実装時の注意点（例: UnityEngine非依存で純粋C#として実装）}
- {技術的な制約（例: Update()内でのGCアロケーション禁止）}

**C#クラス定義（概要）**:
```csharp
public class {クラス名1}
{
    // {フィールド}
    private {型} _{フィールド名};

    // {メソッド}
    public {戻り値型} {メソッド名}({引数}) { }
}
```

### 2. {クラス名2}（{層名}）

**責務**:
- {責務1}
- {責務2}

**実装の要点**:
- {実装時の注意点}
- {技術的な制約}

## データフロー

### {ユースケース名}
```
1. {ステップ1（例: PlayerController が入力を検知）}
2. {ステップ2（例: HpSystem.TakeDamage() を呼び出す）}
3. {ステップ3（例: HpView が HP の変化を表示）}
```

## エラーハンドリング戦略

### 不正値チェック

{不正な入力やエラー状態をどのように処理するか}

```csharp
// 例: HP の不正値チェック
public void TakeDamage(int damage)
{
    if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
    // ...
}
```

### エラーハンドリングパターン

{エラーをどのように処理するか（ログ出力、例外スロー、デフォルト値など）}

## テスト戦略

### EditModeテスト（NUnit）

**テスト対象（Domain層のみ）**:
- {テスト対象クラス1}: {テスト内容}
- {テスト対象クラス2}: {テスト内容}

**テストの命名規則**:
```
[メソッド名]_[条件]_[期待する結果]
例: TakeDamage_WhenDamageExceedsHp_ShouldReturnZero
```

**テストファイルの配置**:
```
Assets/Tests/{クラス名}Tests.cs
```

**注意**: PlayModeテストは対象外（CLAUDE.md参照）

## 追加するUnityパッケージ

{新しく追加するパッケージがあれば記述。なければ「なし」と記載}

```json
// Packages/manifest.json への追加分
{
  "dependencies": {
    "{パッケージ名}": "{バージョン}"
  }
}
```

## ディレクトリ構造

```
{追加・変更されるファイル構造}

例:
Assets/
├── Scripts/
│   └── Sample/
│       ├── Domain/
│       │   └── {システム名}.cs          # 新規追加
│       └── Presentation/
│           └── {コントローラー名}.cs    # 新規追加
└── Tests/
    └── {システム名}Tests.cs             # 新規追加
```

## 実装の順序

1. {実装ステップ1（例: Domain層のクラスを実装）}
2. {実装ステップ2（例: EditModeテストを実装してDomain層を検証）}
3. {実装ステップ3（例: Presentation層のMonoBehaviourを実装）}
4. {実装ステップ4（例: シーンにGameObjectを配置して動作確認）}

## パフォーマンス考慮事項

- {Update()内でのGCアロケーション: new演算子を使わない、文字列連結しない等}
- {頻繁に生成・破棄するオブジェクトにはObjectPoolを使用}
- {GetComponent()はAwake()でキャッシュする}

## スケーリング考慮事項

{将来的な拡張を考慮した設計（インターフェース導入、イベント化など）}
