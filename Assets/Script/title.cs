using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class title : MonoBehaviour
{
    // ホストとして開始
    public void OnClickHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            // NGOのSceneManagerを使ってゲーム画面へ（Build Settingsに"Game"シーンが必要）
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    // クライアントとして開始
    public void OnClickClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void Update()
    {
        // QuestのAボタンでホスト開始テスト用
        if(OVRInput.GetDown(OVRInput.RawButton.A))
        {
            OnClickHost();
        }

        if(OVRInput.GetDown(OVRInput.RawButton.B))
        {
            OnClickClient();
            Debug.Log("Bぼたんおされた");

        }
    }
}

