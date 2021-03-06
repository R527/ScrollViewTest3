using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// RollSettingにあるボタン情報を取得する
/// </summary>
public class RollSettingButton : MonoBehaviour
{
    /// <summary>
    /// 増減専用のenum
    /// </summary>
    public enum Applicant_Type {
        Plus,
        Minus
    }
    
    
    //class
    public RollSetting rollSetting;
    //main
    public ROLLTYPE rollType;
    public Text numText;

    private int switchNum;
    private int switchNumLimit;


    [System.Serializable]
    public class RollNumButton {
        public Button rollSettingButton;
        public Applicant_Type applicant_Type;
    }

    public RollNumButton plusButton;
    public RollNumButton minusButton;

    private void Start() {
        plusButton.rollSettingButton.onClick.AddListener(() => SetRollNum(plusButton.applicant_Type));
        minusButton.rollSettingButton.onClick.AddListener(() => SetRollNum(minusButton.applicant_Type));

        SetUpButton();
        SwitchValue();
        rollSetting.numberPlusButton.onClick.AddListener(CheckLimit);
        rollSetting.numberMinusButton.onClick.AddListener(CheckLimit);
    }

    /// <summary>
    /// 参加人数の設定のメソッドが呼ばれてからこのメソッドは呼ばれる（AddListener二つ目
    /// 各ロールごとの人数制限をチェックしてボタンのONOFFを決定する
    /// </summary>
    private void CheckLimit() {
        //今までのSwithNumLimit
        int oldNumLimit = switchNumLimit;

        SwitchValue();

        if (switchNumLimit < oldNumLimit) {
            //SwithNumLimitがoldNumLimitより低い場合
            if (switchNum >= switchNumLimit) {
                //今の設定人数と最大人数が食い違っている時の処理
                plusButton.rollSettingButton.interactable = false;
                switchNum = switchNumLimit;
                ReturnValue();
            }

        } else if(switchNumLimit > oldNumLimit) {
            //SwithNumLimitがoldNumLimitよりも高い場合
            plusButton.rollSettingButton.interactable = true;
        }

        numText.text = switchNum + " / " + switchNumLimit;
    }


　　/// <summary>
  /// 
  /// </summary>
  /// <param name="applicant_Type">ボタンのプラスマイナスを判定する</param>
    public void SetRollNum(Applicant_Type applicant_Type) {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);

        plusButton.rollSettingButton.interactable = true;
        minusButton.rollSettingButton.interactable = true;

        if (applicant_Type == Applicant_Type.Plus && switchNum != switchNumLimit) {
            switchNum++;
            if (switchNum >= switchNumLimit) {
                plusButton.rollSettingButton.interactable = false;
            }
        } else if (applicant_Type == Applicant_Type.Minus && switchNum != 0) {
            switchNum--;
            if (switchNum <= 0) {
                minusButton.rollSettingButton.interactable = false;
            }
        }
        numText.text = switchNum + " / " + switchNumLimit;

        //変更された値を元の変数に戻す
        ReturnValue();

        //合計をテキスト表示
        rollSetting.SumNumber();
    }


    /// <summary>
    /// 変数をロールごとに変更する
    /// </summary>
    private void SwitchValue() {
        switch (rollType) {
            case ROLLTYPE.市民:
                switchNum = rollSetting.citizen.num;
                switchNumLimit = rollSetting.numLimit - 1;
                break;
            case ROLLTYPE.占い師:
                switchNum = rollSetting.fortune.num;
                switchNumLimit = rollSetting.fortune.numLimit;
                break;
            case ROLLTYPE.騎士:
                switchNum = rollSetting.knight.num;
                switchNumLimit = rollSetting.knight.numLimit;
                break;
            case ROLLTYPE.霊能者:
                switchNum = rollSetting.psychic.num;
                switchNumLimit = rollSetting.psychic.numLimit;
                break;
            case ROLLTYPE.人狼:
                switchNum = rollSetting.werewolf.num;
                switchNumLimit = rollSetting.werewolf.numLimit;
                break;
            case ROLLTYPE.狂人:
                switchNum = rollSetting.madman.num;
                switchNumLimit = rollSetting.madman.numLimit;
                break;
        }
    }

    /// <summary>
    /// 変更された変数を返す
    /// </summary>
    private void ReturnValue() {
        switch (rollType) {
            case ROLLTYPE.市民:
                rollSetting.citizen.num = switchNum;
                break;
            case ROLLTYPE.占い師:
                rollSetting.fortune.num = switchNum;
                break;
            case ROLLTYPE.騎士:
                rollSetting.knight.num = switchNum;
                break;
            case ROLLTYPE.霊能者:
                rollSetting.psychic.num = switchNum;
                break;
            case ROLLTYPE.人狼:
                rollSetting.werewolf.num = switchNum;
                break;
            case ROLLTYPE.狂人:
                rollSetting.madman.num = switchNum;
                break;
        }
    }

    /// <summary>
    /// セットアップ
    /// 無効にするべきボタンを無効にする
    /// </summary>
    private void SetUpButton() {
        foreach (RollNum rollNumObj in rollSetting.rollNumList) {

            //ボタンの役職と一致して、人数制限と一致しているもしくは０の場合は無効にする
            if (rollNumObj.rollType == rollType) {
                if (rollNumObj.num != rollNumObj.numLimit) {
                    continue;
                }else if (rollNumObj.num == rollNumObj.numLimit) {
                    plusButton.rollSettingButton.interactable = false;
                }else if (rollNumObj.num == 0) {
                    minusButton.rollSettingButton.interactable = false;
                }
            }
        }
    }

}
