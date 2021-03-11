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
        switch (rollText.text) {
            case "人狼":
                rollPopUP.rollText.text = ROLLTYPE.人狼.ToString();
                rollPopUP.explanationText.text = "市民と同じ立ち回りをしつつ他プレイヤーを狼に仕立て上げましょう！\r\n\r\n吊指定されたときは騎士COして処刑を回避しましょう。\r\n\r\n夜は、脅威的なプレイヤーを襲撃しましょう。\r\n\r\nただし、騎士が死亡してない間は重要役職が守られている可能性が高いです。";
                rollPopUP.statusText.text = "占い結果：黒  \r\n霊能結果：黒 \r\n勝利条件：狼陣営の勝利";
                break;
            case "占い師":
                rollPopUP.rollText.text = ROLLTYPE.占い師.ToString();
                rollPopUP.explanationText.text = "初日の最初に占い師COして、占い結果を伝えましょう。\r\n\r\nただし、占い結果はあなただけが知る情報です。\r\n\r\n他プレイヤー視点から納得する考察が必要です。\r\n\r\n夜、前日までの発言を元に嘘をついている狼を占いましょう！\r\n\r\nまた、占った理由を用意するとより信用されます。";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "市民":
                rollPopUP.rollText.text = ROLLTYPE.市民.ToString();
                rollPopUP.explanationText.text = "役職持ちではないと知る唯一の人物です！\r\n\r\n他の役職が夜襲撃されないように積極的に発言をして狼を探しましょう。\r\n\r\n逆に自ら吊られる言動はしてはいけません。";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "狂人":
                rollPopUP.rollText.text = ROLLTYPE.狂人.ToString();
                rollPopUP.explanationText.text = "人狼の勝利が狂人の勝利条件です。\r\n\r\n誰が人狼が分からないように村を混乱へと導く役割です。\r\n\r\n初日占い師COして適当なプレイヤーに白（市民）と嘘の占い結果を伝えましょう。\r\n\r\n二日目以降の夜も嘘の占いを用意しましょう。\r\n\r\n状況を見て狼プレイヤーに白（市民）、または市民陣営のプレイヤーに黒（狼）と伝えてみましょう！";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：狼陣営の勝利";
                break;
            case "騎士":
                rollPopUP.rollText.text = ROLLTYPE.騎士.ToString();
                rollPopUP.explanationText.text = "夜に狼からの襲撃から護衛する役割があるので、潜伏しつつ指定されたときは騎士COして処刑を回避しましょう!\r\n\r\n夜は狼が襲撃しそうなプレイヤーを守りましょう！\r\n\r\n状況次第では襲撃の予想先ではなく、明日の生存が重要なプレイヤーを確実に護衛する必要もあります。";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
            case "霊能者":
                rollPopUP.rollText.text = ROLLTYPE.霊能者.ToString();
                rollPopUP.explanationText.text = "初日の最初に霊能者COして対抗相手がいない場合ゲーム進行を務めます。\r\n\r\n初日は占われていなく、COしていないプレイヤーの発言を促して、嘘をついていそうなプレイヤーを吊りましょう。\r\n\r\n夜になると、処刑したプレイヤーの白黒を見ることができるので、それをヒントに明日の会話に繋げましょう。";
                rollPopUP.statusText.text = "占い結果：白 \r\n霊能結果：白　\r\n勝利条件：市民陣営の勝利";
                break;
        }
    }
}
