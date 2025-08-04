# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

このプロジェクトはUnity 6000.0.41f1を使用したSteamVR対応のVRアプリケーションです。SteamVR Unity Plugin v2.8.0が統合されており、VRコントローラーによるインタラクション機能を提供します。

## Development Commands

### Unity関連
- **ビルド**: Unityエディターから File > Build Settings でビルド
- **Play Mode**: Unityエディター上で再生ボタンを押すか、Ctrl+P
- **テスト実行**: Unityエディターの Test Runner (Window > General > Test Runner)

### Git操作
```bash
git status          # プロジェクトの状態確認
git add .           # 変更をステージング
git commit -m "commit message"  # コミット作成
```

## Project Architecture

### Core Structure
- **Assets/Scenes/**: メインシーンファイル
  - `SampleScene.unity`: 基本シーン
  - `Interactions_Example.unity`: SteamVR InteractionSystemのサンプル
- **Assets/SteamVR/**: SteamVR Unity Plugin (v2.8.0)
  - **InteractionSystem/**: VRでの物理的なオブジェクト操作システム
  - **Input/**: SteamVR Input システム関連
  - **Scripts/**: SteamVRコアスクリプト群
- **Assets/Settings/**: レンダリングパイプライン設定 (URP対応)

### Key Components
- **SteamVR Input System**: アクションベースの入力システム
  - `actions.json`: 入力アクション定義
  - バインディングファイル群: 各デバイス用の入力マッピング
- **Interaction System**: VRでの直感的なオブジェクト操作
  - Hand tracking
  - Object grabbing/throwing
  - UI interaction
- **Universal Render Pipeline**: グラフィックス設定

### Project Dependencies
主要なUnityパッケージ:
- `com.unity.render-pipelines.universal`: 17.0.4 (URP)
- `com.unity.inputsystem`: 1.13.1
- `com.unity.xr.management`: 4.5.1
- `com.unity.xr.openxr`: 1.14.3
- `com.valvesoftware.unity.openvr`: ローカルパッケージ

### VR Development Notes
- SteamVRランタイムが必要 (Steam > Tools > SteamVR)
- 開発時はSteamVR Betaブランチの使用を推奨
- SteamVR Input window (Window Menu) でアクション設定
- Interaction Systemサンプルは `Assets/SteamVR/InteractionSystem/Samples/`

### Build Configuration
- Target Platform: PC, Mac & Linux Standalone
- XR Settings: OpenVR (SteamVR)
- Graphics API: Direct3D11/OpenGL
- Scripting Backend: Mono/.NET Framework