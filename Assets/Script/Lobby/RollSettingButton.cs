using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// RollSettingにあるボタン情報を取得する
/// </summary>
public class RollSettingButton : MonoBehaviour
{
    //class
    public RollSetting rollSetting;

    //main
    public ROLLTYPE rollType;
    public Text numText;
    [Header ("trueならプラス")]
    public bool plusOrMinus;
    public int num;
    public int numLimit;

    public Button rollSettingButton;

    private void Start() {
        rollSetting = GameObject.Find("RoomSetting").GetComponent<RollSetting>();
        rollSettingButton.onClick.AddListener(() => PushJudge(rollSetting));
        numText.text = num + " / " + numLimit;
    }

    /// <summary>
    /// 押したボタンの情報をもとに数値を決定する
    /// </summary>
    /// <param name="rollSetting"></param>
    public void PushJudge(RollSetting rollSetting) {
        //ロールごとに人数制限を取得する
        switch (rollType) {
            case ROLLTYPE.市民:
                numLimit = rollSetting.numLimit - 1;
                num = rollSetting.citizenNum;
                break;
            case ROLLTYPE.占い師:
                numLimit = rollSetting.fortuneNumLimit;
                    break;
            case ROLLTYPE.騎士:
                numLimit = rollSetting.knightNumLimit;
                break;
            case ROLLTYPE.霊能者:
                numLimit = rollSetting.psychicNumLimit;
                break;
            case ROLLTYPE.人狼:
                numLimit = rollSetting.werewolfNumLimit;
                break;
            case ROLLTYPE.狂人:
                numLimit = rollSetting.werewolfNumLimit;
                break;
        }
        //人数制限をもとに数値の増減を決定する
        //プラスマイナスの判定
        if (plusOrMinus && num != numLimit) {
            num++;
        } else if(!plusOrMinus && num != 0) {
            num--;
        }

        //text表示
        numText.text = num + " / " + numLimit; 
    }
}
