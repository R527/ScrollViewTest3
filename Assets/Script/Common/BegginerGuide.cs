using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 初心者ガイドの制御
/// </summary>
public class BegginerGuide : MonoBehaviour
{

    //main
    public GUIDE_TYPE guideType;
    public Text guideTitleText;//ガイドのタイトル
    public GameObject begginerGuidePopUp;
    public int num;//ガイド通し番号
    public int numLimit;//通し番号制限
    public Text commentaryText;//解説テキスト
    public Text commentarySSText;//解説テキスト(画像あり
    //役職紹介
    public Text fortuneText;//占い結果
    public Text psychicText;//霊能結果
    public Text groupText;//系統
    public Text campText;//陣営
    public Text abilityText;//能力と勝利条件
    public Text thinkingText;//立ち回り
    public Text rollNameText;//役職名
    public GameObject rollObj;//役職紹介Obj
    public GameObject commentaryTextObj;//役職紹介以外の解説Obj
    public ScrollRect scrollRect;
    //役職注釈関連
    public Text annotationText;
    public Button fortuneButton;
    public Button psychicButton;
    public Button groupButton;
    public Button campButton;
    public Button closeAnnotationButton;
    public GameObject annotationButtonObj;
    //通常時のButton関連
    public Button nextButton;//次の説明へ
    public Button backButton;//後ろの説明へ
    public Button returnButton;//ガイド終了ボタン
    public Button maskButton;//マスクについているボタン
    public Button checkButton;//初心者ガイドをゲーム開始時に出すかどうかを決めるボタン
    public Text returnText;//ガイド終了ボタンのText
    public GameObject mainButtonObj;//上記のButtonをまとめたObj
    //画像
    public List<Sprite> begginerGuideSSList;
    public Image begginerGuideSSImage;




    // Start is called before the first frame update
    void Start()
    {
        backButton.interactable = false;
        num = 1;
        Commentary();
        //メインButtonセット
        nextButton.onClick.AddListener(NextButton);
        backButton.onClick.AddListener(BackButton);
        returnButton.onClick.AddListener(ReturnButton);
        maskButton.onClick.AddListener(ReturnButton);
        //注釈Buttonセット
        fortuneButton.onClick.AddListener(FortuneButton);
        psychicButton.onClick.AddListener(PsychicButton);
        groupButton.onClick.AddListener(GroupButton);
        campButton.onClick.AddListener(CampButton);
        closeAnnotationButton.onClick.AddListener(CloseAnnotation);
        PlayerManager.instance.SetStringForPlayerPrefs("初心者ガイドをみました", PlayerManager.ID_TYPE.begginer);
    }

    /// <summary>
    /// 次の説明文へ
    /// </summary>
    public void NextButton() {
        //AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        num++;
        if(num == numLimit) {
            nextButton.interactable = false;
        }
        if(num == 2) {
            backButton.interactable = true;
        }
        Commentary();
    }

