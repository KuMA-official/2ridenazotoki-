using UnityEngine;    
using Unity.Netcode;  


// NetworkBehaviourにすることで、マルチプレイの力が使えるようになります
public class Player : NetworkBehaviour
{
    [Header("同期させるネットワーク側のパーツ（アバターの見た目）")]
    [SerializeField] private Transform networkHead;      // 他のプレイヤーからも見える「アバターの頭」パーツ
    [SerializeField] private Transform networkLeftHand;  // 他のプレイヤーからも見える「アバターの左手」パーツ
    [SerializeField] private Transform networkRightHand; // 他のプレイヤーからも見える「アバターの右手」パーツ

    // --- 【内部の変数：自分のVRゴーグルやコントローラーの「現在の位置」を一時的にメモする箱】 ---
    private Transform vrCameraTransform;   // 自分のVRゴーグル（頭）の場所
    private Transform vrLeftHandTransform;  // 自分の左手コントローラーの場所
    private Transform vrRightHandTransform; // 自分の右手コントローラーの場所


    void Update()
    {
        // 【超重要】もしこのキャラクターが「自分自身の体」じゃないなら、ここで処理を終了する！
        // （他人の動きはNGOの「NetworkTransform」などが自動で同期してくれるので、上書きしないようにします）
        if (!IsOwner) return;

        // --- 【VR機器（OVRCameraRig）がシーン内からまだ見つかっていない場合の検索処理】 ---
        // タイトル画面からゲーム画面へ切り替わった直後は、VRの準備が遅れて見つからないことがある（ロード待ち対策）
        if (vrCameraTransform == null || vrLeftHandTransform == null || vrRightHandTransform == null)
        {
            // 1. 画面のメインカメラ（＝自分が被っているVRゴーグル）を探して箱に入れる
            if (Camera.main != null)
            {
                vrCameraTransform = Camera.main.transform;
            }

            // 2. Meta Questの本体システム（OVRCameraRig）をシーン全体から探し出す
            OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null)
            {
                // 見つかったら、その中にある「左手のアンカー」と「右手のアンカー」の場所をそれぞれの箱にメモする
                vrLeftHandTransform = rig.leftHandAnchor;
                vrRightHandTransform = rig.rightHandAnchor;
            }

            // もし「頭」か「左手」のどちらかがまだ見つかっていなければ、今フレームの同期は諦めてここで処理を終了する
            // （次のフレームで、見つかるまで何度も探し直します）
            if (vrCameraTransform == null || vrLeftHandTransform == null) return;
        }

        // --- 【同期の本番：自分のVR機器のリアルタイムな動きを、アバターのパーツに毎フレーム強制コピーする】 ---

        // 1. 頭（ゴーグル）の同期
        if (networkHead != null)
        {
            networkHead.position = vrCameraTransform.position; // 自分の頭の位置をアバターにコピー
            networkHead.rotation = vrCameraTransform.rotation; // 自分の頭の向きをアバターにコピー
        }

        // 2. 左手コントローラーの同期
        if (networkLeftHand != null)
        {
            networkLeftHand.position = vrLeftHandTransform.position; // 自分の左手の位置をアバターにコピー
            networkLeftHand.rotation = vrLeftHandTransform.rotation; // 自分の左手の向きをアバターにコピー
        }

        // 3. 右手コントローラーの同期
        if (networkRightHand != null)
        {
            networkRightHand.position = vrRightHandTransform.position; // 自分の右手の位置をアバターにコピー
            networkRightHand.rotation = vrRightHandTransform.rotation; // 自分の右手の向きをアバターにコピー
        }
    }
}