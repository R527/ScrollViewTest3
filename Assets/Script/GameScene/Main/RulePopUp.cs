using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulePopUp : BasePopUP {

    //Text
    public Text numLimitText;
    public Text fortuneText;
    public Text entryLevelText;//募集レベル
    public Text timeText;
    public Text votedText;

    
    // Start is called before the first frame update
     protected override void Start()
    {
        base.Start();
        //ルールを確認画面とゲームシーン専用のメニューに記入する
        numLimitText.text = RoomData.instance.numLimit.ToString();
        fortuneText.text = RoomData.instance.roomInfo.fortuneType.ToString();
        entryLevelText.text = RoomData.instance.roomInfo.roomSelection.ToString();
        timeText.text = RoomData.instance.roomInfo.mainTime + "/" + RoomData.instance.roomInfo.nightTime;
        votedText.text = RoomData.instance.roomInfo.openVoting.ToString();
    }



}
