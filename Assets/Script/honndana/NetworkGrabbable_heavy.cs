using System.Collections;
using Unity.Netcode;     
using UnityEngine;     

[RequireComponent(typeof(Rigidbody))]
public class NetworkHeavyGrabbable : NetworkBehaviour
{
    [Header("Heavy Settings (本棚専用)")]
    [Tooltip("追従するスピード。低いほど重く（遅く）感じます")]
    public float followSpeed = 2.0f;
    [Tooltip("振動を発生させる最小の移動速度")]
    public float movementThreshold = 0.01f;
    
    [Header("Safety Settings (バグ防止用)")]
    [Tooltip("安全装置：手と本棚がこの距離(m)以上離れたら強制的に手を離す")]
    public float autoReleaseDistance = 1.5f;

    private Rigidbody rb;
    private bool isGrabbingHeavy = false;
    private Vector3 lastPosition;
    private float grabOffsetX = 0f;
    
    // 【変更】右手固定ではなく、今掴んでいる手（アクティブな手）を保存する
    private Transform activeHandTransform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    public void OnGrabHeavy()
    {
        isGrabbingHeavy = true;
        
        // 【修正1】右手と左手の両方を探す
        GameObject rightHand = GameObject.Find("RightHandAnchor");
        if (rightHand == null) rightHand = GameObject.Find("RightControllerAnchor");
        
        GameObject leftHand = GameObject.Find("LeftHandAnchor");
        if (leftHand == null) leftHand = GameObject.Find("LeftControllerAnchor");

        float minDistance = float.MaxValue;
        activeHandTransform = null;

        // 右手と左手のうち、本棚に近い方（触っている方）を自動で「掴んでいる手」にする
        if (rightHand != null)
        {
            float dist = Vector3.Distance(transform.position, rightHand.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                activeHandTransform = rightHand.transform;
            }
        }

        if (leftHand != null)
        {
            float dist = Vector3.Distance(transform.position, leftHand.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                activeHandTransform = leftHand.transform;
            }
        }

        if (activeHandTransform != null)
        {
            grabOffsetX = transform.position.x - activeHandTransform.position.x;
        }
        else
        {
            Debug.LogError("本棚スクリプト：両手のオブジェクトが見つかりません！");
        }

        StartCoroutine(ForcePhysicsRoutine());
        GrabServerRpc(NetworkManager.Singleton.LocalClientId, false);
    }

    private IEnumerator ForcePhysicsRoutine()
    {
        yield return null; 
        rb.isKinematic = false; 
    }

    public void OnReleaseHeavy()
    {
        isGrabbingHeavy = false;
        activeHandTransform = null;
        grabOffsetX = 0f;
        ReleaseServerRpc();

        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GrabServerRpc(ulong newOwnerId, bool isKinematicState)
    {
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);
        rb.isKinematic = isKinematicState;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseServerRpc()
    {
        rb.isKinematic = false;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isGrabbingHeavy && activeHandTransform != null)
        {
            // 【修正2】安全装置（バグ防止）
            // 何らかのエラーで「離した判定」が来ないままプレイヤーが後ろに下がった場合、
            // 本棚が強引に引っ張られて荒ぶる（ノイズが出る）のを防ぐために強制解除する。
            float currentDistance = Vector3.Distance(transform.position, activeHandTransform.position);
            if (currentDistance > autoReleaseDistance)
            {
                OnReleaseHeavy(); // 強制的に手を離す！
                return;
            }

            // 本棚の移動処理
            Vector3 handWorldPosition = activeHandTransform.position;
            float targetX = handWorldPosition.x + grabOffsetX;

            Vector3 targetPosition = new Vector3(
                targetX, 
                transform.position.y, 
                transform.position.z
            );

            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * followSpeed);
            rb.MovePosition(newPosition);

            float moveSpeed = (transform.position - lastPosition).magnitude / Time.fixedDeltaTime;

            if (moveSpeed > movementThreshold)
            {
                OVRInput.SetControllerVibration(0.1f, 0.5f, OVRInput.Controller.RTouch);
                OVRInput.SetControllerVibration(0.1f, 0.5f, OVRInput.Controller.LTouch);
            }
            else
            {
                OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
                OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
            }
        }

        lastPosition = transform.position;
    }
}