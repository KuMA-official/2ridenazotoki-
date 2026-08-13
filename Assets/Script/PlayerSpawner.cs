using Unity.Netcode;
using UnityEngine;

public class CustomPlayerSpawner : NetworkBehaviour
{
    [Header("アバタープレハブ")]
    [SerializeField] private GameObject ghostPlayerPrefab; // ホスト（幽霊）用
    [SerializeField] private GameObject humanPlayerPrefab; // クライアント（人間）用

    public override void OnNetworkSpawn()
    {
        // サーバー（ホスト）側だけでプレイヤー生成処理を行う
        if (IsServer)
        {
            // クライアントが接続してきた時のイベントを登録
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // まずホスト自身（自分）を幽霊としてスポーン
            SpawnPlayer(NetworkManager.Singleton.LocalClientId, isGhost: true);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // ホスト以外の接続してきたプレイヤー（クライアント）を人間としてスポーン
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            SpawnPlayer(clientId, isGhost: false);
        }
    }

    private void SpawnPlayer(ulong clientId, bool isGhost)
    {
        GameObject prefabToSpawn = isGhost ? ghostPlayerPrefab : humanPlayerPrefab;
        
        // プレハブを生成
        GameObject playerInstance = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);

        // 該当するクライアントの所有物（PlayerObject）としてネットワーク上にスポーン
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}