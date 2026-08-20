using UnityEngine;
using Unity.Netcode;

public class GhostTagManagerNGO : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // ホスト以外（一般プレイヤー）なら GhostOnly タグのオブジェクトを一括非表示
        if (!IsHost)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("GhostOnly"))
            {
                obj.SetActive(false);
            }
        }
    }
}