using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using DG.Tweening;

/// <summary>
/// 音量を全てのシーンで引き継ぐ
/// </summary>
public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public AudioSource[] bgmSource;
    public AudioSource[] seSource;
    public int currentBgmNum;
    
    //音量調節
    public float bgmVolume;
    public float seVolume;

    //シングルトン
    public static AudioManager instance;

    public enum BGM_TYPE {
        TITLE,
        LOBBY,
        GAME,
        
        //Game中
        お昼,
        投票時間,
        夜の行動,
    }
    public enum SE_TYPE {
        OK,
        NG
    }
    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 音量調節BGM
    /// </summary>
    /// <param name="volume"></param>
    public void SetBGM(float volume) {
        audioMixer.SetFloat("BGMvol", volume);
        bgmVolume = ConvertVolume2db(volume);
        PlayerManager.instance.SetFloatForPlayerPrefs(bgmVolume, PlayerManager.ID_TYPE.bgmVolume);
    }

    /// <summary>
    /// 音量調節SE
    /// </summary>
    /// <param name="volume"></param>
    public void SetSE(float volume) {
        audioMixer.SetFloat("SEvol", volume);
        bgmVolume = ConvertVolume2db(volume);
        PlayerManager.instance.SetFloatForPlayerPrefs(seVolume, PlayerManager.ID_TYPE.seVolume);
    }

    /// <summary>
    /// 0-1の値を-80～0dB(デシベル)に変換(0にすると音量MAXになるので0.01fで止める) 
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    private float ConvertVolume2db(float volume) => 20f * Mathf.Log10(Mathf.Clamp(volume, 0.01f, 1f));

    /// <summary>
    /// BGMの配列を数字に変換して、流す
    /// </summary>
    /// <param name="bgmType"></param>
    public void PlayBGM(BGM_TYPE bgmType) {

        StopBGM();
        currentBgmNum = (int)bgmType;
        bgmSource[currentBgmNum].Play();
    }

    /// <summary>
    /// BGMを止めます
    /// </summary>
    public void StopBGM() {
        if (currentBgmNum != 99) {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(bgmSource[currentBgmNum].DOFade(0, 1.5f)).OnComplete(() => { 
                bgmSource[currentBgmNum].volume = 0.22f; 
            }); 
        }
    }

    /// <summary>
    /// SEの配列を数字に変換して、流す
    /// </summary>
    /// <param name="bgmType"></param>
    public void PlaySE(SE_TYPE seType) {
        seSource[(int)seType].Play();
    }


}