    /// <summary>
    /// 前の説明文へ
    /// </summary>
    public void BackButton() {
        //AudioManager.instance.PlaySE(AudioManager.SE_TYPE.NG);
        num--;
        if (num == numLimit - 1) {
            nextButton.interactable = true;
        }
        if (num == 1) {
            backButton.interactable = false;
        }
        Commentary();
    }
    /// <summary>
    /// ガイドメニューへ戻る
    /// </summary>
    public void ReturnButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.NG);
        Destroy(gameObject);
        Instantiate(begginerGuidePopUp);
    }

    /// <summary>
    /// 初心者ガイドの解説文を通し番号に対応させる
    /// </summary>
    public void Commentary() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (guideType) {
            case GUIDE_TYPE.人狼の遊び方:
                commentaryTextObj.SetActive(true);
                mainButtonObj.SetActive(true);
                numLimit = 2;
                switch (num) {
                    case 1:
                        commentaryText.text = "<b>ゲーム内容</b>\r\n<color=blue>市民陣営</color>と<color=red>人狼陣営</color>に分かれて会話の中から嘘をついている<color=red>狼</color>を見つけ出す心理ゲーム。\r\n\r\n陣営ごとに複数の役職があり、それらの言動をヒントに村を平和へと導きましょう。\r\n\r\n<b>勝利条件</b>\r\n<color=blue>市民陣営</color>は<color=red>人狼</color>の全滅。\r\n<color=red>人狼陣営</color>は<color=red>人狼</color>の人数と<color=blue>市民系統</color>の人数を同数にする。\r\n\r\n<b>ゲームの流れ</b>\r\n<color=navy>夜</color>→<color=green>昼</color>→<color=orange>夕方</color>→<color=navy>夜</color>...と繰り返されどちらかの勝利条件を満たすとゲーム終了。\r\n各時間ごとに行動が変わります";
                        break;
                    case 2:
                        commentaryText.text = "<b>夜の時間</b>\r\n各役職の能力を使えます。\r\n<color=red>人狼以外</color>は相談できません。\r\n前日までの会話を整理しましょう\r\n\r\n<b>昼の時間</b>\r\n話し合う時間です。\r\n<color=blue>市民陣営</color>は嘘をついている<color=red>狼</color>を探しましょう。\r\n<color=red>人狼陣営</color>はうまく紛れて<color=blue>市民</color>を騙しましょう！\r\n\r\n<b>夕方の時間</b>\r\n<color=red>人狼</color>を追放する時間です。\r\n進行が一人のプレイヤーを指定し投票を集めましょう！最も投票の多いプレイヤーを追放します。";
                        break;
                }
                break;
            case GUIDE_TYPE.操作方法:
                commentarySSText.gameObject.SetActive(true);
                begginerGuideSSImage.gameObject.SetActive(true);
                mainButtonObj.SetActive(true);
                numLimit = 3;
                switch (num) {
                    //画像を絡めつつ説明したい
                    case 1:
                        begginerGuideSSImage.sprite = begginerGuideSSList[0];
                        begginerGuideSSImage.rectTransform.sizeDelta = new Vector2(186.9841f, 100f);
                        begginerGuideSSImage.rectTransform.localPosition = new Vector2(-1.5259e-05f, 87.595f);
                        commentarySSText.rectTransform.sizeDelta = new Vector2(186.9842f, 175.9904f);
                        commentarySSText.rectTransform.localPosition = new Vector2(0f, -50.4f);

                        commentarySSText.text = "<b>1.</b>あなたの本当の姿です。役職紹介が見れます。\n\r<b>2.</b>課金残高です。追加で購入もできます。\n\r<b>3.</b>参加人数と生存人数を表示しています。\n\r<b>4.</b>残り時間を表示しています。0秒になると次のフェーズに移ります。\n\r<b>5.</b>退出ボタンと時短ボタンです。お昼の時間のみ時間の短縮を希望することができます。死亡後は退出が可能になりますが、ゲーム途中での退出には課金が必要です。\n\r<b>6.</b>専門用語をまとめています。\n\r<b>7.</b>メニュー";
                        break;
                    case 2:
                        begginerGuideSSImage.sprite = begginerGuideSSList[1];
                        begginerGuideSSImage.rectTransform.sizeDelta = new Vector2(135.2714f, 205.8609f);
                        begginerGuideSSImage.rectTransform.localPosition = new Vector2(-25.856f, 37.53f);
                        commentarySSText.rectTransform.sizeDelta = new Vector2(186.9842f, 72.99475f);
                        commentarySSText.rectTransform.localPosition = new Vector2(0f, -101.9f);
                        commentarySSText.text = "<b>1.</b>コメントを上下に移動できます。\n\r<b>2.</b>フィルターです。下のプレイヤーボタンを押すとそのプレイヤーが発言したコメントのみを表示します。";
                        break;
                    case 3:
                        //begginerGuideSSImage.sprite = begginerGuideSSList[2];
                        commentarySSText.text = "<b>1.</b>上下ボタンです\n\r<b>2.</b>人狼と市民チャットを切り替えれます。\n\r<b>3.</b>自分の正体を明かすことができます。\n\r<b>4.</b>発言することができます。\n\r<b>5.</b>青チャットと通常チャットの切り替えです。青チャットには課金が必要です\n\r<b>6.</b>プレイヤーボタンです。フェイズごとに挙動が変わります。\n\r<b>夕方</b>、プレイヤーを投票できます。\n\r<b>夜</b>、役職ごとの行動を決定します。\n\rまた、<b>ルームマスターは</b>ゲーム開始前プレイヤーを強制退場できます。\n\r<b>ゲーム終了後または死亡後</b>にプレイヤーを回避することができます。";
                        break;
                }
                break;
            case GUIDE_TYPE.用語説明:
                commentaryTextObj.SetActive(true);
                mainButtonObj.SetActive(true);
                numLimit = 3;
                switch (num) {
                    case 1:
                        commentaryText.text = "人狼ではいくつかの専門用語を扱います。\r\n\r\n<b>CO・塩</b>\r\nカミングアウトの略。<b>自分の役職を公表すること</b>。\r\n\r\n<b>白・黒</b><color=blue> 白とは市民</color>。<color=red>黒とは狼</color>を指す。\r\n\r\n<b>確白・確黒</b>  占い、霊能結果から一人のプレイヤーが<b>白・黒確定</b>したこと。\r\n\r\n<b>グレー</b>\r\n<b>占われていない</b>、かつ、<b>役職が確定していない</b>プレイヤーを指す。";
                        break;
                    case 2:
                        //、<color=fuchsia>第3陣営</color>
                        commentaryText.text = "<b>人外</b>\r\n<color=red>人狼陣営</color>を指す。\r\n\r\n<b>真・偽</b>  真は本物、偽は偽物を指す。<b>占い霊能者などが二人以上CO</b>したときに使われる。\r\n\r\n<b>吊</b>  処刑することや、<b>処刑対象</b>を指す\r\n\r\n<b>〇〇ロラ</b>  同じ役職が複数COされている時に、<b>その全員を処刑する</b>ことを指す。\r\n（例）霊能ロラ　霊能者COしているプレイヤー全員処刑する。";
                        break;
                    case 3:
                        commentaryText.text = "<b>囲い・逆囲い</b>\r\n偽の占い師結果で、<color=red>狼</color>に対して<color=blue>白（市民）</color>と騙していることを<b>囲い</b>\r\n<color=blue>市民</color>に対して<color=red>黒（狼）</color>と騙していることを<b>逆囲い</b>と言う。\r\n\r\n<b>噛み・襲撃</b>  <color=red>狼</color>が夜の行動で、プレイヤー倒すことを指す。\r\n\r\n<b>潜伏</b>\r\n市民以外が<b>カミングアウトせずに市民のふり</b>をすること。\r\n\r\n<b>凸</b>  突然死の略。<b>1日の間に一言も話さないと<color=red>死亡</color>する</b>。";
                        break;
                    //case 4:
                    //    commentaryText.text = "また、その他の用語もゲーム中に右上のメニューから確認できます！";
                    //    //メニューへの誘導画像
                    //    break;
                }
                break;

                //利用規約完成後修正する
            case GUIDE_TYPE.禁止事項:
                commentaryTextObj.SetActive(true);
                mainButtonObj.SetActive(true);
                numLimit = 2;
                switch (num) {
                    case 1:
                        commentaryText.text = "故意に敗北へ導く行為全て禁止とします。\r\n\r\n・生存中に回線を切る、会話に参加しない行為\r\n\r\n・他の役職を不正方法で把握する行為\r\n\r\n・意味なく別役職のカミングアウト\r\n\r\n・暴言等、著しく不快な言動\r\n\r\n・上級者が初心者のふりをする行為";
                        break;
                    case 2:
                        commentaryText.text = "・占い結果などゲームシステムを利用した発言\r\n\r\n・バグ技の利用\r\n\r\n・日本語以外での会話\r\n\r\n・その他運営が不適切と判断する行為\r\n\r\n詳細は利用規約参照して下さい。";
                        //\r\n\r\n初めてプレイされる方は<b>迷惑行為</b>、<b>禁止事項</b>が分からない部分もあると思います。\r\nまず、<b>初心者ガイド</b>や<b>用語説明</b>などを参考にしてください。
                        //観戦画面への誘導画像
                        break;
                }
                break;
            case GUIDE_TYPE.役職紹介:
                rollObj.SetActive(true);
                mainButtonObj.SetActive(true);
                numLimit = 6;
                scrollRect.verticalNormalizedPosition = 1.0f;
                switch (num) {
                    case 1:
                        //市民
                        rollNameText.text = "市民";
                        fortuneText.text = "占い：市民";
                        psychicText.text = "霊能：市民";
                        groupText.text = "系統：市民";
                        campText.text = "市民陣営";
                        abilityText.text = "能力：" + "なし" + "\r\n勝利条件：" + "人狼の全滅";
                        thinkingText.text = "役職持ちではないと知る唯一の人物です！\r\n\r\n他の役職が夜襲撃されないように積極的に発言をして狼を探しましょう。\r\n\r\n逆に自ら吊られる言動はしてはいけません。";
                        break;
                    case 2:
                        //占い師
                        rollNameText.text = "占い師";
                        fortuneText.text = "占い：市民";
                        psychicText.text = "霊能：市民";
                        groupText.text = "系統：市民";
                        campText.text = "市民陣営";
                        abilityText.text = "能力：" + "夜に指定したプレイヤーの占い結果を得ます。" + "\r\n勝利条件：" + "人狼の全滅";
                        thinkingText.text = "初日の最初に占い師COして、占い結果を伝えましょう。\r\n\r\nただし、占い結果はあなただけが知る情報です。\r\n\r\n他プレイヤー視点から納得する考察が必要です。\r\n\r\n夜、前日までの発言を元に嘘をついている狼を占いましょう！\r\n\r\nまた、占った理由を用意するとより信用されます。";
                        break;
                    case 3:
                        //騎士
                        rollNameText.text = "騎士";
                        fortuneText.text = "占い：市民";
                        psychicText.text = "霊能：市民";
                        groupText.text = "系統：市民";
                        campText.text = "市民陣営";
                        abilityText.text = "能力：" + "夜に指定したプレイヤーを護衛する。" + "\r\n勝利条件：" + "人狼の全滅";
                        thinkingText.text = "夜に狼からの襲撃から護衛する役割があるので、潜伏しつつ指定されたときは騎士COして処刑を回避しましょう!\r\n\r\n夜は狼が襲撃しそうなプレイヤーを守りましょう！\r\n\r\n状況次第では襲撃の予想先ではなく、明日の生存が重要なプレイヤーを確実に護衛する必要もあります。";
                        break;
                    case 4:
                        //霊能者
                        rollNameText.text = "霊能者";
                        fortuneText.text = "占い：市民";
                        psychicText.text = "霊能：市民";
                        groupText.text = "系統：市民";
                        campText.text = "市民陣営";
                        abilityText.text = "能力：" + "処刑したプレイヤーの霊能結果を得ます。" + "\r\n勝利条件：" + "人狼の全滅";
                        thinkingText.text = "初日の最初に霊能者COして対抗相手がいない場合ゲーム進行を務めます。\r\n\r\n初日は占われていなく、COしていないプレイヤーの発言を促して、嘘をついていそうなプレイヤーを吊りましょう。\r\n\r\n夜になると、処刑したプレイヤーの白黒を見ることができるので、それをヒントに明日の会話に繋げましょう。";
                        break;
                    case 5:
                        //人狼
                        rollNameText.text = "人狼";
                        fortuneText.text = "占い：人狼";
                        psychicText.text = "霊能：人狼";
                        groupText.text = "系統：人狼";
                        campText.text = "人狼陣営";
                        abilityText.text = "能力：" + "夜に指定したプレイヤーを襲撃する。" + "\r\n勝利条件：" + "人狼の人数と市民陣営の人数を同数にする";
                        thinkingText.text = "市民と同じ立ち回りをしつつ他プレイヤーを狼に仕立て上げましょう！\r\n\r\n吊指定されたときは騎士COして処刑を回避しましょう。\r\n\r\n夜は、脅威的なプレイヤーを襲撃しましょう。\r\n\r\nただし、騎士が死亡してない間は重要役職が守られている可能性が高いです。";
                        break;
                    case 6:
                        //狂信者
                        rollNameText.text = "狂信者";
                        fortuneText.text = "占い：市民";
                        psychicText.text = "霊能：市民";
                        groupText.text = "系統：市民";
                        campText.text = "人狼陣営";
                        abilityText.text = "能力：" + "人狼が誰かを知っています。" + "\r\n勝利条件：" + "人狼の人数と市民陣営の人数を同数にする。";
                        thinkingText.text = "人狼の勝利が狂信者の勝利条件です。\r\n\r\n誰が人狼が分からないように村を混乱へと導く役割です。\r\n\r\n初日占い師COして適当なプレイヤーに白（市民）と嘘の占い結果を伝えましょう。\r\n\r\n二日目以降の夜も嘘の占いを用意しましょう。\r\n\r\n状況を見て狼プレイヤーに白（市民）、または市民陣営のプレイヤーに黒（狼）と伝えてみましょう！";
                        break;
                }
                break;
        }
        returnText.text = "戻る" + num + "/" + numLimit;
    }

    //注釈ボタンの制御
    /// <summary>
    /// 占い結果解説
    /// </summary>
    public void FortuneButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        SwichObj();
        annotationText.text = "占い師が夜に占った結果を表示します。\r\n\r\n狂信者など、占い結果と陣営が違うことがあります。";
    }
    /// <summary>
    /// 霊能結果の解説
    /// </summary>
    public void PsychicButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        SwichObj();
        annotationText.text = "霊能者が夜に見る結果を表示します。";

    }
    /// <summary>
    /// 系統の解説
    /// </summary>
    public void GroupButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        SwichObj();
        annotationText.text = "市民、人狼の勝利条件に関わるその役職の系統を表示します。\r\n\r\n（例）最終日に市民、狂信者、人狼の3人が残った場合、市民陣営は一人、人狼陣営は二人ですが狂信者は市民とカウントします。\r\n人狼の勝利条件を満たしていないのでゲーム継続されます。";

    }
    /// <summary>
    /// 陣営の解説
    /// </summary>
    public void CampButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        SwichObj();
        annotationText.text = "各陣営を表示します。\r\n\r\n各陣営の勝利条件\r\n\r\n市民陣営：人狼の全滅\r\n人狼陣営：市民系統と人狼の人数が同数になる。\r\n第3陣営：各役職ごとに勝利条件が違います。役職解説を参照してください。";

    }

    /// <summary>
    /// 注釈閉じる
    /// </summary>
    public void CloseAnnotation() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        annotationButtonObj.SetActive(false);
        mainButtonObj.SetActive(true);
        annotationText.gameObject.SetActive(false);
        rollObj.SetActive(true);

    }

    /// <summary>
    /// Objの交換とOnOff
    /// </summary>
    public void SwichObj() {
        rollObj.SetActive(false);
        annotationText.gameObject.SetActive(true);
        annotationButtonObj.SetActive(true);
        mainButtonObj.SetActive(false);
    }

}
