using UnityEngine;

public class RoomLightLayerTrigger : MonoBehaviour
{
    [Header("切り替えるライト（空欄の場合は進入したプレイヤーの子要素から自動取得）")]
    public Light targetLight;

    [Header("判定対象のタグ")]
    public string targetTag = "Player";

    // URPの Rendering Layer Mask（ビットマスク値）
    // Layer 0 (Default)     = 1  (1 << 0)
    // Layer 1 (GimmickRoom) = 2  (1 << 1)
    // Layer 0 + Layer 1     = 3  ((1 << 0) | (1 << 1))
    private const int LAYER_DEFAULT = 1;
    private const int LAYER_GIMMICK = 2;
    private const int LAYER_BOTH = 3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Light l = GetLight(other);
            if (l != null)
            {
                // 部屋に入ったら Layer 0（廊下）と Layer 1（部屋）の両方を照らせるように変更
                l.renderingLayerMask = LAYER_BOTH;

                // ※「部屋の中（Layer 1）だけ」を照らしたい場合はこちら↓に切り替えてください
                // l.renderingLayerMask = LAYER_GIMMICK;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Light l = GetLight(other);
            if (l != null)
            {
                // 部屋から出たら Default (Layer 0) だけを照らす状態に戻す
                l.renderingLayerMask = LAYER_DEFAULT;
            }
        }
    }

    private Light GetLight(Collider col)
    {

        if (targetLight != null) return targetLight;
        
        // 進入したプレイヤー自身、またはその子要素（手元や頭）から Light を探す
        return col.GetComponentInChildren<Light>();
    }
}