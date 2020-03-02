using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームシーン専用のMenuButton管理
/// </summary>
public class GameMenuButtonManager : MonoBehaviour {

    public Button beginnerButton;
    public Button upDateButton;
    public Button settingButton;
    public Button ruleButton;
    public Button backButton;//戻るボタン
    public Button maskButton;//マスクについてるボタン

    public GameObject settingPopUpObj;
    public GameObject begginerGuidePopUpObj;
    public GameObject rulePopUpObj;

    private void Start() {
        beginnerButton.onClick.AddListener(BegginerPopUp);
        //upDateButton.onClick.AddListener();
        settingButton.onClick.AddListener(SettingPopUp);
        ruleButton.onClick.AddListener(RulePopUp);

        backButton.onClick.AddListener(OnDestroy);
        maskButton.onClick.AddListener(OnDestroy);
    }

    /// <summary>
    /// セッティングPopUpをインスタンス
    /// </summary>
    public void SettingPopUp() {
        Instantiate(settingPopUpObj);
        OnDestroy();
    }


    /// <summary>
    /// 初心者PopUpをインスタンス
    /// </summary>
    public void BegginerPopUp() {
        Instantiate(begginerGuidePopUpObj);
        OnDestroy();
    }
    /// <summary>
    /// ルールPopUpwoインスタンス
    /// </summary>
    public void RulePopUp() {
        Instantiate(rulePopUpObj);
        OnDestroy();
    }
    /// <summary>
    /// メニューPopUpを削除
    /// </summary>
    public void OnDestroy() {
        Destroy(gameObject);
    }



}
