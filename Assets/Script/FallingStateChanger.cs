using Unity.Netcode;
using UnityEngine;

public class FallingStateChanger : NetworkBehaviour
{
    [Header("変化後の新しいプレハブ")]
    public GameObject newBlockPrefab;

    [Header("この高さを下回ったら変化する (Y座標)")]
    public float thresholdY = -2.0f;

    [Header("SE設定")]
    [SerializeField] private AudioClip dropSound; // 鳴らしたい音声ファイル(.wavや.mp3)

    // 重複して処理が走るのを防ぐためのフラグ
    private bool isChanged = false;

    void Update()
    {
        // 判定はサーバー（ホスト）のPCだけで行う
        if (!IsServer || isChanged) return;

        // Y座標が設定した高さを下回ったかチェック
        if (transform.position.y < thresholdY)
        {
            // 全員の画面で変化音を鳴らす（消滅対策済みのRPC呼び出し）
            PlaySoundClientRpc(transform.position);

            ChangeBlock();
        }
    }

    // 全プレイヤーの画面で音を鳴らす命令
    [ClientRpc]
    private void PlaySoundClientRpc(Vector3 soundPosition)
    {
        if (dropSound != null)
        {
            // オブジェクト本体がDespawnして消えても、その場所に臨時スピーカーを作って最後まで鳴らしてくれる機能
            AudioSource.PlayClipAtPoint(dropSound, soundPosition);
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