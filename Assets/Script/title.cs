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
    [Tooltip("接続先のIPアドレスを入力するフィールド。")]
    public TMP_InputField addressInput;

    [Tooltip("自分自身のローカルIPアドレスを表示するためのテキストUI。")]
    public TextMeshProUGUI myAddressText;

    [Header("--- Connection Settings ---")]
    [Tooltip("通信に使用するポート番号。")]
    public ushort port = 7777;

    // PC画面のUIで入力されたIPアドレスを保持する変数
    private string pcInputIP = "192.168.10.182";

    void Start()
    {
        // 起動時にUnity上のIPアドレスを取得してテキストUIに表示する
        if (myAddressText != null)
        {
            myAddressText.text = "Your IP:\n" + GetUnityLocalIP();
        }

        // TMP_InputFieldの初期値があれば取得、なければデフォルト値を入れる
        if (addressInput != null && !string.IsNullOrEmpty(addressInput.text))
        {
            pcInputIP = addressInput.text;
        }
    }

    void Update()
    {
        // VR用 (QuestのA/Bボタン)
        if (OVRInput.GetDown(OVRInput.RawButton.A)) StartHost();
        if (OVRInput.GetDown(OVRInput.RawButton.B)) StartClient();
    }

    // ★追加：ビルド後のPC画面上に入力欄を表示する処理
    private void OnGUI()
    {
        // すでに接続中（HostまたはClient起動後）はPC画面のUIを非表示にする
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            return;
        }

        // PC画面左上にIP入力ボックスを表示
        GUILayout.BeginArea(new Rect(20, 20, 300, 130), GUI.skin.box);
        GUILayout.Label("<b>【PC操作】接続先IPアドレス</b>");
        
        // PCの物理キーボードで文字入力（127.0.0.1 などを書き換え可能）
        pcInputIP = GUILayout.TextField(pcInputIP, GUILayout.Height(25));

        // UI（TMP_InputField）が存在していれば文字を自動同期する
        if (addressInput != null)
        {
            addressInput.text = pcInputIP;
        }

        GUILayout.Space(5);
        GUILayout.Label("<b>VRコントローラー操作:</b>");
        GUILayout.Label("・<b>Aボタン</b>: 部屋を作る (Host)");
        GUILayout.Label("・<b>Bボタン</b>: 参加する (Client)");
        GUILayout.EndArea();
    }

    public void StartHost()
    {
        ApplyAddress(true); // ホスト用のIP設定を適用
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        ApplyAddress(false); // クライアント用のIP設定を適用
        NetworkManager.Singleton.StartClient();
    }

    // --- 【裏方の仕事①：通信先の設定（IPとポート）を適用する関数】 ---
    private void ApplyAddress(bool isHost)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Port = port;

        if (isHost)
        {
            // 自分自身（127.0.0.1）をホストの住所に設定する
            transport.ConnectionData.Address = "127.0.0.1";
            // 「0.0.0.0」にすることで、他のどんなIPからの接続も受け入れる
            transport.ConnectionData.ServerListenAddress = "0.0.0.0";
        }
        else
        {
            // PCのOnGUIまたはTMP_InputFieldで入力された最新のIPアドレスを取得
            string inputAddress = "";
            
            if (addressInput != null && !string.IsNullOrEmpty(addressInput.text.Trim()))
            {
                inputAddress = addressInput.text.Trim();
            }
            else
            {
                inputAddress = pcInputIP.Trim();
            }

            // もし入力されていればそのIPを、空っぽなら「127.0.0.1」を接続先に設定
            transport.ConnectionData.Address = !string.IsNullOrEmpty(inputAddress) ? inputAddress : "127.0.0.1";

            if (myAddressText != null)
            {
                myAddressText.text = "Trying to connect:\n" + transport.ConnectionData.Address;
            }
        }
    }

    // --- 【現在アクティブなネットワークのIPアドレスを確実に取得する処理】 ---
    private string GetUnityLocalIP()
    {
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null)
                {
                    return endPoint.Address.ToString();
                }
            }
        }
        catch
        {
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }

        return "127.0.0.1";
    }
}