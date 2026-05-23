using Unity.Netcode;
using UnityEngine;

public class FallingStateChanger : NetworkBehaviour
{
    [Header("変化後の新しいプレハブ")]
    public GameObject newBlockPrefab;

    [Header("この高さを下回ったら変化する (Y座標)")]
    public float thresholdY = -2.0f;

    // 重複して処理が走るのを防ぐためのフラグ
    private bool isChanged = false;

    void Update()
    {
        // 判定はサーバー（ホスト）のPCだけで行う（全員がやると何個も生成されてしまうため）
        if (!IsServer || isChanged) return;

        // Y座標が設定した高さを下回ったかチェック
        if (transform.position.y < thresholdY)
        {
            ChangeBlock();
        }
    }

    private void ChangeBlock()
    {
        isChanged = true;

        // 1. 新しいプレハブを今の位置・回転で生成する
        GameObject newBlock = Instantiate(newBlockPrefab, transform.position, transform.rotation);

        // 2. 新しいプレハブをネットワーク上の全プレイヤーに同期（Spawn）する
        NetworkObject netObj = newBlock.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }

        // 3. 自分自身（古いブロック）をネットワークから完全に消去する
        GetComponent<NetworkObject>().Despawn();
    }
}