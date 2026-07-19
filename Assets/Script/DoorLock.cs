using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [Header("ロック解除するコンポーネント（Door_tugaiのGrabInteractable）")]
    public Behaviour targetGrabInteractable;

    private void OnTriggerEnter(Collider other)
    {
        // もし触れたオブジェクトのタグが "Key_1" だったら
        if (other.CompareTag("Key_1"))
        {
            // 1. ドアのロックを解除！
            if (targetGrabInteractable != null)
            {
                targetGrabInteractable.enabled = true;
                Debug.Log("鍵が開きました！");
            }

            // --- 2. 鍵を「安全に」消滅させるトリック ---
            // 鍵の見た目（MeshRenderer）をオフにして透明にする
            MeshRenderer keyRenderer = other.GetComponent<MeshRenderer>();
            if (keyRenderer != null)
            {
                keyRenderer.enabled = false;
            }

            // 鍵の物理演算を止めて、手から離した時に床に落ちる音などを防ぐ
            Rigidbody keyRb = other.GetComponent<Rigidbody>();
            if (keyRb != null)
            {
                keyRb.isKinematic = true;
            }

            // 3. この鍵穴（KeyHole）のセンサーをオフにして、2回以上反応しないようにする
            GetComponent<Collider>().enabled = false;
        }
    }
}