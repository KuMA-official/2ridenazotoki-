using Unity.Netcode;
using UnityEngine;

public class NetworkGrabbable : NetworkBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 1. 掴んだ時に呼ばれる（MetaのInteraction SDKなどのイベントから実行）
    public void OnGrab()
    {
        // サーバーに対して「所有権を自分（LocalClientId）に変えて」とリクエストする
        RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);

        // 掴んでいる間は物理演算が邪魔しないようにKinematicにする
        SetKinematicServerRpc(true);
    }

    // 2. 離した時に呼ばれる
    public void OnRelease()
    {
        // 物理演算を再開させる
        SetKinematicServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong newOwnerId)
    {
        // サーバー側で所有権を変更
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);
    }

    [ServerRpc]
    private void SetKinematicServerRpc(bool isKinematic)
    {
        // 全員に状態を同期させる（NetworkVariableを使っても良い）
        SetKinematicClientRpc(isKinematic);
    }

    [ClientRpc]
    private void SetKinematicClientRpc(bool isKinematic)
    {
        rb.isKinematic = isKinematic;
    }
}