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
        plusButton.rollSettingButton.onClick.AddListener(() => PushJudge(plusButton.applicant_Type));
        minusButton.rollSettingButton.onClick.AddListener(() => PushJudge(minusButton.applicant_Type));

        SetUpButton();
    }


    /// <summary>
    /// 押したボタンの情報をもとに数値を決定する
    /// </summary>
    /// <param name="rollSetting"></param>
    public void PushJudge(Applicant_Type applicant_Type) {

        plusButton.rollSettingButton.interactable = true;
        minusButton.rollSettingButton.interactable = true;

        //市民
        if (rollType == ROLLTYPE.市民) {
            if (applicant_Type == Applicant_Type.Plus && rollSetting.citizen.num != rollSetting.numLimit - 1) {
                rollSetting.citizen.num++;
                if (rollSetting.citizen.num >= rollSetting.numLimit - 1) {
                    plusButton.rollSettingButton.interactable = false;
                }
            } else if (applicant_Type == Applicant_Type.Minus && rollSetting.citizen.num != 0) {
                rollSetting.citizen.num--;
                if (rollSetting.citizen.num <= 0) {
                    minusButton.rollSettingButton.interactable = false;
                }
            }
            numText.text = rollSetting.citizen.num + " / " + (rollSetting.numLimit - 1);

        //市民以外
        } else {
            //変数をロールごとに変更する
            SwitchValue();

            //市民以外の処理
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
        }

        //合計をテキスト表示
        rollSetting.SumNumber();
    }


    /// <summary>
    /// 変数をロールごとに変更する
    /// </summary>
    private void SwitchValue() {
        switch (rollType) {
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
