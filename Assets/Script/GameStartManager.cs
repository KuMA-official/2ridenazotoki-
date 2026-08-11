using System.Collections;
using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    [SerializeField] private Transform vrRig;
    [SerializeField] private Transform[] startPoints;

    public void SelectDifficultyAndTeleport(int difficultyIndex)
    {
        StartCoroutine(TeleportRoutine(difficultyIndex));
    }

    private IEnumerator TeleportRoutine(int difficultyIndex)
    {
        yield return null; // XR Simulator対策（1フレーム待つ）

        if (vrRig != null && difficultyIndex >= 0 && difficultyIndex < startPoints.Length)
        {
            Transform targetPoint = startPoints[difficultyIndex];
            if (targetPoint != null)
            {
                // 1. 瞬間移動を邪魔しないように Character Controller を一時的に切る
                CharacterController cc = vrRig.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                // 2. 座標と向きをスタート地点へ瞬間移動
                vrRig.position = targetPoint.position;
                vrRig.rotation = targetPoint.rotation;

                // 3. 移動が終わったら Character Controller をONに戻す
                if (cc != null) cc.enabled = true;

                Debug.Log($"難易度 [{difficultyIndex}] へ安全に移動しました！");
            }
        }
    }
}