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
    
    private Transform activeHandTransform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
        
        // ★【重要】本棚は常に Kinematic（重力・物理の暴走防止）を固定にする
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void OnGrabHeavy()
    {
        isGrabbingHeavy = true;
        
        // 近くにある手を探す
        FindClosestHand();

        if (activeHandTransform != null)
        {
            grabOffsetX = transform.position.x - activeHandTransform.position.x;
        }
        else
        {
            Debug.LogError("[NetworkHeavyGrabbable] 本棚：手のアタッチ先が見つかりません！");
        }

        // サーバーへ所有権の変更をリクエスト（isKinematic は true のまま保持）
        GrabServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    private void FindClosestHand()
    {
        string[] handNames = new string[] { "RightHandAnchor", "RightControllerAnchor", "LeftHandAnchor", "LeftControllerAnchor" };
        float minDistance = float.MaxValue;
        activeHandTransform = null;

        var allTransforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var trans in allTransforms)
        {
            foreach (string name in handNames)
            {
                if (trans.name == name)
                {
                    float dist = Vector3.Distance(transform.position, trans.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        activeHandTransform = trans;
                    }
                }
            }
        }
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
    private void GrabServerRpc(ulong newOwnerId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);
        // ★ サーバー側でも Kinematic を維持
        rb.isKinematic = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseServerRpc()
    {
        // ★ 手放した時も Kinematic を維持（落下させない）
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isGrabbingHeavy && activeHandTransform != null)
        {
            // 安全装置（手が離れすぎたら強制リリース）
            float currentDistance = Vector3.Distance(transform.position, activeHandTransform.position);
            if (currentDistance > autoReleaseDistance)
            {
                OnReleaseHeavy();
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

            // Kinematic な Rigidbody でも MovePosition は正常に動作します
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