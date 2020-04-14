using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
    public Button citizenPlusButton;
    public Button citizenMinusButton;
    public Button fortunePlusButton;
    public Button fortuneMinusButton;
    public Button knightPlusButton;
    public Button knightMinusButton;
    public Button psychicPlusButton;
    public Button psychicMinusButton;
    public Button werewolfPlusButton;
    public Button werewolfMinusButton;
    public Button madmanPlusButton;
    public Button madmanMinusButton;
    private Button nullPlusButton;
    private Button nullMinusButton;
    private Text nullText;
    private int nullNum;
    private int nullLimit;
    public int numLimit;
    public Text sumNumText;
    public int citizenCampNum;
    public Text citizenCampNumText;
    public int wolfCampNum;
    public Text wolfCampNumText;
    public int etcCampNum;
    public Text etcCampNumText;
    public Text citizenText;
    public int citizenNum;
    public int citizenNumLimit;
    public Text fortuneText;
    public int fortuneNum;
    public int fortuneNumLimit;
    public Text knightText;
    public int knightNum;
    public int knightNumLimit;
    public Text psychicText;
    public int psychicNum;
    public int psychicNumLimit;
    public Text werewolfText;
    public int werewolfNum;
    public int werewolfNumLimit;
    public Text madmanText;
    public int madmanNum;
    public int madmanNumLimit;
    public InputField titleText;


    private void Start() {
        //int[] joinNum = new int[3] {
        //    citizenCampNum,
        //    wolfCampNum,
        //    etcCampNum
        //};
        //SumNumber(joinNum);

        //表示設定
        sumNumText.text = numLimit.ToString();
        werewolfPlusButton.interactable = false;
        citizenCampNumText.text = citizenCampNum + "人";
        wolfCampNumText.text = wolfCampNum + "人";
        citizenText.text = citizenNum + "/" + (numLimit - 1);
        fortuneText.text = fortuneNum + "/" + fortuneNumLimit;
        knightText.text = knightNum + "/" + knightNumLimit;
        psychicText.text = psychicNum + "/" + psychicNumLimit;
        werewolfText.text = werewolfNum + "/" + werewolfNumLimit;
        madmanText.text = madmanNum + "/" + madmanNumLimit;

        //ボタン設定
        nextButton.onClick.AddListener(NextButton);
        backButton.onClick.AddListener(BackButton);
        numberPlusButton.onClick.AddListener(SumNumberPlusButton);
        numberMinusButton.onClick.AddListener(SumNumberMinusButton);
    }

    //public void SumNumber(int [] nums) {
    //    int total = 0;
    //    // sumNum = citizenCampNum + wolfCampNum + etcCampNum;
    //    for (int i = 0; i < nums.Length; i++) {
    //        total += nums[i];
    //    }
    //    sumNumText.text = sumNum.ToString();
    //}

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
        } else if (werewolfNum　== 0) {
            wrongPopUpObj.SetActive(true);
            wrongPopUp.wrongText.text = "少なくとも狼が1匹以上必要です。";
        }else if (numLimit == citizenCampNum + wolfCampNum) {
            rollSettingObj.SetActive(false);
            roomSettingCanvas.SetActive(true);
            titleText.ActivateInputField();
            NumList.Add(citizenNum);
            NumList.Add(fortuneNum);
            NumList.Add(knightNum);
            NumList.Add(psychicNum);
            NumList.Add(werewolfNum);
            NumList.Add(madmanNum);
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
                werewolfNumLimit = 2;
                werewolfPlusButton.interactable = true;
                break;
            case 8:
                fortuneNumLimit = 2;
                knightNumLimit = 2;
                psychicNumLimit = 2;
                madmanNumLimit = 2;
                fortunePlusButton.interactable = true;
                knightPlusButton.interactable = true;
                psychicPlusButton.interactable = true;
                madmanPlusButton.interactable = true;
                break;
            case 12:
                werewolfNumLimit = 3;
                werewolfPlusButton.interactable = true;
                break;
            case 13:
                madmanNumLimit = 3;
                madmanPlusButton.interactable = true;
                break;
            case 14:
                fortuneNumLimit = 3;
                knightNumLimit = 3;
                psychicNumLimit = 3;
                fortunePlusButton.interactable = true;
                knightPlusButton.interactable = true;
                psychicPlusButton.interactable = true;
                break;
        }
        citizenText.text = citizenNum + "/" + (numLimit - 1);
        fortuneText.text = fortuneNum + "/" + fortuneNumLimit;
        knightText.text = knightNum + "/" + knightNumLimit;
        psychicText.text = psychicNum + "/" + psychicNumLimit;
        werewolfText.text = werewolfNum + "/" + werewolfNumLimit;
        madmanText.text = madmanNum + "/" + madmanNumLimit;
        citizenCampNumText.text = citizenCampNum + "人";
        wolfCampNumText.text = wolfCampNum + "人";
        SumNumber();
        //CitizenNum();
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
                werewolfNumLimit = 1;
                break;
            case 7:
                fortuneNumLimit = 1;
                knightNumLimit = 1;
                psychicNumLimit = 1;
                madmanNumLimit = 1;
                break;
            case 11:
                werewolfNumLimit = 2;
                break;
            case 12:
                madmanNumLimit = 2;
                break;
            case 13:
                fortuneNumLimit = 2;
                knightNumLimit = 2;
                psychicNumLimit = 2;
                break;
        }
        //今の設定人数と最大人数が食い違っている時の処理
        if(fortuneNum > fortuneNumLimit) {
            fortuneNum = fortuneNumLimit;
            fortunePlusButton.interactable = false;
        }
        if (knightNum > knightNumLimit) {
            knightNum = knightNumLimit;
            knightPlusButton.interactable = false;
        }
        if (psychicNum > psychicNumLimit) {
            psychicNum = psychicNumLimit;
            psychicPlusButton.interactable = false;
        }
        if (werewolfNum > werewolfNumLimit) {
            werewolfNum = werewolfNumLimit;
            wolfCampNum--;
            werewolfPlusButton.interactable = false;
        }
        if (madmanNum > madmanNumLimit) {
            madmanNum = madmanNumLimit;
            wolfCampNum--;
            madmanPlusButton.interactable = false;
        }
        citizenText.text = citizenNum + "/" + (numLimit - 1);
        fortuneText.text = fortuneNum + "/" + fortuneNumLimit;
        knightText.text = knightNum + "/" + knightNumLimit;
        psychicText.text = psychicNum + "/" + psychicNumLimit;
        werewolfText.text = werewolfNum + "/" + werewolfNumLimit;
        madmanText.text = madmanNum + "/" + madmanNumLimit;
        citizenCampNumText.text = citizenCampNum + "人";
        wolfCampNumText.text = wolfCampNum + "人";
        SumNumber();
        //CitizenNum();
        if (citizenNum <= 0) {
            citizenNum = 0;
            citizenText.text = citizenNum + "/" + numLimit;
        }
    }

    /// <summary>
    /// 市民、狼陣営の合計を計算
    /// </summary>
    public void SumNumber() {
        citizenCampNum = citizenNum + fortuneNum + knightNum + psychicNum;
        wolfCampNum = werewolfNum + madmanNum;
        citizenCampNumText.text = citizenCampNum + "人";
        wolfCampNumText.text = wolfCampNum + "人";
    }
    ///// <summary>
    ///// 市民陣営プラスボタンの制御
    ///// </summary>
    //public void CitizenCampPlus() {
    //    if(citizenNum == 0) {
    //        citizenCampNum++;
    //    }
    //    citizenCampNumText.text = citizenCampNum + "人";
    //}

    ///// <summary>
    ///// 狼陣営プラスボタンの制御
    ///// </summary>
    //public void WolfCampPlus() {
    //    //if (citizenNum == 0) {
    //        wolfCampNum++;
    //    //}
    //    citizenCampNum--;
    //    citizenCampNumText.text = citizenCampNum + "人";
    //    wolfCampNumText.text = wolfCampNum + "人";
    //}

    ///// <summary>
    ///// 市民陣営マイナスボタンの制御
    ///// </summary>
    //public void CitizenCampMinus() {
    //    if (citizenNum == 0) {
    //        citizenCampNum--;
    //    }
    //    citizenCampNumText.text = citizenCampNum + "人";
    //}

    ///// <summary>
    ///// 狼陣営マイナスボタンの制御
    ///// </summary>
    //public void WolfCampMinus() {
    //    //if (citizenNum == 0) {
    //        wolfCampNum--;
    //    //}
    //    citizenCampNum++;
    //    citizenCampNumText.text = citizenCampNum + "人";
    //    wolfCampNumText.text = wolfCampNum + "人";
    //}


    ///// <summary>
    ///// 市民の人数を計算する
    ///// </summary>
    //public void CitizenNum() {
    //    citizenNum = citizenCampNum - (fortuneNum + knightNum + psychicNum);
    //    citizenText.text = citizenNum + "/" + "-";
    //}


    /// <summary>
    /// 引数RollNameをもとに変数を変更する
    /// </summary>
    /// <param name="rollName"></param>
    public void RollSwitch(string rollName) {
        switch (rollName) {
            case "citizen":
                nullNum = citizenNum;
                nullText = citizenText;
                nullLimit = numLimit - 1;
                nullPlusButton = citizenPlusButton;
                nullMinusButton = citizenMinusButton;
                break;
            case "fortune":
                nullNum = fortuneNum;
                nullText = fortuneText;
                nullLimit = fortuneNumLimit;
                nullPlusButton = fortunePlusButton;
                nullMinusButton = fortuneMinusButton;
                break;
            case "knight":
                nullNum = knightNum;
                nullText = knightText;
                nullLimit = knightNumLimit;
                nullPlusButton = knightPlusButton;
                nullMinusButton = knightMinusButton;
                break;
            case "psychic":
                nullNum = psychicNum;
                nullText = psychicText;
                nullLimit = psychicNumLimit;
                nullPlusButton = psychicPlusButton;
                nullMinusButton = psychicMinusButton;
                break;
            case "werewolf":
                nullNum = werewolfNum;
                nullText = werewolfText;
                nullLimit = werewolfNumLimit;
                nullPlusButton = werewolfPlusButton;
                nullMinusButton = werewolfMinusButton;
                break;
            case "madman":
                nullNum = madmanNum;
                nullText = madmanText;
                nullLimit = madmanNumLimit;
                nullPlusButton = madmanPlusButton;
                nullMinusButton = madmanMinusButton;
                break;
        }
    }
    /// <summary>
    /// 変数の値を修正します
    /// </summary>
    public void ChangeVariable(string rollName) {
        switch (rollName) {
            case "citizen":
                citizenNum = nullNum;
                break;
            case "fortune":
                fortuneNum = nullNum;
                break;
            case "knight":
                 knightNum = nullNum;
                break;
            case "psychic":
                psychicNum = nullNum;
                break;
            case "werewolf":
                werewolfNum = nullNum;
                break;
            case "madman":
                madmanNum = nullNum;
                break;
        }
    }

    /// <summary>
    /// 市民陣営プラスボタン　役職判定は引数から
    /// </summary>
    /// <param name="rollName"></param>
    public void CitizenPlusButton(string rollName) {
        RollSwitch(rollName);
        nullNum++;
        nullText.text = nullNum + "/" + nullLimit;
        ChangeVariable(rollName);
        if (nullNum == nullLimit) {
            nullPlusButton.interactable = false;
        }
        if (nullNum == 1) {
            nullMinusButton.interactable = true;
        }
        SumNumber();
        //CitizenCampPlus();
        //CitizenNum();
    }
    /// <summary>
    /// 市民陣営マイナスボタン　役職判定は引数から
    /// </summary>
    /// <param name="rollName"></param>
    public void CitizenMinusButton(string rollName) {
        RollSwitch(rollName);
        nullNum--;
        nullText.text = nullNum.ToString() + "/" + nullLimit;
        ChangeVariable(rollName);
        if (nullNum == nullLimit - 1) {
            nullPlusButton.interactable = true;
        }
        if (nullNum == 0) {
            nullMinusButton.interactable = false;
        }
        SumNumber();
        //CitizenCampMinus();
        //CitizenNum();
    }

    /// <summary>
    /// 狼陣営プラスボタン　役職判定は引数から
    /// </summary>
    /// <param name="rollName"></param>
    public void WolfPlusButton(string rollName) {
        RollSwitch(rollName);
        nullNum++;
        nullText.text = nullNum.ToString() + "/" + nullLimit;
        ChangeVariable(rollName);
        if (nullNum == nullLimit) {
            nullPlusButton.interactable = false;
        }
        if (nullNum == 1) {
            nullMinusButton.interactable = true;
        }
        SumNumber();
        //WolfCampPlus();
        //CitizenNum();
    }
    /// <summary>
    /// 狼陣営マイナスボタン　役職判定は引数から
    /// </summary>
    /// <param name="rollName"></param>
    public void WolfMinusButton(string rollName) {
        RollSwitch(rollName);
        nullNum--;
        nullText.text = nullNum.ToString() + "/" + nullLimit;
        ChangeVariable(rollName);
        if (nullNum == nullLimit - 1) {
            nullPlusButton.interactable = true;
        }
        if (nullNum == 0) {
            nullMinusButton.interactable = false;
        }
        SumNumber();
        //WolfCampMinus();
        //CitizenNum();
    }
}


