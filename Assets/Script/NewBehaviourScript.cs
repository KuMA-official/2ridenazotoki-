using UnityEngine;
using Unity.Netcode;

public class TestSimpleUI : MonoBehaviour
{
    void OnGUI()
    {
        // 画面の左上に少し大きめに表示エリアを作る
        GUILayout.BeginArea(new Rect(20, 20, 250, 200));

        // 現在の状態を表示
        string status = "停止中";
        if (NetworkManager.Singleton.IsHost) status = "ホスト(親)として動作中";
        else if (NetworkManager.Singleton.IsClient) status = "クライアント(子)として接続中";
        else if (NetworkManager.Singleton.IsServer) status = "サーバーとして動作中";

        GUILayout.Label($"【現在の状態】: {status}");
        GUILayout.Space(10);

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
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
            // 接続に成功した時だけ表示
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                GUILayout.Label("✅ 接続成功！パイプは繋がっています。");
            }
            
            if (GUILayout.Button("切断する"))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}