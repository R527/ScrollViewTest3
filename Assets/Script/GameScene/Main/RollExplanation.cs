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
    public RollExplanationButtonPrefab rollExplanationButtonPrefab;//その他の

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
