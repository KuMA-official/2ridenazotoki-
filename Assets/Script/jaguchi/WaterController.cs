using Unity.Netcode;
using UnityEngine;

public class WaterController : NetworkBehaviour
{
    [Header("表示/非表示を切り替える水オブジェクト")]
    public GameObject waterObject;

    [Header("水が出始める回転角度（度）")]
    public float targetAngle = 90f;

    // ネットワーク上の全員で共有される「水が出ているか」のフラグ
    private NetworkVariable<bool> isWaterRunning = new NetworkVariable<bool>();

    void Update()
    {
        // 1. 【全員のPCで実行】共有フラグの中身を見て、水の表示・非表示を常に合わせる
        if (waterObject != null)
        {
            waterObject.SetActive(isWaterRunning.Value);
        }

        // これより下の「角度チェックと判定」はサーバー（ホスト）だけで行う
        if (!IsServer) return;

        // 2. 角度の計算 (0〜360度を -180〜180度に直して扱いやすくする)
        float currentAngle = transform.localEulerAngles.y;
        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }

        // 3. 角度が targetAngle（90度）を超えたかチェック
        if (Mathf.Abs(currentAngle) >= targetAngle)
        {
            isWaterRunning.Value = true;  // 水を出す！
        }
        else
        {
            isWaterRunning.Value = false; // 水を止める！
        }
    }
}