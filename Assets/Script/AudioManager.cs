using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource loopAudioSource;
    [SerializeField] private AudioClip loopBGM; // またはSE

    // ボタンを押すたびに「再生 ⇔ 停止」を交互に切り替える関数
    public void ToggleLoopSound()
    {
        if (loopAudioSource == null || loopBGM == null) return;

        // 再生中なら止める、止まっていれば鳴らす
        if (loopAudioSource.isPlaying)
        {
            loopAudioSource.Stop();
        }
        else
        {
            loopAudioSource.clip = loopBGM;
            loopAudioSource.loop = true; // ループをオン
            loopAudioSource.Play();
        }
    }

    // 直接再生したい場合
    public void PlayLoopSound()
    {
        if (loopAudioSource == null || loopBGM == null) return;
        loopAudioSource.clip = loopBGM;
        loopAudioSource.loop = true;
        if (!loopAudioSource.isPlaying) loopAudioSource.Play();
    }

    // 直接止めたい場合
    public void StopLoopSound()
    {
        if (loopAudioSource != null) loopAudioSource.Stop();
    }
}