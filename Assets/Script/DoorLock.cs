using Unity.Netcode;
using UnityEngine;

public class DoorLock : NetworkBehaviour
{
    [Header("ロック解除するコンポーネント（Door_tugaiのGrabInteractable）")]
    public Behaviour targetGrabInteractable;

    // ドアが開いているかどうかを全員に自動同期する変数
    private NetworkVariable<bool> isUnlocked = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        // ロック状態が変更されたら、全員の画面でGrabInteractableを更新する
        isUnlocked.OnValueChanged += OnLockStateChanged;

        // 初期状態の反映
        if (targetGrabInteractable != null)
        {
            targetGrabInteractable.enabled = isUnlocked.Value;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 鍵判定（サーバー/ホスト側のみで処理）
        if (other.CompareTag("Key_1"))
        {
            // 1. 鍵の「当たり判定（Collider）」を即座に消して透明な壁を防ぐ
            Collider keyCollider = other.GetComponent<Collider>();
            if (keyCollider != null) keyCollider.enabled = false;

            // 2. 見た目と物理をオフにする
            MeshRenderer keyRenderer = other.GetComponent<MeshRenderer>();
            if (keyRenderer != null) keyRenderer.enabled = false;

            Rigidbody keyRb = other.GetComponent<Rigidbody>();
            if (keyRb != null) keyRb.isKinematic = true;

            // 3. 鍵穴のセンサーをオフにする
            GetComponent<Collider>().enabled = false;

            // 4. サーバーに鍵開けを通知（全クライアントに同期される）
            UnlockDoorServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnlockDoorServerRpc()
    {
        isUnlocked.Value = true; // これにより OnLockStateChanged が全員のPCで実行される
    }

    private void OnLockStateChanged(bool previousValue, bool newValue)
    {
        if (targetGrabInteractable != null)
        {
            targetGrabInteractable.enabled = newValue;
            Debug.Log("[DoorLock] 全員の画面でドアのロックが解除されました！");
        }
    }
}