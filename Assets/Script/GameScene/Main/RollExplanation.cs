using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 役職説明、自分の役職へのボタンなど
/// </summary>
public class RollExplanation : MonoBehaviour
{

    //class
    public ChatSystem chatSystem;

    //main
    public Text rollText;//役職名
    public Text explanationText;//役職説明
    public Text statusText;//ステータス
    public GameObject rollExplanationPopUp;
    public GameObject rollButtonContent;//その他のボタンを置く場所
    public RollExplanationButtonPrefab RollExplanationButtonPrefab;//その他の
    public Button rollExplanationButton;//自分の役職詳細ボタン


    /// <summary>
    /// 役職説明ボタンの無効化と部屋設定ごとに役職説明用のボタンを用意する
    /// </summary>
    private void Start() {
        rollExplanationButton.onClick.AddListener(RollExplanationButton);
    }

    /// <summary>
    /// GameManagerより役職リストをもらって役職説明ボタンを作る準備をする
    /// </summary>
    /// <param name="rollTypeList"></param>
    public void RollExplanationSetUp(List<ROLLTYPE> rollTypeList) {
        for (int i = 0; i < rollTypeList.Count; i++) {
            RollExplanationButtonPrefab Obj = Instantiate(RollExplanationButtonPrefab, rollButtonContent.transform, false);
            Obj.rollText.text = rollTypeList[i].ToString();
        }
    }

    /// <summary>
    /// 役職説明ボタン
    /// </summary>
    public void RollExplanationButton() {
        rollExplanationPopUp.SetActive(true);
        switch (chatSystem.myPlayer.rollType) {
            case ROLLTYPE.人狼:
                rollText.text = ROLLTYPE.人狼.ToString();
                explanationText.text = "じんろうだよー";
                statusText.text = "占い結果：黒  \r\n霊能結果：黒 \r\n勝利条件：狼陣営の勝利";
                break;
            case ROLLTYPE.占い師:
                rollText.text = ROLLTYPE.占い師.ToString();
                explanationText.text = "占い師";
                statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case ROLLTYPE.市民:
                rollText.text = ROLLTYPE.市民.ToString();
                explanationText.text = "市民";
                statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case ROLLTYPE.狂人:
                rollText.text = ROLLTYPE.狂人.ToString();
                explanationText.text = "狂人";
                statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case ROLLTYPE.騎士:
                rollText.text = ROLLTYPE.騎士.ToString();
                explanationText.text = "狩人";
                statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case ROLLTYPE.霊能者:
                rollText.text = ROLLTYPE.霊能者.ToString();
                explanationText.text = "霊能";
                statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
        }
    }
}
