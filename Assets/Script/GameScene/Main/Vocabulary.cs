using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 用語解説
/// </summary>
public class Vocabulary : MonoBehaviour
{

    //main
    public Text glossaryText;//Text解説
    public Text glossaryLavelText;//解説のラベル
    public string buttonText;
    public GameObject glossaryObj;//解説Obj
    public GameObject vocabularyPopUP;//Buttonの羅列があるやつ
    public List<Button> vocabularyButtonList = new List<Button>();//用語ボタン
    public Button vocabularymaskButton;//用語ボタンがある方のますく
    public Button vocabularybackButton;//用語ボタンのある方のバックボタン
    public Button glossarymaskButton;//解説側のマスク
    public Button glossarybackButton;//解説側のバックボタン
    private void Start() {
        vocabularymaskButton.onClick.AddListener(CloseVocabularyPopUP);
        vocabularybackButton.onClick.AddListener(CloseVocabularyPopUP);
        glossarymaskButton.onClick.AddListener(CloseGlossaryPopUp);
        glossarybackButton.onClick.AddListener(CloseGlossaryPopUp);
        foreach (Button obj in vocabularyButtonList) {
            obj.onClick.AddListener(VocabluryButton);
        }
    }

    /// <summary>
    /// 用語ごとのボタンを押したら解説が表示される
    /// </summary>
    public void VocabluryButton() {
        Debug.Log(buttonText);
        switch (buttonText) {
            //初心者向け
            case "CO・塩":
                glossaryText.text = "カミングアウトの略。\r\n自分の役職を公表すること。";
                glossaryLavelText.text = buttonText;
                break;
            case "白・黒":
                glossaryText.text = "白とは市民。黒とは狼を指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "確白・確黒":
                glossaryText.text = "占い、霊能結果から一人のプレイヤーが白黒確定したこと。";
                glossaryLavelText.text = buttonText;
                break;
            case "グレー":
                glossaryText.text = "占われていない、かつ、役職が確定していないプレイヤーを指す。占い、処刑対象になりやすい。";
                glossaryLavelText.text = buttonText;
                break;
            case "人外":
                glossaryText.text = "人狼陣営、第3陣営を指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "真・偽":
                glossaryText.text = "真は本物、偽は偽物を指す。占い、霊能者が二人以上COしたときに使われる。";
                glossaryLavelText.text = buttonText;
                break;
            case "対抗":
                glossaryText.text = "同じ役職が複数出た場合に、自分から見たもう一方の役を指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "役職持ち":
                glossaryText.text = "市民を除いた市民陣営を指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "吊":
                glossaryText.text = "処刑することや、処刑対象を指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "〇〇ロラ":
                glossaryText.text = "同じ役職が複数COされている時に、その全員を処刑することを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "囲い・逆囲い":
                glossaryText.text = "偽の占い師が、狼に対して、白（市民）と結果を出すことを囲い、\r\n\r\n市民に対して黒（狼）の結果を出すことを「逆囲い」と言う。";
                glossaryLavelText.text = buttonText;
                break;
            case "噛み":
                glossaryText.text = "狼が夜の行動で、市民陣営を噛み殺すことを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "潜伏":
                glossaryText.text = "狼が夜の行動で、市民以外がカミングアウトせずに市民のふりをすること。";
                glossaryLavelText.text = buttonText;
                break;
            case "騙る":
                glossaryText.text = "嘘をつくこと。";
                glossaryLavelText.text = buttonText;
                break;
            case "破綻":
                glossaryText.text = "矛盾すること。主に人狼陣営が嘘を言っているのがばれるときに使われる。";
                glossaryLavelText.text = buttonText;
                break;
            case "縄":
                glossaryText.text = "残りの処刑回数制限を指す。　9人村であれば4縄、つまり4回人狼を釣るチャンスがあることを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "凸":
                glossaryText.text = "突然死の略。1日の間に一言も話さないと「突然死」する。";
                glossaryLavelText.text = buttonText;
                break;
                //中級者向け
            case "PP":
                glossaryText.text = "パワープレイの略。　村の総数に対して人狼陣営の割合が多いときに使われる。　意図的に市民陣営に投票を集めて人狼陣営を勝利へと導く。";
                glossaryLavelText.text = buttonText;
                break;
            case "ベグ":
                glossaryText.text = "狼から見てどちらの占い師が真であるかがわからない状態で噛むことを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "漂白噛み":
                glossaryText.text = "人狼があえて狂人を噛むことを指す。村側の狼は真の占い師を噛むだろうという心理を逆手に取った戦術。";
                glossaryLavelText.text = buttonText;
                break;
            case "GS":
                glossaryText.text = "グレースケールの略。グレーの中から怪しい順にランキングすることを指す。場合によってはグレーを排除した状態で使われることもある。";
                glossaryLavelText.text = buttonText;
                break;
            case "グレラン":
                glossaryText.text = "グレーランダムの略。進行方法のひとつで、グレーの中からランダムに投票する。";
                glossaryLavelText.text = buttonText;
                break;
            case "SG":
                glossaryText.text = "スケープゴートの略。狼が市民を人狼に仕立て上げる、または、発言内容から人狼に仕立て上げられやすいプレイヤーを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "RPP":
                glossaryText.text = "ランダムパワープレイの略。村の総数が偶数の状態でパワープレイすることを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "パンダ":
                glossaryText.text = "二人以上の占い師または、霊能者が一人のプレイヤーに白と黒判定を出すときに使われる。";
                glossaryLavelText.text = buttonText;
                break;
            case "FO":
                glossaryText.text = "フルオープンの略。進行方法や、試合の中盤で役職を持っているプレイヤー全員がCOすることを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "多弁":
                glossaryText.text = "よくしゃべるプレイヤーを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "スライド":
                glossaryText.text = "一度COした後に別の役職にCOしなおす、または、COを取り消すことを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "ライン":
                glossaryText.text = "人狼同士のつながりを指す。人狼は市民陣営と違い話し合うことができるので会話から探すときに使われる。";
                glossaryLavelText.text = buttonText;
                break;
            case "GJ":
                glossaryText.text = "騎士が夜、狼の噛み先を守った時に使われる。";
                glossaryLavelText.text = buttonText;
                break;
            case "貫通":
                glossaryText.text = "進行方法の一つ。吊指定されたプレイヤーの役職関係なくCOさせずに吊を決行することを指す。";
                glossaryLavelText.text = buttonText;
                break;
            case "回避":
                glossaryText.text = "進行方法の一つ。吊指定されたプレイヤーが騎士COするなどして、吊から逃れることを指す。";
                glossaryLavelText.text = buttonText;
                break;
        }
        glossaryObj.SetActive(true);
        vocabularyPopUP.SetActive(false);
    }

    /// <summary>
    /// ボタンに表示されているテキストを取得する
    /// </summary>
    /// <param name="obj"></param>
    public void PushJuge(GameObject obj) {
        buttonText = obj.GetComponentInChildren<Text>().text;
    }

    
　   /// <summary>
    /// 用語ボタン用のPopUp
    /// </summary>
    public void CloseVocabularyPopUP() {
        vocabularyPopUP.SetActive(false);
    }
    /// <summary>
    /// 解説用のPopUP
    /// </summary>
    public void CloseGlossaryPopUp() {
        glossaryObj.SetActive(false);
        vocabularyPopUP.SetActive(true);
    }

}
