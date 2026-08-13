using UnityEngine;

public class ButtonSoundTrigger : MonoBehaviour
{
    private AudioSource audioSource;
    private bool hasPlayed = false;

    // 音が鳴る沈み込みの閾値（初期位置からどのくらい下に下がったら鳴らすか）
    [SerializeField] private float pressThreshold = 0.03f;

    private float startY;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // ゲーム開始時のローカルY座標を記憶
        startY = transform.localPosition.y;
    }

    void Update()
    {
        Vector3 currentPos = transform.localPosition;
        currentPos.y = Mathf.Clamp(currentPos.y, startY - 0.04f, startY);
        transform.localPosition = currentPos;

        // 現在の高さが、初期位置より pressThreshold 以上下がっているかチェック
        float currentY = transform.localPosition.y;

        if (startY - currentY >= pressThreshold)
        {
            if (!hasPlayed)
            {
                if (audioSource != null)
                {
                    audioSource.Play();
                }
                hasPlayed = true; // 連打・連続再生を防止
            }
        }
        else
        {
            // ボタンが元の高さ近くまで戻ったら、再度押せるようにリセット
            if (startY - currentY < pressThreshold * 0.5f)
            {
                hasPlayed = false;
            }
        }
    }
}