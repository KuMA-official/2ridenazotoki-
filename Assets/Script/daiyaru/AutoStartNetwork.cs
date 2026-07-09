using UnityEngine;
using Unity.Netcode;

public class AutoStartNetwork : MonoBehaviour
{
    void Start()
    {
        // ネットワークが動いていなければ、自動でホストとして開始する
        if (NetworkManager.Singleton != null &&
            !NetworkManager.Singleton.IsClient &&
            !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost();
        }
    }
}
