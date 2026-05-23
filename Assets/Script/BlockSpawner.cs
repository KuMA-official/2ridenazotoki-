using Unity.Netcode;
using UnityEngine;

public class BlockSpawner : NetworkBehaviour
{
    [Header("変化前の掴めるブロックのプレハブ")]
    public GameObject blockPrefab;

    [Header("生成したい場所（空のGameObjectなどを指定）")]
    public Transform spawnPoint;

    public override void OnNetworkSpawn()
    {
        // サーバー（ホスト）だけが生成を実行する
        if (IsServer)
        {
            GameObject block = Instantiate(blockPrefab, spawnPoint.position, spawnPoint.rotation);
            // ネットワーク上に生み出す
            block.GetComponent<NetworkObject>().Spawn();
        }
    }
}