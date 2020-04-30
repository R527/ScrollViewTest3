using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ゲームシーンにある確認画面とメニューにルールを記入する
/// </summary>
public class SetRule : BasePopUP {

    //class
    public GameManager gameManager;

    //main
    public Button ruleButton;//参加確認が確認画面にあるルール閲覧ボタン
    public Text ruleConfiramationButtonText;//ルール確認ボタン
    public GameObject ruleConfiramationPrefab;
    public Transform ruleTran;



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ruleButton.onClick.AddListener(RuleConfirmation);
    }

    /// <summary>
    /// 人数が規定人数揃った後の確認UIにあるルール表示ボタン
    /// </summary>
    public void RuleConfirmation() {
        if (ruleConfiramationButtonText.text == "↓") {
            destroyObject = Instantiate(ruleConfiramationPrefab, ruleTran,false);
            ruleConfiramationButtonText.text = "↑";
        } else {
            Debug.Log("destroy");
            DestroyPopUP();
            ruleConfiramationButtonText.text = "↓";
        }
    }
}
