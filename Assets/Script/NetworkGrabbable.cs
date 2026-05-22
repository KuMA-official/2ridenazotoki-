using Unity.Netcode;
using UnityEngine;

public class NetworkGrabbable : NetworkBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 1. 掴んだ時に呼ばれる
    public void OnGrab()
    {
        // 所有権を自分（LocalClientId）に変えてとリクエストする
        RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);

       
    }

    // 2. 離した時に呼ばれる
    public void OnRelease()
    {
        Debug.Log("【デバッグ】OnReleaseが呼ばれました！ローカルで物理を再開します。");

    }

    // 所有権の変更だけは、どうしてもサーバーにお願いする必要があるため残します
    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong newOwnerId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);
    }
}