// ========================================
// 必要なライブラリのインポート
// ========================================
using System.Collections;         // コルーチン（協調的マルチタスク）を使用するために必要
using System.Collections.Generic; // List、Dictionary等のコレクションクラスを使用するために必要
using UnityEngine;                // Unity基本機能（GameObject、Transform、MonoBehaviour等）
using System.IO;                  // ファイルの読み書き機能（CSV保存用）
using System.Text;                // 文字列操作機能（StringBuilder等、効率的な文字列結合用）
using Valve.VR;                   // SteamVR機能（Vive Trackerとの通信用）

/// <summary>
/// Vive Trackerからの位置・速度・角速度データを取得し、CSVファイルに記録する。
/// また、取得した速度に応じて音を再生する機能も提供する。
/// このスクリプトをGameObjectにアタッチして使用する。
/// </summary>
public class ViveTrackerLogger : MonoBehaviour
{
  // ========================================
  // Unityエディターで設定可能なパブリック変数
  // ========================================

  [Header("トラッキング設定")]
  [Tooltip("Vive TrackerにアタッチされたSteamVR_Behaviour_Poseコンポーネントをここに設定してください")]
  public SteamVR_Behaviour_Pose trackerPose; // Vive Trackerの位置・回転・速度情報を取得するためのコンポーネント

  [Header("音声設定")]
  [Tooltip("音を再生するAudioSourceコンポーネント。未設定の場合は実行時に自動で追加されます")]
  public AudioSource audioSource;            // 音を再生するためのAudioSourceコンポーネント
  [Tooltip("速度に応じて再生する音声ファイル（AudioClip）")]
  public AudioClip velocitySound;            // 再生する音声クリップ
  [Range(0f, 2f)]
  [Tooltip("音量の倍率設定（0.0～2.0倍）通常は1.0で使用")]
  public float volumeMultiplier = 1f;        // 音量を調整する倍率
  [Range(0f, 5f)]
  [Tooltip("最大音量で再生される速度（m/s）この値以上の速度では音量が最大になる")]
  public float maxVelocityForVolume = 3f;    // 音量が最大になる速度の基準値

  [Header("データ記録設定")]
  [Tooltip("保存するCSVファイルの名前")]
  public string csvFileName = "vive_tracker_data.csv"; // 保存するCSVファイル名
  [Tooltip("データを記録する間隔（秒）0.1秒 = 10Hz（1秒間に10回記録）")]
  public float loggingInterval = 0.1f;       // データ記録の間隔（秒単位）

  // ========================================
  // 内部処理用のプライベート変数（外部から変更不可）
  // ========================================

  private string csvFilePath;              // CSVファイルの完全なファイルパス（フォルダ名+ファイル名）
  private StringBuilder csvData;           // CSVデータを一時的に保存するメモリ上のバッファ
  private float lastLogTime;              // 最後にデータを記録した時刻（Time.timeで取得）
  private Vector3 previousPosition;       // 前回フレームでの位置（将来の拡張用・現在は未使用）
  private Vector3 currentVelocity;        // 現在フレームでの速度（3次元ベクトル：X,Y,Z方向）
  private Vector3 currentAngularVelocity; // 現在フレームでの角速度（3次元ベクトル：X,Y,Z軸周りの回転速度）
  private bool isTracking = false;        // Vive Trackerがトラッキング中かどうかのフラグ

  /// <summary>
  /// ゲーム開始時に1回だけ呼ばれる初期化メソッド
  /// すべての初期設定を行い、システムの準備を整える
  /// </summary>
  void Start()
  {
    // CSV記録システムの初期化（ファイルパス設定、ヘッダー作成等）
    InitializeCSVLogging();
    // 音声再生システムの初期化（AudioSourceの設定等）
    InitializeAudio();

    // Vive TrackerのSteamVR_Behaviour_Poseコンポーネントが設定されているかチェック
    if (trackerPose == null)
    {
      Debug.LogError("SteamVR_Behaviour_Pose is not assigned!");
      return; // 設定されていない場合は処理を中断
    }

    // 初期位置を記録（将来の機能拡張用）
    previousPosition = trackerPose.transform.position;
    // データ記録開始時刻を記録
    lastLogTime = Time.time;
  }

