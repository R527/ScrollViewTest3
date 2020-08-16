using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// セッティングPopUpの設定
/// </summary>
public class SettingPopUp : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider seSlider;
    public Button maskButton;
    public Button backButton;
    public GameObject SettingPopUpObj;


    /// <summary>
    /// 音量調節の数値ををスタートで取得する
    /// </summary>
    private void Start() {
        bgmSlider.value = AudioManager.instance.bgmVolume;
        seSlider.value = AudioManager.instance.seVolume;
        bgmSlider.onValueChanged.AddListener(AudioManager.instance.SetBGM);
        seSlider.onValueChanged.AddListener(AudioManager.instance.SetSE);
        maskButton.onClick.AddListener(OnDestroy);
        backButton.onClick.AddListener(OnDestroy);
        //sliderButton.OnPointerUp
    }

    public void OnDestroy() {
        Destroy(SettingPopUpObj);
    }



}
