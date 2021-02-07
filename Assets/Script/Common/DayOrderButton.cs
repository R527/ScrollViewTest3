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

        //タイトル画面ならリターン
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
        ScrollToCore(nextDaysList[GetPrevDayPosY()], 1.0f);
    }

    public void NextDay() {
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

        //ローカル座標がContentHeightの上辺を0として負の値で格納される
        float targetPos = contentHeight + GetPosY(targetRect,contentHeight, viewportHeight) + targetRect.rect.height * align;

        //上端～下端合わせたのための調整量
        float gap = viewportHeight * align;

        //差分を計算する
        float normalizedPos = (targetPos - gap) / (contentHeight - viewportHeight);
        //Clamp01を使ってFloatを０～1にする
        normalizedPos = Mathf.Clamp01(normalizedPos);

        //上記の情報を使ってVerticalNormalizedPositionを実行
        scrollRect.verticalNormalizedPosition = normalizedPos;
        
    }

    /// <summary>
    /// LocalPosison.yのPivotによるずれをRect.yで補正
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    private float GetPosY(RectTransform transform,float contentHeight, float viewportHeight) {
        float i = transform.localPosition.y - contentHeight;
        return i + transform.rect.y;
    }

    /// <summary>
    /// 前日に戻るために使う 今のチャット画面のポジションと日付の位置を比較してどのObjが前日かを決定する
    /// Intを返す
    /// </summary>
    /// <returns></returns>
    public int GetPrevDayPosY() {

        int i = 0;
        for (i = nextDaysList.Count - 1; 0 <= i; i--) {
            RectTransform targetRect = nextDaysList[i].transform.GetComponent<RectTransform>();
            float targetPosY = targetRect.localPosition.y + targetRect.rect.y;
            float contentPosY = - scrollRect.content.localPosition.y;
            if (targetPosY > contentPosY) {
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
        for (i = 0; nextDaysList.Count - 1  > i; i++) {
            RectTransform targetRect = nextDaysList[i].transform.GetComponent<RectTransform>();
            float targetPosY = targetRect.localPosition.y + targetRect.rect.y;
            float contentPosY = - scrollRect.content.localPosition.y;
            float chatAreaHeight = scrollRect.GetComponent<RectTransform>().rect.height;
            if (targetPosY < contentPosY) {
                i++;
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
