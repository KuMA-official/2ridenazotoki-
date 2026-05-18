using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class title : MonoBehaviour
{
    [Header("入力フィールド（接続先アドレス）")]
    public TMP_InputField addressInput;

    [Header("自身のアドレス表示用")]
    public TextMeshProUGUI myAddressText;

    [Header("接続設定")]
    public ushort port = 7777;

    [Header("遷移先")]
    public string nextSceneName;

    private UnityTransport transport;
    
    // 二重押しを防止するためのフラグ
    private bool isConnecting = false;

    void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("NetworkManagerにUnityTransportがアタッチされていません。");
        }
        else
        {
            Debug.Log("UnityTransportを検出しました。");
        }

        // 起動時に「候補となるローカルIP」をすべて取得してUIに表示する
        string localIPs = GetAllLocalIPAddresses();
        if (myAddressText != null)
        {
            myAddressText.text = "Your Addresses:\n" + localIPs;
        }
    }

    private void Update()
    {
        // すでに接続済み、または接続処理中ならボタン入力を無視する
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer || isConnecting)
        {
            return;
        }

        // PCデバッグ用ショートカットキー
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Hキー押下：ホストを開始します");
            StartHost();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Cキー押下：クライアントを開始します");
            StartClient();
        }

        // VR用 (QuestのA/Bボタン)
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            Debug.Log("Aボタン押下：ホストを開始します");
            StartHost();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Debug.Log("Bボタン押下：クライアントを開始します");
            StartClient();
        }
    }

    // --- UIのボタンから呼び出されるメソッド ---
    public void OnConnectButtonPressed()
    {
        if (isConnecting) return;

        string inputAddress = addressInput != null ? addressInput.text.Trim() : "";

        if (string.IsNullOrEmpty(inputAddress))
        {
            Debug.Log("IPアドレスが未入力のため、ホストとして起動します。");
            StartHost();
        }
        else
        {
            Debug.Log($"IPアドレスが入力されているため、クライアントとして接続します。接続先: {inputAddress}");
            StartClient();
        }
    }

    private void ApplyAddress(bool isHost)
    {
        if (transport == null) return;

        string inputAddress = addressInput != null ? addressInput.text.Trim() : "";

        if (isHost)
        {
            // ホストは自分自身に接続するため 127.0.0.1 で固定
            transport.ConnectionData.Address = "127.0.0.1";
            transport.ConnectionData.Port = port;
            transport.ConnectionData.ServerListenAddress = "0.0.0.0";
            Debug.Log($"ホスト設定完了：Listen=127.0.0.1:{port} / serverListenAddress=0.0.0.0");
        }
        else
        {
            // クライアント用設定
            string connectTo = !string.IsNullOrEmpty(inputAddress) ? inputAddress : "127.0.0.1";
            transport.ConnectionData.Address = connectTo;
            transport.ConnectionData.Port = port;
            Debug.Log($"クライアント設定完了：接続先={connectTo}:{port}");
        }
    }

    private string GetAllLocalIPAddresses()
    {
        string ipList = "";
        try
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    string name = ni.Name.ToLower();
                    string desc = ni.Description.ToLower();
                    
                    // 仮想アダプタを除外
                    if (name.Contains("virtual") || desc.Contains("virtual") ||
                        name.Contains("vpn") || desc.Contains("vpn") ||
                        name.Contains("vethernet") || name.Contains("tailscale") || 
                        name.Contains("zerotier") || name.Contains("wsl"))
                    {
                        continue;
                    }

                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            string ipStr = ip.Address.ToString();
                            if (ipStr.StartsWith("169.254.")) continue;
                            ipList += $"{ipStr} ({ni.Name})\n"; 
                        }
                    }
                }
            }
        }
        catch
        {
            Debug.LogWarning("ローカルIPアドレスの取得に失敗しました。");
        }
        
        return string.IsNullOrEmpty(ipList) ? "127.0.0.1" : ipList.TrimEnd();
    }

    private void StartHost()
    {
        if (isConnecting) return;
        isConnecting = true;

        ApplyAddress(true);
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("ホストを開始しました。");
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("ホストの起動に失敗しました。");
            isConnecting = false; // 失敗したら再度押せるようにする
        }
    }

    private void StartClient()
    {
        if (isConnecting) return;
        isConnecting = true;

        ApplyAddress(false);
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("クライアント接続開始。");
            // クライアントの場合、成功したかどうかは OnClientConnectedCallback 等で判断するため
            // 接続に時間がかかる場合のタイムアウト処理などは別途必要になることがあります
        }
        else
        {
            Debug.LogError("クライアント接続に失敗しました。");
            isConnecting = false; // 失敗したら再度押せるようにする
        }
    }
}