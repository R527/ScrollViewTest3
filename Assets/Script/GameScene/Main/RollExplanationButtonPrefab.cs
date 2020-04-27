using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollExplanationButtonPrefab : MonoBehaviour {

    //class
    public RollExplanation rollExplanation;
    public Text rollText;


    private void Start() {
        rollExplanation = GameObject.Find("RollExplanation").GetComponent<RollExplanation>();
    }

    public void RollExplanationButton() {
        switch (rollText.text) {
            case "人狼":
                rollExplanation.rollText.text = ROLLTYPE.人狼.ToString();
                rollExplanation.explanationText.text = "じんろうだよー";
                rollExplanation.statusText.text = "占い結果：黒  \r\n霊能結果：黒 \r\n勝利条件：狼陣営の勝利";
                break;
            case "占い師":
                rollExplanation.rollText.text = ROLLTYPE.占い師.ToString();
                rollExplanation.explanationText.text = "占い師";
                rollExplanation.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "市民":
                rollExplanation.rollText.text = ROLLTYPE.市民.ToString();
                rollExplanation.explanationText.text = "市民";
                rollExplanation.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "狂人":
                rollExplanation.rollText.text = ROLLTYPE.狂人.ToString();
                rollExplanation.explanationText.text = "狂人";
                rollExplanation.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：狼陣営の勝利";
                break;
            case "騎士":
                rollExplanation.rollText.text = ROLLTYPE.騎士.ToString();
                rollExplanation.explanationText.text = "騎士";
                rollExplanation.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "霊能者":
                rollExplanation.rollText.text = ROLLTYPE.霊能者.ToString();
                rollExplanation.explanationText.text = "霊能";
                rollExplanation.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
        }
    }
}
