using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 参加確認画面にコンポーネントされている
/// スタートの時点でルールと役職を記載する
/// </summary>
public class RulePopUp : MonoBehaviour {

    //Text
    public Text numLimitText;
    public Text fortuneText;
    public Text entryLevelText;//募集レベル
    public Text timeText;
    public Text votedText;

    public GameObject rollButtonContent;
    public RollExplanationButtonPrefab rollExplanationButtonPrefab;

    public Text confirmationRollListText;

    public Button maskBtn;
    public Button closeBtn;


    // Start is called before the first frame update
    void Start()
    {
        //役職一覧記載
        DisplayRollList();

        //ルールを確認画面とゲームシーン専用のメニューに記入する
        numLimitText.text = RoomData.instance.numLimit.ToString();
        fortuneText.text = RoomData.instance.roomInfo.fortuneType.ToString();
        entryLevelText.text = RoomData.instance.roomInfo.roomSelection.ToString();
        timeText.text = RoomData.instance.roomInfo.mainTime + "/" + RoomData.instance.roomInfo.nightTime;
        votedText.text = RoomData.instance.roomInfo.openVoting.ToString();

        maskBtn.onClick.AddListener(DestroyPopUp);
        closeBtn.onClick.AddListener(DestroyPopUp);
    }

    void DestroyPopUp() {
        GraphicRaycastersManager rayCastManagerObj = GameObject.FindGameObjectWithTag("RaycastersManager").GetComponent<GraphicRaycastersManager>();
        rayCastManagerObj.SwitchGraphicRaycasters(true);
        Destroy(gameObject);
    }

    /// <summary>
    /// 役職のテキスト表示
    /// </summary>
    /// <param name="numList"></param>
    private void DisplayRollList() {
        confirmationRollListText.text = string.Empty;
        //役職テキスト
        int num = 0;
        for (int i = 0; i < RoomData.instance.rollList.Count; i++) {
            if (RoomData.instance.rollList[i] != 0) {
                string emptyStr = "";
                num++;
                if (num != 0 && num % 3 == 1) {
                    emptyStr = "\r\n";
                }
                string str = ((ROLLTYPE)i) + ": " + RoomData.instance.rollList[i];
                confirmationRollListText.text += emptyStr + str;
            }
        }
    }

    /// <summary>
    /// GameManagerより役職リストをもらって役職説明ボタンを作る準備をする
    /// </summary>
    /// <param name="rollTypeList"></param>
    public void RollExplanationSetUp(List<ROLLTYPE> rollTypeList) {
        for (int i = 0; i < rollTypeList.Count; i++) {
            RollExplanationButtonPrefab Obj = Instantiate(rollExplanationButtonPrefab, rollButtonContent.transform, false);
            Obj.rollText.text = rollTypeList[i].ToString();
        }
    }
}
