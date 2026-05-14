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
        // 自分がこのオブジェクトの持ち主（自分自身）である場合のみ実行
        if (IsOwner)
        {
            // 1. 頭（カメラ）を探す
            if (Camera.main != null)
            {
                vrCameraTransform = Camera.main.transform;
            }

            // 2. MetaのOVRCameraRigをシーン内から探す
            OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();

            if (rig != null)
            {
                // Meta公式の「Anchor」を同期元としてセット
                vrLeftHandTransform = rig.leftHandAnchor;
                vrRightHandTransform = rig.rightHandAnchor;
                Debug.Log("OVRCameraRigの手を認識しました！");
            }
            else
            {
                // もしこれが出るなら、Gameシーンに[BuildingBlock] Camera Rigがない証拠です
                Debug.LogError("OVRCameraRigが見つかりません。Gameシーンに配置されていますか？");
            }
        }
    }

    void Update()
    {
        // 自分自身でない（＝他人の画面に映っている他人）なら、位置の更新はNGO（ClientNetworkTransform）に任せる
        if (!IsOwner) return;

        // --- 自分のVR機器の動きを、アバターのパーツにコピーする ---

        // 頭の同期
        if (vrCameraTransform != null && networkHead != null)
        {
            networkHead.position = vrCameraTransform.position;
            networkHead.rotation = vrCameraTransform.rotation;
        }

        // 左手の同期
        if (vrLeftHandTransform != null && networkLeftHand != null)
        {
            networkLeftHand.position = vrLeftHandTransform.position;
            networkLeftHand.rotation = vrLeftHandTransform.rotation;
        }

        // 右手の同期
        if (vrRightHandTransform != null && networkRightHand != null)
        {
            networkRightHand.position = vrRightHandTransform.position;
            networkRightHand.rotation = vrRightHandTransform.rotation;
        }
    }
}
