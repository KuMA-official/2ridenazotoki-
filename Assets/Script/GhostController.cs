using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GhostController : NetworkBehaviour
{
    [Header("幽霊専用：ヒント本オブジェクト")]
    [SerializeField] private GameObject ghostBook;

    [Header("幽霊の見た目（人間から隠すモデル）")]
    [SerializeField] private Renderer[] ghostRenderers;

    [Header("実体化（物理干渉）設定")]
    [SerializeField] private float physicalDuration = 5.0f;

    // 手の全センサー（Collider）と掴み機能（Interactor）のキャッシュリスト
    private List<Behaviour> cachedInteractors = new List<Behaviour>();
    private List<Collider> cachedColliders = new List<Collider>();

    private bool isPhysical = false;
    private Coroutine physicalCoroutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // 1. 手の中にある掴み系機能だけを自動検出（視点操作系は除外）
            CacheAllHandInteractors();

            // 2. スタート時は非実体化（掴み機能をOFF）
            SetPhysicalState(false);
        }
        else
        {
            // 人間の画面では幽霊の見た目を消す
            if (ghostRenderers != null)
            {
                foreach (var renderer in ghostRenderers)
                {
                    if (renderer != null) renderer.enabled = false;
                }
            }
        }

        if (ghostBook != null) ghostBook.SetActive(false);
    }

    /// <summary>
    /// LeftInteractions / RightInteractions 配下から「掴み機能」だけを抽出する
    /// </summary>
    private void CacheAllHandInteractors()
    {
        cachedInteractors.Clear();
        cachedColliders.Clear();

        var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj.name == "LeftInteractions" || obj.name == "RightInteractions")
            {
                // 配下にあるすべての Collider（掴み判定センサー）を取得
                var cols = obj.GetComponentsInChildren<Collider>(true);
                cachedColliders.AddRange(cols);

                // 配下にある Interactor スクリプトを取得
                var behaviours = obj.GetComponentsInChildren<Behaviour>(true);
                foreach (var b in behaviours)
                {
                    if (b != null)
                    {
                        string typeName = b.GetType().Name;
                        string objName = b.gameObject.name;

                        // "Interactor" を含むが、視点・移動系（Locomotion / Turn / Rotate / Scroll）は除外する
                        bool isInteractor = typeName.Contains("Interactor");
                        bool isLocomotion = typeName.Contains("Locomotion") || typeName.Contains("Turn") || typeName.Contains("Rotate")
                                         || objName.Contains("Locomotion") || objName.Contains("Turn") || objName.Contains("Rotate") || objName.Contains("Scroll");

                        if (isInteractor && !isLocomotion)
                        {
                            cachedInteractors.Add(b);
                        }
                    }
                }
            }
        }

        Debug.Log($"[GhostController] 検出完了: 掴み用センサー {cachedColliders.Count}個 / 掴み機能 {cachedInteractors.Count}個（※視点操作は保護済み）");
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Bボタン：本
        if (OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.B))
        {
            ToggleGhostBook();
        }

        // Aボタン：実体化
        if (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.A))
        {
            if (isPhysical) return;

            if (physicalCoroutine != null) StopCoroutine(physicalCoroutine);
            physicalCoroutine = StartCoroutine(ActivatePhysicalMode());
        }
    }

    private void ToggleGhostBook()
    {
        if (ghostBook != null) ghostBook.SetActive(!ghostBook.activeSelf);
    }

    private IEnumerator ActivatePhysicalMode()
    {
        isPhysical = true;
        SetPhysicalState(true); // ★実体化：掴み機能をON！
        Debug.Log($"幽霊が実体化しました！（{physicalDuration}秒間触れます）");

        yield return new WaitForSeconds(physicalDuration);

        SetPhysicalState(false); // ★非実体化：掴み機能をOFF！
        isPhysical = false;
        Debug.Log("幽霊の実体化が解除されました。");
    }

    private void SetPhysicalState(bool state)
    {
        // 1. 掴み用コライダー（センサー）のON/OFF
        foreach (var col in cachedColliders)
        {
            if (col != null) col.enabled = state;
        }

        // 2. 掴み系 Interactor スクリプトのON/OFF
        foreach (var interactor in cachedInteractors)
        {
            if (interactor != null) interactor.enabled = state;
        }
    }
}