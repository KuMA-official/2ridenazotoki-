using Unity.Netcode;
using UnityEngine;
using Oculus.Interaction;

public class NetworkGrabOwnership : NetworkBehaviour
{
    private PointableElement pointable;

    private void Awake()
    {
        pointable = GetComponent<PointableElement>();
    }

    private void OnEnable()
    {
        if (pointable != null)
        {
            // Meta SDKのPointerEvent登録（イベント監視）
            pointable.WhenPointerEventRaised += OnPointerEvent;
        }
    }

    private void OnDisable()
    {
        if (pointable != null)
        {
            pointable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        // イベントの種類が「掴んだ（Select）」瞬間だった場合
        if (evt.Type == PointerEventType.Select)
        {
            // まだ自分が所有権を持っていないなら、所有権をリクエストする
            if (!IsOwner)
            {
                RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong newOwnerId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);
    }
}