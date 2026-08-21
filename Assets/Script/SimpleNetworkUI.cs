using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class PCAddressInputUI : MonoBehaviour
{
    [Header("デフォルトのIPアドレス")]
    [SerializeField] private string targetIP = "192.168.1.5";
    [SerializeField] private ushort port = 7777;

    private bool isIPSet = false;

    private void Start()
    {
        // 起動時にデフォルトIPを自動セット
        ApplyIPAddress(targetIP);
    }

    private void OnGUI()
    {
        // 既に接続済みの場合はPC画面のUIを隠す
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            return;
        }

        // PC画面左上にIP入力用ウィンドを表示
        GUILayout.BeginArea(new Rect(20, 20, 320, 140), GUI.skin.box);
        GUILayout.Label("<b>【PC側操作】接続先IPアドレス</b>");

        GUILayout.Space(5);
        GUILayout.Label("IP入力後、<b>[Enter]</b> キーで確定:");

        // テキスト入力欄
        targetIP = GUILayout.TextField(targetIP, GUILayout.Height(25));

        // Enterキーが押されたか判定
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            ApplyIPAddress(targetIP);
            isIPSet = true;
        }

        // 状態メッセージ表示
        GUILayout.Space(5);
        if (isIPSet)
        {
            GUILayout.Label($"<color=green><b>★ IP確定: {targetIP}</b></color>");
            GUILayout.Label("VR空間内のボタンで接続してください");
        }
        else
        {
            GUILayout.Label("<color=yellow>※ Enterキーを押して確定してください</color>");
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// IPアドレスを UnityTransport に適用する
    /// </summary>
    public void ApplyIPAddress(string ip)
    {
        var transport = NetworkManager.Singleton != null ? NetworkManager.Singleton.GetComponent<UnityTransport>() : null;
        if (transport != null)
        {
            transport.SetConnectionData(ip, port);
            Debug.Log($"[Network] 接続先IPを {ip}:{port} に設定しました。");
        }
    }

    // ========================================================
    // ★ 以下の関数を VR空間内の3Dボタン（When Select等）に割り当てる
    // ========================================================

    /// <summary>
    /// VR内の「部屋を作る (Host)」ボタン用
    /// </summary>
    public void StartHostFromVR()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("[VR操作] Hostとして起動しました！");
        }
    }

    /// <summary>
    /// VR内の「参加する (Client)」ボタン用
    /// </summary>
    public void StartClientFromVR()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            // Enterを押し忘れていても入力されたIPを再適用して接続
            ApplyIPAddress(targetIP);
            NetworkManager.Singleton.StartClient();
            Debug.Log($"[VR操作] {targetIP} へClient接続を開始します！");
        }
    }
}