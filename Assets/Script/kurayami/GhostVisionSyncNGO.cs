using UnityEngine;
using Unity.Netcode;

public class GhostVisionSyncNGO : NetworkBehaviour
{
    [Header("ホスト（幽霊）だけに表示するオブジェクト群（ライトやブロックなど）")]
    public GameObject[] ghostOnlyObjects;

    public override void OnNetworkSpawn()
    {
        // 自分がホスト（幽霊）かどうか判定
        bool isGhost = IsHost;

        // リストに登録された全オブジェクトを一括で切り替え
        foreach (GameObject obj in ghostOnlyObjects)
        {
            if (obj != null)
            {
                obj.SetActive(isGhost);
            }
        }
    }
}