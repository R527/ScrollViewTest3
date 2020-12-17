using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 初心者ガイドPopUpの制御
/// </summary>
public class BegginerGuidePopUp : BasePopUP {


    //Button
    public List<Button> guideButtonList = new List<Button>();
    public Button maskBtn;


    //main
    public BegginerGuide begginerGuideObj;
    public string thatText;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        foreach(Button Obj in guideButtonList) {
            Obj.onClick.AddListener(GuideButton);
        }
        maskBtn.onClick.AddListener(DestroyPopUP);

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
        //OnDestroy();
        DestroyPopUP();
    }
    /// <summary>
    /// 押したボタンのテキストを読み取りGuideButton（）で利用
    /// </summary>
    /// <param name="obj"></param>
    public void PushJudge(GameObject obj) {
        thatText = obj.GetComponentInChildren<Text>().text;
    }

}
