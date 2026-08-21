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
        Debug.Log("★ToggleLoopSoundが呼ばれました！");

        if (loopAudioSource == null || loopBGM == null)
        {
            Debug.LogError("★AudioSource か BGM が Null です！");
            return;
        }

        if (loopAudioSource.isPlaying)
        {
            loopAudioSource.Stop();
            Debug.Log("★音を停止しました");
        }
        else
        {
            loopAudioSource.clip = loopBGM;
            loopAudioSource.loop = true;
            loopAudioSource.Play();
            Debug.Log("★音を再生しました");
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