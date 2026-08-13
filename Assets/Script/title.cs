using UnityEngine;                  // Unityの基本機能（MonoBehaviourなど）を使うための準備
using UnityEngine.SceneManagement;  // シーンの切り替え（LoadSceneなど）をするための準備
using TMPro;                        // TextMeshPro（文字を表示・入力するUI）を使うための準備
using Unity.Netcode;                // マルチプレイの要である「NGO」を使うための準備
using Unity.Netcode.Transports.UTP; // NGOの裏で動く通信の仕組み「UTP」をいじるための準備
using System;
using System.Net;                   // Dns（IPを探す機能）を使うための準備
using System.Net.Sockets;           // IPアドレスの種類（IPv4かどうかなど）を判別するための準備

public class Title : MonoBehaviour
{
    [Header("--- UI References ---")]
    [Tooltip("接続先のIPアドレスを入力するフィールド。空欄の場合はホストとして起動します。")]
    public TMP_InputField addressInput;
    // Unity内でtitlescenes上にcanveで置いてある。クライアントで入るときはここにホスト側のUnity上でのIPアドレスを入力。

    [Tooltip("自分自身のローカルIPアドレスを表示するためのテキストUI。")]
    public TextMeshProUGUI myAddressText;
    // 上のunity上でのIPアドレスはこれ、pc本体のとは異なるので注意。

    [Header("--- Connection Settings ---")]
    [Tooltip("通信に使用するポート番号。ルーターのポート開放やファイアウォールの許可が必要です。")]
    public ushort port = 7777;
    // 基本変えない

    void Start()
    {
        // 起動時にUnity上のIPアドレスを取得してテキストUIに表示する
        if (myAddressText != null)
        {
            myAddressText.text = "Your IP:\n" + GetUnityLocalIP();
        }
    }

    void Update()
    {
        // VR用 (QuestのA/Bボタン)
        if (OVRInput.GetDown(OVRInput.RawButton.A)) StartHost();
        if (OVRInput.GetDown(OVRInput.RawButton.B)) StartClient();
    }

    public void StartHost()
    {
        ApplyAddress(true); // ホスト用のIP設定を適用
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        //ここのGameはScenes名
    }

    public void StartClient()
    {
        ApplyAddress(false); // クライアント用のIP設定を適用
        NetworkManager.Singleton.StartClient();
    }

    // --- 【裏方の仕事①：通信先の設定（IPとポート）を適用する関数】 ---
    // isHost が true ならホスト用、false ならクライアント用の設定にする
    private void ApplyAddress(bool isHost)
    {
        // NetworkManagerにくっついている「UnityTransport（通信の配達員）」を取得する
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // 配達員に「ポート番号はこれを使ってね」と教える
        transport.ConnectionData.Port = port;

        if (isHost)
        {
            // 自分自身（127.0.0.1）をホストの住所に設定する
            transport.ConnectionData.Address = "127.0.0.1";
            // 「0.0.0.0」にすることで、他のどんなIPからの接続も受け入れる（Listen）状態にする
            transport.ConnectionData.ServerListenAddress = "0.0.0.0";
            // これは他からの接続を待ち受ける設定
        }
        else
        {
            // 入力枠の文字を取得する
            string inputAddress = addressInput != null ? addressInput.text.Trim() : "";
            // もし入力されていればそのIPを、空っぽならテスト用に「127.0.0.1（自分自身）」を接続先に設定する
            transport.ConnectionData.Address = !string.IsNullOrEmpty(inputAddress) ? inputAddress : "127.0.0.1";
        }
    }

    // --- 【修正版】現在アクティブなネットワーク（Wi-Fi/LAN）のIPアドレスを確実に取得する処理 ---
    private string GetUnityLocalIP()
    {
        try
        {
            // UDPソケットを作成してアクティブなネットワークアダプターのIPを判定する（ダミー接続）
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null)
                {
                    return endPoint.Address.ToString(); // Wi-Fi等で実際に使われている192.168.x.xなどが取得できる
                }
            }
        }
        catch
        {
            // ソケットでの取得に失敗した場合は予備（フォールバック）で従来のDns検索を行う
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }

        // どこからも取れなかった場合の初期値
        return "127.0.0.1";
    }
}