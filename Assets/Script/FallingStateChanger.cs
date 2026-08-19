using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FallingStateChanger : NetworkBehaviour
{
    [Header("変化後の新しいプレハブ（複数設定可能）")]
    [Tooltip("インスペクターの右下の + ボタンで枠を増やし、出したいプレハブをそれぞれ設定します")]
    public List<GameObject> newBlockPrefabs = new List<GameObject>();

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
            // 全員の画面で変化音を鳴らす
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
            AudioSource.PlayClipAtPoint(dropSound, soundPosition);
        }
    }

    private void ChangeBlock()
    {
        isChanged = true;

        // リストに登録された数の分だけ順番に生成する
        foreach (GameObject prefab in newBlockPrefabs)
        {
            if (prefab == null) continue; // 空欄(None)があった場合はスキップする

            // 物理演算で重なって爆発（反発）しないように、少しだけ位置を散らす
            Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0f, 0.5f), Random.Range(-0.2f, 0.2f));
            Vector3 spawnPosition = transform.position + randomOffset;

            // 1. 新しいプレハブを少しずらした位置・元の回転で生成する
            GameObject newBlock = Instantiate(prefab, spawnPosition, transform.rotation);

            // 2. 新しいプレハブをネットワーク上の全プレイヤーに同期（Spawn）する
            NetworkObject netObj = newBlock.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
        }

        // 3. 自分自身（古いブロック）をネットワークから完全に消去する
        GetComponent<NetworkObject>().Despawn();
    }
}