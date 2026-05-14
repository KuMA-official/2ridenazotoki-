using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class title : MonoBehaviour
{
    // 二重押しを防止するためのフラグ
    private bool isConnecting = false;

    // ホストとして開始
    public void OnClickHost()
    {
        if (isConnecting) return; // 接続中なら何もしない
        isConnecting = true;

        if (NetworkManager.Singleton.StartHost())
        {
            // NGOのSceneManagerを使ってゲーム画面へ
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else
        {
            isConnecting = false; // 失敗したら再度押せるようにする
        }
    }

    // クライアントとして開始
    public void OnClickClient()
    {
        if (isConnecting) return; // 接続中なら何もしない
        isConnecting = true;

        if (!NetworkManager.Singleton.StartClient())
        {
            isConnecting = false; // 失敗したら再度押せるようにする
        }
    }

    private void Update()
    {
        // すでに接続済み、または接続処理中ならボタン入力を無視する
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer || isConnecting)
        {
            return;
        }

        // QuestのAボタンでホスト開始
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            OnClickHost();
        }

        // QuestのBボタンでクライアント開始
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Debug.Log("Bボタン押された：接続を開始します");
            OnClickClient();
        }
    }
}