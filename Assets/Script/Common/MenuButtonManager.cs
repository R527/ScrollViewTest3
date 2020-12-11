using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//メニューボタンの制御はTitlescript
/// <summary>
/// メニューボタンに関するスクリプト
/// </summary>
public class MenuButtonManager : MonoBehaviour
{
    public Button beginnerButton;
    //public Button upDateButton;
    public Button settingButton;
    public Button backButton;//戻るボタン
    public Button maskButton;//マスクについてるボタン
    public Button rulesButton;//利用規約

    public GameObject settingPopUpObj;
    public GameObject begginerGuidePopUpObj;

    //利用規約関連
    private const string URL = "https://sun471044.wixsite.com/mysite";


    private void Start() {
        beginnerButton.onClick.AddListener(BegginerPopUp);
        //upDateButton.onClick.AddListener(UpDatePopUp);
        settingButton.onClick.AddListener(SettingPopUp);
        rulesButton.onClick.AddListener(SubmitRules);

        backButton.onClick.AddListener(OnDestroy);
        maskButton.onClick.AddListener(OnDestroy);
    }

    /// <summary>
    /// セッティングPopUpをインスタンス
    /// </summary>
    public void SettingPopUp() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        Instantiate(settingPopUpObj);
        OnDestroy();
    }

    //public void UpDatePopUp() {
    //    AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
    //}

    /// <summary>
    /// 初心者PopUpをインスタンス
    /// </summary>
    public void BegginerPopUp() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        Instantiate(begginerGuidePopUpObj);
        OnDestroy();
    }
    /// <summary>
    /// メニューPopUpを削除
    /// </summary>
    public void OnDestroy() {
        Destroy(gameObject);
    }

    void SubmitRules() {
        Application.OpenURL(URL);
    }


}
