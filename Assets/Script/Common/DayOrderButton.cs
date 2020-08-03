using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Game画面にあるサイドボタン
/// 日付を変更するときに使われる
/// </summary>
public class DayOrderButton : MonoBehaviour
{

    public ChatListManager chatListManager;
    public List<GameObject> nextDaysList = new List<GameObject>();
    public ScrollRect scrollRect;
    //public List<float> targetPosYList = new List<float>();

    //Button
    public Button topButton;
    public Button bottomButton;
    public Button nextDayButton;//翌日指定
    public Button prevDayButton;//前日指定

    public bool isCheckNormalizedPosition;

    //その他
    public bool isCheckChatLog;//チャットログで利用しているかどうかtrueならチャットログ



    // Start is called before the first frame update
    void Start()
    {
        topButton.onClick.AddListener(FirstDay);
        bottomButton.onClick.AddListener(LastDay);
        nextDayButton.onClick.AddListener(NextDay);
        prevDayButton.onClick.AddListener(PrevDay);
    }

    /// <summary>
    /// チャット画面が最新投稿に位置していないときに新たなチャットの生成を停止してチャットが流れないように監視する
    /// </summary>
    private void Update() {

        if (isCheckChatLog) {
            return;
        }

        if(scrollRect.verticalNormalizedPosition > GetContentPosY() && isCheckNormalizedPosition) {
            return;
        }

        //チャット画面が一番下ではないときはチャットをfalseにする
        if (scrollRect.verticalNormalizedPosition > GetContentPosY()) {
            isCheckNormalizedPosition = true;
            return;
        }

        //フィルター中でないときに画面の一番下に来たらチャットをtrueにする
        if (isCheckNormalizedPosition && scrollRect.verticalNormalizedPosition <= GetContentPosY() && !chatListManager.isfilter) {
            chatListManager.OffFilter();
            isCheckNormalizedPosition = false;
            return;
        }

 
    }

    /// <summary>
    /// 一番最初の日にちに戻る
    /// </summary>
    public void FirstDay() {
        ScrollToCore(nextDaysList[0], 1.0f);
    }

    /// <summary>
    /// 一番最後の日にち戻る
    /// </summary>
    public void LastDay() {
        scrollRect.verticalNormalizedPosition = 0f;
    }
    /// <summary>
    /// 1日戻る
    /// </summary>
    public void PrevDay() {
        //Debug.Log(GetContentPosY());
        ScrollToCore(nextDaysList[GetPrevDayPosY()], 1.0f);
    }

