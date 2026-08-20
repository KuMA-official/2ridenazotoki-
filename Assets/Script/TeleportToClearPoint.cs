using UnityEngine;

public class TeleportToClearPoint : MonoBehaviour
{
    [SerializeField] private Transform clearPoint; // ワープ先のTransform
    [SerializeField] private GameObject clearObject; // 表示させたいクリアUIや演出

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // CharacterController使用時の移動キャンセル対策
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // プレイヤーの位置と回転をクリアポイントへ移動
            other.transform.position = clearPoint.position;
            other.transform.rotation = clearPoint.rotation;

            if (cc != null) cc.enabled = true;

            // クリア用オブジェクト・UIを表示
            if (clearObject != null)
            {
                clearObject.SetActive(true);
            }
        }
    }
}