using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [Header("オンオフしたいセンサー(暗闇)オブジェクト")]
    public GameObject sensorObject;

    [Header("ドアの目隠し(Maku)オブジェクト")]
    public GameObject makuObject;

    private void OnTriggerEnter(Collider other)
    {
        // 触れたオブジェクトが「Hands」というタグを持っているか確認
        if (other.CompareTag("Hands"))
        {
            if (sensorObject != null)
            {
                // 次の状態を決める（現在オンならオフ、オフならオン）
                bool nextState = !sensorObject.activeSelf;

                // センサー（暗闇）のオンオフ切り替え
                sensorObject.SetActive(nextState);

                // 幕（Maku）もセンサーと同じ状態に連動させる
                if (makuObject != null)
                {
                    makuObject.SetActive(nextState);
                }
            }
        }
    }
}