using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;


/// <summary>
/// 役職設定
/// </summary>
public class RollSetting : MonoBehaviour
{
    //class
    public WrongPopUp wrongPopUp;

    //main
    public Button nextButton;
    public Button backButton;
    public GameObject wrongPopUpObj;
    public GameObject rollSettingObj;
    public GameObject roomSettingCanvas;
    public GameObject roomSelectCanvas;
    public List<int> NumList = new List<int>();
    public Button numberPlusButton;
    public Button numberMinusButton;
    public InputField titleText;

    //合計人数
    public int numLimit;
    public Text sumNumText;
    public int citizenCampNum;
    public Text citizenCampNumText;
    public int wolfCampNum;
    public Text wolfCampNumText;
    public int etcCampNum;
    public Text etcCampNumText;
    public Text differenceNumText;//差異表示テキスト

    //役職関係
    public RollNum citizen;
    public RollNum fortune;
    public RollNum knight;
    public RollNum psychic;
    public RollNum werewolf;
    public RollNum madman;

    public List<RollNum> rollNumList = new List<RollNum>();
    


    private void Start() {

        SetUpRollNum();
        UpdateRollNumAndNumLimitText();

        //表示設定
        sumNumText.text = numLimit.ToString();

        //ボタン設定
        nextButton.onClick.AddListener(NextButton);
        backButton.onClick.AddListener(BackButton);
        numberPlusButton.onClick.AddListener(SumNumberPlusButton);
        numberMinusButton.onClick.AddListener(SumNumberMinusButton);
    }


    /// <summary>
    /// RoomSettingCanvasへ移行する 人数が合わないならポップアップが出る
    /// </summary>
    public void NextButton() {

        if(numLimit > citizenCampNum + wolfCampNum) {
            wrongPopUpObj.SetActive(true);
            wrongPopUp.wrongText.text = "役職が少ないです。";

        } else if (numLimit < citizenCampNum + wolfCampNum) {
            wrongPopUpObj.SetActive(true);
            wrongPopUp.wrongText.text = "役職が多すぎます。";

        } else if (werewolf.num == 0) {
            wrongPopUpObj.SetActive(true);
            wrongPopUp.wrongText.text = "少なくとも狼が1匹以上必要です。";

        }else if (numLimit == citizenCampNum + wolfCampNum) {
            rollSettingObj.SetActive(false);
            roomSettingCanvas.SetActive(true);
            titleText.ActivateInputField();

            foreach(RollNum rollNum in rollNumList) {
                NumList.Add(rollNum.num);
            }
        }
    }

    /// <summary>
    /// RoomSelectCanvasへ
    /// </summary>
    public void BackButton() {
        rollSettingObj.SetActive(false);
        roomSelectCanvas.SetActive(true);
    }


    /// <summary>
    /// 役職のセットアップ
    /// </summary>
    private void SetUpRollNum() {

        foreach (ROLLTYPE rollType in Enum.GetValues(typeof(ROLLTYPE))) {
            if (rollType == ROLLTYPE.GM || rollType == ROLLTYPE.ETC) {
                continue;
            }

            switch (rollType) {
                case ROLLTYPE.市民:
                    citizen.num = 3;
                    citizen.numLimit = 8;
                    rollNumList.Add(citizen);
                    break;
                case ROLLTYPE.占い師:
                    fortune.num = 1;
                    fortune.numLimit = 2;
                    rollNumList.Add(fortune);
                    break;
                case ROLLTYPE.騎士:
                    knight.num = 1;
                    knight.numLimit = 2;
                    rollNumList.Add(knight);
                    break;
                case ROLLTYPE.霊能者:
                    psychic.num = 1;
                    psychic.numLimit = 2;
                    rollNumList.Add(psychic);
                    break;
                case ROLLTYPE.人狼:
                    werewolf.num = 2;
                    werewolf.numLimit = 2;
                    rollNumList.Add(werewolf);
                    break;
                case ROLLTYPE.狂人:
                    madman.num = 1;
                    madman.numLimit = 2;
                    rollNumList.Add(madman);
                    break;
            }
        }
    }

    /// <summary>
    /// テキストの更新
    /// </summary>
    private void UpdateRollNumAndNumLimitText() {
        foreach(RollNum rollNum in rollNumList) {
            rollNum.rollText.text = rollNum.num + " / " + rollNum.numLimit;
        }
    }
    /// <summary>
    /// 合計人数増加
    /// </summary>
    public void SumNumberPlusButton() {
        numLimit++;
        sumNumText.text = numLimit.ToString();
        if(numLimit == 15) {
            numberPlusButton.interactable = false;
        }else if(numLimit == 5) {
            numberMinusButton.interactable = true;
        }

        //役職ごとの人数制限増加
        switch(numLimit) {
            case 6:
                werewolf.numLimit = 2;
                break;
            case 8:
                fortune.numLimit = 2;
                knight.numLimit = 2;
                psychic.numLimit = 2;
                madman.numLimit = 2;
                break;
            case 12:
                werewolf.numLimit = 3;
                break;
            case 13:
                madman.numLimit = 3;
                break;
            case 14:
                fortune.numLimit = 3;
                knight.numLimit = 3;
                psychic.numLimit = 3;
                break;
        }


        citizenCampNumText.text = citizenCampNum + "人";
        wolfCampNumText.text = wolfCampNum + "人";
        SumNumber();
    }


    /// <summary>
    /// 合計人数減少
    /// </summary>
    public void SumNumberMinusButton() {
        numLimit--;
        citizenCampNum--;
        sumNumText.text = numLimit.ToString();
        if (numLimit == 4) {
            numberMinusButton.interactable = false;
        } else if (numLimit == 14) {
            numberPlusButton.interactable = true;
        }

        //役職ごとの人数制限
        switch (numLimit) {
            case 5:
                werewolf.numLimit = 1;
                break;
            case 7:
                fortune.numLimit = 1;
                knight.numLimit = 1;
                psychic.numLimit = 1;
                madman.numLimit = 1;
                break;
            case 11:
                werewolf.numLimit = 2;
                break;
            case 12:
                madman.numLimit = 2;
                break;
            case 13:
                fortune.numLimit = 2;
                knight.numLimit = 2;
                psychic.numLimit = 2;
                break;
        }


        citizenCampNumText.text = citizenCampNum + "人";
        wolfCampNumText.text = wolfCampNum + "人";
        
        SumNumber();

        if (citizen.num <= 0) {
            citizen.num = 0;
            citizen.rollText.text = citizen.num + "/" + numLimit;
        }
    }

    /// <summary>
    /// 市民、狼陣営の合計を計算
    /// </summary>
    public void SumNumber() {
        citizenCampNum = citizen.num + fortune.num + knight.num + psychic.num;
        wolfCampNum = werewolf.num + madman.num;
        citizenCampNumText.text = citizenCampNum + "人";
        wolfCampNumText.text = wolfCampNum + "人";


        //人数差異を表示
        if(numLimit > citizenCampNum + wolfCampNum) {
            //人数が不足している時
            differenceNumText.text = numLimit - (citizenCampNum + wolfCampNum) + "人不足";
        }else if(numLimit < citizenCampNum + wolfCampNum) {
            //人数が超過している時
            differenceNumText.text = - (numLimit - (citizenCampNum + wolfCampNum)) + "人超過";
        } else {
            //人数が指定通りの時
            differenceNumText.text = "";
        }

    }
}


