using UnityEngine;
using Unity.Netcode;

public class VRButtonLightEventNGO : NetworkBehaviour
{
    [Header("同期するライト（または部屋の親オブジェクト）")]
    public GameObject targetLight;

    private NetworkVariable<bool> isLightOn = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner
    );

    private void Start()
    {
        isLightOn.OnValueChanged += OnLightStateChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        isLightOn.OnValueChanged -= OnLightStateChanged;
    }

    // --- UnityEvent から呼び出す関数群 ---

    /// <summary>
    /// ライトのオン/オフをトグル切り替え
    /// </summary>
    public void ToggleLight()
    {
        ToggleLightServerRpc();
    }

    /// <summary>
    /// ライトを確実に ON にする
    /// </summary>
    public void TurnOnLight()
    {
        SetLightServerRpc(true);
    }

    /// <summary>
    /// ライトを確実に OFF にする
    /// </summary>
    public void TurnOffLight()
    {
        SetLightServerRpc(false);
    }

    // --- ServerRPC ＆ 状態同期 ---

    [ServerRpc(RequireOwnership = false)]
    private void ToggleLightServerRpc()
    {
        isLightOn.Value = !isLightOn.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetLightServerRpc(bool state)
    {
        isLightOn.Value = state;
    }

    private void OnLightStateChanged(bool previousValue, bool newValue)
    {
        if (targetLight != null)
        {
            targetLight.SetActive(newValue);
        }
    }
}