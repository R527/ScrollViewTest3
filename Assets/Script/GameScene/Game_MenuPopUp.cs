using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Game_MenuPopUp : MonoBehaviour
{
    public Button beginnerButton;
    public Button upDateButton;
    public Button settingButton;
    public Button ruleButton;
    public Button backButton;//戻るボタン
    public Button maskButton;//マスクについてるボタン

    public GameObject settingPopUpObj;
    public GameObject begginerGuidePopUpObj;
    public GameObject rulePopUpObj;

    public Transform objTran;
    private void Start() {
        objTran = GameObject.FindGameObjectWithTag("GameCanvas").GetComponent<Transform>();
        beginnerButton.onClick.AddListener(BegginerPopUp);
        upDateButton.onClick.AddListener(UpDatePopUp);
        settingButton.onClick.AddListener(SettingPopUp);
        ruleButton.onClick.AddListener(rulePopUp);
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

    public void UpDatePopUp() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
    }

    /// <summary>
    /// 初心者PopUpをインスタンス
    /// </summary>
    public void BegginerPopUp() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        Instantiate(begginerGuidePopUpObj);
        OnDestroy();
    }

    public void rulePopUp() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        GraphicRaycastersManager rayCastManagerObj = GameObject.FindGameObjectWithTag("RaycastersManager").GetComponent<GraphicRaycastersManager>();
        rayCastManagerObj.SwitchGraphicRaycasters(false);
        Instantiate(rulePopUpObj, objTran,false);
        OnDestroy();
    }
    /// <summary>
    /// メニューPopUpを削除
    /// </summary>
    public void OnDestroy() {
        Destroy(gameObject);
    }
}
