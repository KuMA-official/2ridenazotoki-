using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FallingStateChanger : NetworkBehaviour
{
    [Header("変化後の新しいプレハブ（複数設定可能）")]
    [Tooltip("インスペクターの右下の + ボタンで枠を増やし、出したいプレハブをそれぞれ設定します")]
    public List<GameObject> newBlockPrefabs = new List<GameObject>();

    [Header("この高さを下回ったら変化する (Y座標)")]
    public float thresholdY = -2.0f;

    [Header("SE設定")]
    [SerializeField] private AudioClip dropSound; // 鳴らしたい音声ファイル(.wavや.mp3)

    // 重複して処理が走るのを防ぐためのフラグ
    private NetworkVariable<bool> isChanged = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 1. フラグが変わった時に全員の画面で切り替えるイベントを登録
        isChanged.OnValueChanged += OnStateChanged;

        // 2. 初期状態を確実に適用する
        OnStateChanged(false, isChanged.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isChanged.OnValueChanged -= OnStateChanged;
    }

    // 全員のPCで状態が変わった時に呼ばれる処理
    private void OnStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            // 1. 割れたオブジェクト（newBlockPrefabs）をすべてアクティブ化
            foreach (GameObject prefab in newBlockPrefabs)
            {
                if (prefab == null) continue;
                prefab.SetActive(true);

                // Rigidbodyがついている（＝飛び散る破片）なら、親子関係を解除して置き去りにする
                Rigidbody[] rbs = prefab.GetComponentsInChildren<Rigidbody>();
                if (rbs.Length > 0)
                {
                    prefab.transform.SetParent(null);
                }
            }

            // 2. 元のオブジェクトの見た目と当たり判定を消す
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                // newBlockPrefabs 配下の要素は消さないよう判定
                bool isDebris = false;
                foreach (var p in newBlockPrefabs)
                {
                    if (p != null && r.transform.IsChildOf(p.transform)) isDebris = true;
                }
                if (!isDebris) r.enabled = false;
            }

            var colliders = GetComponentsInChildren<Collider>();
            foreach (var c in colliders)
            {
                bool isDebris = false;
                foreach (var p in newBlockPrefabs)
                {
                    if (p != null && c.transform.IsChildOf(p.transform)) isDebris = true;
                }
                if (!isDebris) c.enabled = false;
            }

            // 3. 全員の画面で変化音を鳴らす
            if (dropSound != null)
            {
                AudioSource.PlayClipAtPoint(dropSound, transform.position);
            }
        }
        else
        {
            // 初期状態：変化後のオブジェクトを非アクティブにしておく
            foreach (GameObject prefab in newBlockPrefabs)
            {
                if (prefab == null) continue;
                prefab.SetActive(false);
            }
        }
    }

    void Update()
    {
        // 判定はサーバー（ホスト）のPCだけで行う
        if (!IsServer || isChanged.Value) return;

        // Y座標が設定した高さを下回ったかチェック
        if (transform.position.y < thresholdY)
        {
            isChanged.Value = true;
        }
    }
}