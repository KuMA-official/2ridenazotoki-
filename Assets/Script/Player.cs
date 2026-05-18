using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("同期させるネットワーク側のパーツ（アバターの見た目）")]
    [SerializeField] private Transform networkHead;
    [SerializeField] private Transform networkLeftHand;
    [SerializeField] private Transform networkRightHand;

    // 自分のVR機器（OVRCameraRig）の場所を一時的に覚えさせる変数
    private Transform vrCameraTransform;
    private Transform vrLeftHandTransform;
    private Transform vrRightHandTransform;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("自分がオーナーのPlayerが生成されました。VRリグの検索を開始します。");
        }
    }

    void Update()
    {
        // 自分自身でないなら同期処理はNGOに任せる
        if (!IsOwner) return;

        // --- 修正箇所：VRリグがまだ見つかっていない場合は探す（シーンロード待ち対策） ---
        if (vrCameraTransform == null || vrLeftHandTransform == null || vrRightHandTransform == null)
        {
            // 1. 頭（カメラ）を探す
            if (Camera.main != null)
            {
                vrCameraTransform = Camera.main.transform;
            }

            // 2. MetaのOVRCameraRigを探す
            OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null)
            {
                vrLeftHandTransform = rig.leftHandAnchor;
                vrRightHandTransform = rig.rightHandAnchor;
                Debug.Log("OVRCameraRigを検出しました！トラッキングを開始します。");
            }

            // まだ見つからなければ、ここで処理を止めて次のフレームで再挑戦する
            if (vrCameraTransform == null || vrLeftHandTransform == null) return;
        }

        // --- 自分のVR機器の動きを、アバターのパーツにコピーする ---

        // 頭の同期
        if (networkHead != null)
        {
            networkHead.position = vrCameraTransform.position;
            networkHead.rotation = vrCameraTransform.rotation;
        }

        // 左手の同期
        if (networkLeftHand != null)
        {
            networkLeftHand.position = vrLeftHandTransform.position;
            networkLeftHand.rotation = vrLeftHandTransform.rotation;
        }

        // 右手の同期
        if (networkRightHand != null)
        {
            networkRightHand.position = vrRightHandTransform.position;
            networkRightHand.rotation = vrRightHandTransform.rotation;
        }
    }
}