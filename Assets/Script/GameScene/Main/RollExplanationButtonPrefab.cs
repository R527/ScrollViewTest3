using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollExplanationButtonPrefab : MonoBehaviour {

    //class
    public RollPopUp rollPopUPPrefab;
    public GameObject gameCanvas;
    public Button rollBtn;
    public GameObject vocabulary;

    public Text rollText;

    private void Start() {
        vocabulary = GameObject.FindGameObjectWithTag("Vocabulary");
        rollBtn.onClick.AddListener(RollExplanationButton);
    }

    public void RollExplanationButton() {
        if(gameCanvas == null) {
            gameCanvas = GameObject.FindGameObjectWithTag("GameCanvas");
        }

        RollPopUp rollPopUP = Instantiate(rollPopUPPrefab, gameCanvas.transform, false);
        rollPopUP.vocabulary = vocabulary;
        vocabulary.SetActive(false);
        Debug.Log(rollText.text);
        switch (rollText.text) {
            case "人狼":
                rollPopUP.rollText.text = ROLLTYPE.人狼.ToString();
                rollPopUP.explanationText.text = "人狼";
                rollPopUP.statusText.text = "占い結果：黒  \r\n霊能結果：黒 \r\n勝利条件：狼陣営の勝利";
                break;
            case "占い師":
                rollPopUP.rollText.text = ROLLTYPE.占い師.ToString();
                rollPopUP.explanationText.text = "占い師";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "市民":
                rollPopUP.rollText.text = ROLLTYPE.市民.ToString();
                rollPopUP.explanationText.text = "市民";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "狂人":
                rollPopUP.rollText.text = ROLLTYPE.狂人.ToString();
                rollPopUP.explanationText.text = "狂人";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：狼陣営の勝利";
                break;
            case "騎士":
                rollPopUP.rollText.text = ROLLTYPE.騎士.ToString();
                rollPopUP.explanationText.text = "騎士";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "霊能者":
                rollPopUP.rollText.text = ROLLTYPE.霊能者.ToString();
                rollPopUP.explanationText.text = "霊能";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
        }
    }
}
