using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 初心者ガイドPopUpの制御
/// </summary>
public class BegginerGuidePopUp : MonoBehaviour
{


    //Button
    public List<Button> guideButtonList = new List<Button>();
    public List<Button> destroyButtonList = new List<Button>();


    //main
    public BegginerGuide begginerGuideObj;
    public string thatText;


    // Start is called before the first frame update
    void Start()
    {
        foreach(Button Obj in guideButtonList) {
            Obj.onClick.AddListener(GuideButton);
        }
        foreach (Button Obj in destroyButtonList) {
            Obj.onClick.AddListener(OnDestroy);
        }

    }

    /// <summary>
    /// それぞれのボタンテキストに応じてGuideを出す
    /// </summary>
    public void GuideButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        BegginerGuide obj = Instantiate(begginerGuideObj);
        switch (thatText) {
            case "人狼とは":
                obj.guideType = GUIDE_TYPE.人狼の遊び方;
                obj.guideTitleText.text = GUIDE_TYPE.人狼の遊び方.ToString();
                break;
            case "操作方法":
                obj.guideType = GUIDE_TYPE.操作方法;
                obj.guideTitleText.text = GUIDE_TYPE.操作方法.ToString();
                break;
            case "用語説明":
                obj.guideType = GUIDE_TYPE.用語説明;
                obj.guideTitleText.text = GUIDE_TYPE.用語説明.ToString();
                break;
            case "禁止事項":
                obj.guideType = GUIDE_TYPE.禁止事項;
                obj.guideTitleText.text = GUIDE_TYPE.禁止事項.ToString();
                break;
            case "役職紹介":
                obj.guideType = GUIDE_TYPE.役職紹介;
                obj.guideTitleText.text = GUIDE_TYPE.役職紹介.ToString();
                break;
        }
        OnDestroy();
    }
    /// <summary>
    /// 押したボタンのテキストを読み取りGuideButton（）で利用
    /// </summary>
    /// <param name="obj"></param>
    public void PushJudge(GameObject obj) {
        thatText = obj.GetComponentInChildren<Text>().text;
    }
    /// <summary>
    /// このスクリプトがついているGameObjectを削除
    /// </summary>
    public void OnDestroy() {
        Destroy(gameObject);
    }
}
