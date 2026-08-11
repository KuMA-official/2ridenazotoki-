using Unity.Netcode;
using UnityEngine;

public class Fireplace : NetworkBehaviour
{
    [Header("消す火のオブジェクト（パーティクルなど）")]
    public GameObject fireObject;

    [Header("出現させる鍵オブジェクト")]
    public GameObject keyObject;

    // ネットワーク上の全員で共有される「火が消えたか」のフラグ
    private NetworkVariable<bool> isFireExtinguished = new NetworkVariable<bool>();

    void Update()
    {
        // 【全員のPCで実行】火が消えた状態をリアルタイムで画面に反映する
        if (isFireExtinguished.Value)
        {
            // 火を消す
            if (fireObject != null && fireObject.activeSelf)
            {
                fireObject.SetActive(false);
            }

            // 鍵を表示する
            if (keyObject != null && !keyObject.activeSelf)
            {
                keyObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// バケツ（BucketController）から「水をこぼした」と通知された時に呼ばれる関数
    /// </summary>
    public void ExtinguishFire()
    {
        // 判定はサーバー（ホスト）側のみで行う
        if (!IsServer) return;

        // すでに火が消えている場合は処理しない
        if (isFireExtinguished.Value) return;

        // 消火フラグを true に変更（全員のPCへ即座に同期される）
        isFireExtinguished.Value = true;

        Debug.Log("暖炉の火が消え、鍵が出現しました！");
    }
}