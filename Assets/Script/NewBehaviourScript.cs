using UnityEngine;
using Unity.Netcode;

public class TestSimpleUI : MonoBehaviour
{
    void OnGUI()
    {
        // 画面の左上に表示エリアを作成
        GUILayout.BeginArea(new Rect(20, 20, 300, 250));

        // 1. ネットワークの役割を表示
        string role = "停止中";
        if (NetworkManager.Singleton.IsHost) role = "ホスト(親)";
        else if (NetworkManager.Singleton.IsClient) role = "クライアント(子)";
        
        GUILayout.Label($"【役割】: {role}");

        // 2. 接続人数を表示（これが重要！）
        // サーバー（ホスト）の場合は接続リストの数を、クライアントの場合は自分のみかを確認
        int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        GUILayout.Label($"【現在の接続人数】: {connectedCount} 人");

        GUILayout.Space(10);

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            // 未接続時
            if (GUILayout.Button("Host開始 (このPCを親にする)", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUILayout.Button("Client開始 (親に繋ぐ)", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
        else
        {
            // 接続動作中
            if (connectedCount >= 2)
            {
                GUI.color = Color.green;
                GUILayout.Label("✅ 相手との通信を確認！完璧です！");
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.Label("待機中... (相手の接続を待っています)");
            }
            
            GUILayout.Space(10);
            if (GUILayout.Button("切断する"))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}