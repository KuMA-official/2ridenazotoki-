using Unity.Netcode;
using UnityEngine;

public class BucketController : NetworkBehaviour
{
    [Header("バケツの中の水面オブジェクト")]
    public GameObject waterSurface;

    [Header("水と判定するタグ名")]
    public string waterTag = "Water";

    [Header("水をこぼす傾き角度（度）")]
    public float pourAngleThreshold = 90f;

    // ネットワーク上の全員で共有される「満水状態」フラグ (true: 水が入っている, false: 空)
    private NetworkVariable<bool> isFull = new NetworkVariable<bool>();

    // 現在入っている「暖炉の消火エリア」を記憶する変数
    private Fireplace currentFireplaceArea;

    void Update()
    {
        // 1. 【全員のPCで実行】共有フラグを見て、バケツの中の水面を表示/非表示にする
        if (waterSurface != null)
        {
            waterSurface.SetActive(isFull.Value);
        }

        // ----------------------------------------------------
        // これより下の判定（傾きチェック・水をこぼす処理）はサーバー（ホスト）だけで行う
        if (!IsServer) return;

        // 水が入っていないときは傾き判定をしない
        if (!isFull.Value) return;

        // 2. バケツの傾きをチェック
        // 「バケツの上方向(transform.up)」と「真上(Vector3.up)」の角度差を計算
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        // 90度以上傾いたら水をこぼす！
        if (tiltAngle >= pourAngleThreshold)
        {
            PourWater();
        }
    }

    /// <summary>
    /// 当たり判定に入った時（水に触れた / 暖炉エリアに入った）
    /// </summary>


    private void OnTriggerEnter(Collider other)
    {
        // ...
        Fireplace fireplace = other.GetComponent<Fireplace>();
        if (fireplace != null)
        {
            currentFireplaceArea = fireplace;
        }
    }

    /// <summary>
    /// 当たり判定から出た時（暖炉エリアから離れた）
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        // ...
        Fireplace fireplace = other.GetComponent<Fireplace>();
        if (fireplace != null && fireplace == currentFireplaceArea)
        {
            currentFireplaceArea = null;
        }
    }

    /// <summary>
    /// 水をこぼす処理
    /// </summary>
    private void PourWater()
    {
        // 1. バケツを空っぽにする
        isFull.Value = false;

        // 2. もし暖炉の消火エリア内にいたら、暖炉に「火を消せ！」と命令を送る
        if (currentFireplaceArea != null)
        {
            currentFireplaceArea.ExtinguishFire();
        }
    }
}