  /// <summary>
  /// CSVファイル記録システムの初期化
  /// ファイルパスの設定、データバッファの準備、ヘッダー行の作成を行う
  /// </summary>
  void InitializeCSVLogging()
  {
    // アプリケーションの永続データフォルダにCSVファイルのフルパスを作成
    // Application.persistentDataPath: アプリが削除されるまで保持されるフォルダのパス
    // Path.Combine(): フォルダパスとファイル名を適切に結合する（OS間の違いを吸収）
    csvFilePath = Path.Combine(Application.persistentDataPath, csvFileName);

    // CSVデータを効率的に構築するためのStringBuilderを初期化
    // 通常の文字列結合（+演算子）より高速で、メモリ使用量も少ない
    csvData = new StringBuilder();

    // CSVファイルのヘッダー行を作成
    // 各列の意味: 時刻,位置XYZ,速度XYZ,速度の大きさ,角速度XYZ,角速度の大きさ,トラッキング状態
    csvData.AppendLine("Timestamp,PositionX,PositionY,PositionZ,VelocityX,VelocityY,VelocityZ,VelocityMagnitude,AngularVelocityX,AngularVelocityY,AngularVelocityZ,AngularVelocityMagnitude,IsTracking");

    // 初期化完了をUnityのコンソールに出力（デバッグ用）
    Debug.Log($"CSV logging initialized. File path: {csvFilePath}");
  }

  /// <summary>
  /// 音声再生システムの初期化
  /// AudioSourceコンポーネントの取得・作成、音声設定の確認と初期設定を行う
  /// </summary>
  void InitializeAudio()
  {
    // AudioSourceコンポーネントが設定されていない場合の処理
    if (audioSource == null)
    {
      // 同じGameObjectにアタッチされているAudioSourceを探す
      audioSource = GetComponent<AudioSource>();

      // 見つからない場合は新しくAudioSourceコンポーネントを追加
      if (audioSource == null)
      {
        audioSource = gameObject.AddComponent<AudioSource>();
      }
    }

    // 再生する音声クリップが設定されているかチェック
    if (velocitySound == null)
    {
      // 警告メッセージをコンソールに表示（エラーではないが注意が必要）
      Debug.LogWarning("Velocity sound clip is not assigned!");
    }

    // AudioSourceの基本設定
    audioSource.playOnAwake = false;  // ゲーム開始時に自動再生しない
    audioSource.loop = false;         // 音声をループ再生しない（速度変化に応じて動的制御するため）
  }

  /// <summary>
  /// 毎フレーム実行されるメインの更新処理
  /// Vive Trackerのデータ取得、CSV記録、音声再生を順次実行する
  /// </summary>
  void Update()
  {
    // Vive Trackerが設定されていない場合は何もしない
    if (trackerPose == null) return;

    // Vive Trackerから最新のトラッキングデータ（位置、速度、角速度）を取得
    UpdateTrackingData();

    // 指定した間隔（loggingInterval）が経過した場合にCSVにデータを記録
    // 例：loggingInterval = 0.1秒の場合、1秒間に10回記録される
    if (Time.time - lastLogTime >= loggingInterval)
    {
      LogDataToCSV();           // 現在のデータをCSVバッファに追加
      lastLogTime = Time.time;  // 記録時刻を更新
    }

    // 取得した速度データに基づいて音声を再生・制御
    PlayVelocityBasedAudio();
  }

