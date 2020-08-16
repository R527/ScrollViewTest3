using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 参加確認画面にコンポーネントされている
/// スタートの時点でルールと役職を記載する
/// </summary>
public class RulePopUp : BasePopUP {

    //Text
    public Text numLimitText;
    public Text fortuneText;
    public Text entryLevelText;//募集レベル
    public Text timeText;
    public Text votedText;

    public Text confirmationRollListText;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //役職一覧記載


        //ルールを確認画面とゲームシーン専用のメニューに記入する
        numLimitText.text = RoomData.instance.numLimit.ToString();
        fortuneText.text = RoomData.instance.roomInfo.fortuneType.ToString();
        entryLevelText.text = RoomData.instance.roomInfo.roomSelection.ToString();
        timeText.text = RoomData.instance.roomInfo.mainTime + "/" + RoomData.instance.roomInfo.nightTime;
        votedText.text = RoomData.instance.roomInfo.openVoting.ToString();

        DisplayRollList();
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

}
