using Unity.Netcode;
using UnityEngine;

public class WaterController : NetworkBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("表示/非表示を切り替える水オブジェクト")]
    public GameObject waterObject;

    [Header("ダイヤルの回転設定")]
    [Tooltip("ダイヤルが回る軸（基本はY軸）")]
    public RotationAxis rotateAxis = RotationAxis.Y;
    [Tooltip("水が出始める回転角度（度）")]
    public float targetAngle = 90f;

    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dropSound; // 流れる水の音

    // ネットワーク上の全員で共有される「水が出ているか」のフラグ
    private NetworkVariable<bool> isWaterRunning = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 1. フラグが変わった時に全員の画面で切り替えるイベントを登録
        isWaterRunning.OnValueChanged += OnWaterStateChanged;

        // 2. ★【ここを追加】ゲーム開始時に初期状態（最初は水OFF）を確実に適用する！
        OnWaterStateChanged(false, isWaterRunning.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isWaterRunning.OnValueChanged -= OnWaterStateChanged;
    }

    // 全員のPCで「水の状態が変わった時」に呼ばれる処理
    private void OnWaterStateChanged(bool previousValue, bool newValue)
    {
        // 1. 水オブジェクトの表示/非表示切り替え
        if (waterObject != null)
        {
            waterObject.SetActive(newValue);
        }

        // 2. 水の音のON/OFF
        if (audioSource != null && dropSound != null)
        {
            if (newValue)
            {
                audioSource.clip = dropSound;
                audioSource.loop = true; // 出続けている間はループ再生
                if (!audioSource.isPlaying) audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    void Update()
    {
        // 角度チェックと判定はサーバー（ホスト）だけで行う
        if (!IsServer) return;

        // 指定した軸の角度を取得
        float currentAngle = 0f;
        switch (rotateAxis)
        {
            case RotationAxis.X: currentAngle = transform.localEulerAngles.x; break;
            case RotationAxis.Y: currentAngle = transform.localEulerAngles.y; break;
            case RotationAxis.Z: currentAngle = transform.localEulerAngles.z; break;
        }

        // 角度の計算 (0〜360度を -180〜180度に直す)
        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }

        // 角度が targetAngle（90度）を超えたかチェックしてフラグを更新
        isWaterRunning.Value = (Mathf.Abs(currentAngle) >= targetAngle);
    }
}