    public void NextDay() {
        //Debug.Log(GetNextDayPosY());
        ScrollToCore(nextDaysList[GetNextDayPosY()], 1.0f);
    }
    /// <summary>
    /// 指定した日にちに移動
    /// </summary>
    /// <param name="obj">移動したい日にちのゲームオブジェクト</param>
    /// <param name="align">移動したゲームオブジェクトの配置場所、一番下が0.0f　中央が0.5f 一番下が1.0f 基本日付を一番上に配置するので1.0fで使う</param>
    /// <returns></returns>
    private void ScrollToCore(GameObject obj, float align) {
        RectTransform targetRect = obj.GetComponent<RectTransform>();
        //Contentの高さ取得
        float contentHeight = scrollRect.content.rect.height;

        //Viewportの高さを取得
        float viewportHeight = scrollRect.viewport.rect.height;
        
        //Contentの高さがViewportの高さより小さい場合にはそれ以上スクロールできないので、スクロール不要
        if(contentHeight < viewportHeight) {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        ////InputViewが上に上昇中の微調整分
        //float inputViewHeight = 0;
        //if(chatListManager.gameManager.fillter.inputView.foldingText.text == "↓") {
        //    inputViewHeight = - 72.0f;
        //}
        //ローカル座標がContentHeightの上辺を0として負の値で格納される
        float targetPos = contentHeight + GetPosY(targetRect,contentHeight, viewportHeight) + targetRect.rect.height * align;

        //上端～下端合わせたのための調整量
        float gap = viewportHeight * align;
        //Debug.Log(viewportHeight);
        //Debug.Log(align);

        //差分を計算する
        float normalizedPos = (targetPos - gap) / (contentHeight - viewportHeight);
        //Debug.Log("normalizedPos" + normalizedPos);
        //Clamp01を使ってFloatを０～1にする
        normalizedPos = Mathf.Clamp01(normalizedPos);

        //上記の情報を使ってVerticalNormalizedPositionを実行
        scrollRect.verticalNormalizedPosition = normalizedPos;
        Debug.Log("contentHeight" + contentHeight);//正しい
        //Debug.Log("viewportHeight" + viewportHeight);//正しい
        Debug.Log("targetPos" + targetPos);
        Debug.Log("GetPosY(targetRect)" + GetPosY(targetRect,contentHeight, viewportHeight));
        Debug.Log("targetRect.rect.height" + targetRect.rect.height);//正しい日付変更Objの高さ
        //Debug.Log("gap" + gap);//正しい
        Debug.Log("normalizedPos" + normalizedPos);
        
    }

    /// <summary>
    /// LocalPosison.yのPivotによるずれをRect.yで補正
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    private float GetPosY(RectTransform transform,float contentHeight, float viewportHeight) {
        Debug.Log("transform.localPosition.y" + transform.localPosition.y);
        float i = transform.localPosition.y - contentHeight;
        Debug.Log(i);
        Debug.Log("transform.rect.y" + transform.rect.y);
        Debug.Log("viewportHeight" + viewportHeight);
        return i + transform.rect.y;
    }

    /// <summary>
    /// 前日に戻るために使う 今のチャット画面のポジションと日付の位置を比較してどのObjが前日かを決定する
    /// Intを返す
    /// </summary>
    /// <returns></returns>
    public int GetPrevDayPosY() {

        int i = 0;
        Debug.Log("GetContentPosY");
        for (i = nextDaysList.Count - 1; 0 <= i; i--) {

            RectTransform targetRect = nextDaysList[i].transform.GetComponent<RectTransform>();
            Debug.Log("targetRect.localPosition.y" + targetRect.localPosition.y);//マイナスになってない
            Debug.Log("targetRect.rect.y"+targetRect.rect.y);
            float targetPosY = targetRect.localPosition.y + targetRect.rect.y;
            Debug.Log("targetPosY" + targetPosY);
            float contentPosY = - scrollRect.content.localPosition.y;
            //float chatAreaHeight = - scrollRect.GetComponent<RectTransform>().rect.height;
            //float gap = contentPosY + chatAreaHeight;
            Debug.Log("contentPosY"+contentPosY);//
            //Debug.Log("chatAreaHeight" + chatAreaHeight);//正しい
            //Debug.Log("gap" + gap);
            if (targetPosY > contentPosY) {
                Debug.Log(i);
                Debug.Log("決定");
                break;
            }
        }

        if(i <= 0) {
            i = 0;
        }
        return i;
    }

    /// <summary>
    /// 翌日に戻るために使う 今のチャット画面のポジションと日付の位置を比較してどのObjが翌日化を決定する
    /// Intを返す
    /// </summary>
    /// <returns></returns>
    public int GetNextDayPosY() {
        int i = 0;
        Debug.Log("GetContentPosY");
        for (i = 0; nextDaysList.Count - 1  > i; i++) {
            Debug.Log("test");
            Debug.Log(i);
            RectTransform targetRect = nextDaysList[i].transform.GetComponent<RectTransform>();
            float targetPosY = targetRect.localPosition.y + targetRect.rect.y;
            Debug.Log(targetPosY);
            float contentPosY = - scrollRect.content.localPosition.y;
            float chatAreaHeight = scrollRect.GetComponent<RectTransform>().rect.height;
            //float gap = contentPosY + chatAreaHeight;
            //Debug.Log("gap" + gap);
            Debug.Log("chatAreaHeight" + chatAreaHeight);
            Debug.Log("contentPosY" + contentPosY);
            Debug.Log("targetPosY" + targetPosY);
            if (targetPosY < contentPosY) {
                i++;
                Debug.Log(i);
                Debug.Log("決定");
                break;
            }
        }
        return i;
    }

    /// <summary>
    /// コンテントの位置からverticalNormalizedPositionを取得する
    /// </summary>
    /// <returns></returns>
    private float GetContentPosY() {
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float normalizedPos = viewportHeight / contentHeight;
        normalizedPos = Mathf.Clamp01(normalizedPos);
        return normalizedPos;
    }
}
