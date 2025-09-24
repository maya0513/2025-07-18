# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

このプロジェクトはUnity 6000.0.41f1を使用したAR実験システムです。車いす推進技能学習の効果検証のためのリアルタイム聴覚フィードバックシステムを開発しています。VIVE Focus Vision（ARヘッドセット）とVIVE Trackers 4台を使用してモーションデータを取得し、AR環境でフィードバックを提供します。

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
  - `Interactions_Example.unity`: SteamVR InteractionSystemのサンプル（レガシー）
- **Assets/Scripts/**: カスタムスクリプト群
  - 実験管理システム（被験者選択、セッション管理）
  - VIVE Trackerデータ取得・記録システム
  - AR視覚・聴覚フィードバックシステム
- **Assets/SteamVR/**: SteamVR Unity Plugin (v2.8.0) - レガシーサポート
- **Assets/Settings/**: レンダリングパイプライン設定 (URP対応)
- **docs/**: プロジェクト仕様書
  - `task1.md`: 現在の実装要件
  - `発表スライド.md`: 研究背景と実験デザイン

### Key Components
- **実験管理システム**: 被験者IDとセッション管理
  - 被験者選択UI（新規/既存）
  - 実験フェーズ選択（事前テスト/トレーニング/事後テスト/保持テスト）
  - タスク選択（ジグザグ走/8の字走/バスケットボール持ち8の字走）
- **VIVE Tracker データシステム**: 4台のトラッカーからデータ取得
  - 左手首・右手首・左車輪・右車輪のモーショントラッカー
  - リアルタイムデータ処理とCSV記録
  - VIVE OpenXRプラグインとの連携
- **ARフィードバックシステム**: AR環境でのフィードバック提示
  - 視覚フィードバック: 左右車輪対応の2本のバー表示
  - 聴覚フィードバック: ヘッドセット内蔵スピーカーからの音声
  - ARパススルー越しのオーバーレイ表示
- **VIVE Focus Vision対応**: AR環境での実空間利用
  - ARヘッドセットによる現実描画
  - 環境に干渉しないフィードバック専用利用

### Project Dependencies
主要なUnityパッケージ:
- `com.unity.render-pipelines.universal`: 17.0.4 (URP)
- `com.unity.inputsystem`: 1.13.1
- `com.unity.xr.management`: 4.5.1
- `com.unity.xr.openxr`: 1.14.3 （VIVE OpenXR対応）
- `com.valvesoftware.unity.openvr`: ローカルパッケージ （レガシーサポート）
- `com.unity.ugui`: 2.0.0 （UI System）

### AR実験システム開発
- **ハードウェア要件**:
  - VIVE Focus Vision（ARヘッドセット）
  - VIVE Trackers × 4台（左手首/右手首/左車輪/右車輪）
  - Unity 6000.0.41f1開発環境
- **実験設計**: 3群間比較のランダム化比較試験（RCT）
  - 聴覚フィードバック群
  - 視覚フィードバック群
  - フィードバック無し群
- **実験タスク**: 車いす推進技能学習
  - ジグザグ走（コーン間をジグザグ走行）
  - 8の字走（5m間隔のコーンを8の字に5周）
  - 8の字走（バスケットボール持ち）

### データ記録とフィードバック
- **モーションデータ**: 位置・姿勢をリアルタイム取得
- **CSV記録**: 生データの実験用記録
- **視覚フィードバック**: 車輪動作に対応する左右バー表示
- **聴覚フィードバック**: 推進効率に応じた音響フィードバック
- **AR表示**: 現実空間を妨げない軽量オーバーレイ

### Build Configuration
- Target Platform: Android (VIVE Focus Vision)
- XR Settings: OpenXR (VIVE OpenXR Provider)
- Graphics API: Vulkan/OpenGL ES
- Scripting Backend: IL2CPP
- AR Foundation: パススルー機能対応