  /// <summary>
  /// Vive Trackerからトラッキングデータを更新する
  /// トラッキング状態の確認と速度・角速度データの取得を行う
  /// </summary>
  void UpdateTrackingData()
  {
    // トラッキング状態を判定
    // isValid: SteamVRがデバイスを認識しているか（デバイスが接続されているか等）
    // isActive: デバイスが現在アクティブな状態か（トラッキング可能な範囲内にあるか等）
    isTracking = trackerPose.isValid && trackerPose.isActive;

    if (isTracking)
    {
      // トラッキングが有効な場合：SteamVRから直接速度データを取得
      currentVelocity = trackerPose.GetVelocity();               // 線形速度（m/s）を取得
      currentAngularVelocity = trackerPose.GetAngularVelocity(); // 角速度（rad/s）を取得
    }
    else
    {
      // トラッキングが無効な場合：速度を0にリセット
      // デバイスが見失われた場合やトラッキング範囲外にある場合の安全な処理
      currentVelocity = Vector3.zero;
      currentAngularVelocity = Vector3.zero;
    }
  }

  void LogDataToCSV()
  {
    if (!isTracking) return;

    float timestamp = Time.time;
    Vector3 position = trackerPose.transform.position;

    string logEntry = string.Format(
        "{0:F3},{1:F6},{2:F6},{3:F6},{4:F6},{5:F6},{6:F6},{7:F6},{8:F6},{9:F6},{10:F6},{11:F6},{12}",
        timestamp,
        position.x, position.y, position.z,
        currentVelocity.x, currentVelocity.y, currentVelocity.z, currentVelocity.magnitude,
        currentAngularVelocity.x, currentAngularVelocity.y, currentAngularVelocity.z, currentAngularVelocity.magnitude,
        isTracking ? "True" : "False"
    );

    csvData.AppendLine(logEntry);

    // Debug output (optional)
    if (Time.frameCount % 100 == 0) // Every 100 frames
    {
      Debug.Log($"Velocity: {currentVelocity.magnitude:F3} m/s, Angular: {currentAngularVelocity.magnitude:F3} rad/s");
    }
  }

  void PlayVelocityBasedAudio()
  {
    if (velocitySound == null || audioSource == null || !isTracking) return;

    float velocityMagnitude = currentVelocity.magnitude;

    if (velocityMagnitude > 0.1f) // Threshold to avoid playing sound for minimal movement
    {
      float normalizedVelocity = Mathf.Clamp01(velocityMagnitude / maxVelocityForVolume);
      float targetVolume = normalizedVelocity * volumeMultiplier;

      if (!audioSource.isPlaying)
      {
        audioSource.clip = velocitySound;
        audioSource.volume = targetVolume;
        audioSource.Play();
      }
      else
      {
        // Adjust volume dynamically while playing
        audioSource.volume = targetVolume;
      }
    }
    else if (audioSource.isPlaying)
    {
      audioSource.Stop();
    }
  }

  void OnApplicationPause(bool pauseStatus)
  {
    if (pauseStatus)
    {
      SaveCSVData();
    }
  }

  void OnApplicationQuit()
  {
    SaveCSVData();
  }

  void SaveCSVData()
  {
    if (csvData != null && csvData.Length > 0)
    {
      try
      {
        File.WriteAllText(csvFilePath, csvData.ToString());
        Debug.Log($"CSV data saved to: {csvFilePath}");
      }
      catch (System.Exception e)
      {
        Debug.LogError($"Failed to save CSV data: {e.Message}");
      }
    }
  }

  // Public method to manually save data
  public void SaveDataManually()
  {
    SaveCSVData();
  }

  // Public method to clear logged data
  public void ClearLoggedData()
  {
    csvData.Clear();
    csvData.AppendLine("Timestamp,PositionX,PositionY,PositionZ,VelocityX,VelocityY,VelocityZ,VelocityMagnitude,AngularVelocityX,AngularVelocityY,AngularVelocityZ,AngularVelocityMagnitude,IsTracking");
    Debug.Log("CSV data cleared");
  }

  // Getter methods for external access
  public Vector3 GetCurrentVelocity() => currentVelocity;
  public Vector3 GetCurrentAngularVelocity() => currentAngularVelocity;
  public bool IsCurrentlyTracking() => isTracking;
  public string GetCSVFilePath() => csvFilePath;
}