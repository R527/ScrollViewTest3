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
    public int dayIndex;
    public ScrollRect scrollRect;


    //Button
    public Button topButton;
    public Button bottomButton;
    public Button nextDayButton;//翌日指定
    public Button prevDayButton;//前日指定

    public bool isCheckNormalizedPosition;

    public float contentPosY;


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
        //チャット画面が一番下かつチャットをtrueにしているならreturn
        if(!isCheckNormalizedPosition　&& scrollRect.verticalNormalizedPosition <= 0) {
            return;
        }

        //フィルター中でないときに画面の一番下に来たらチャットをtrueにする
        if(scrollRect.verticalNormalizedPosition <= 0 && !chatListManager.isfilter) {
            chatListManager.OffFilter();
            isCheckNormalizedPosition = false;
            return;
        }

        //チャット画面が一番下ではないときはチャットをfalseにする
        if (scrollRect.verticalNormalizedPosition > 0) {
            isCheckNormalizedPosition = true;
            return;
        }
    }

    /// <summary>
    /// 一番最初の日にちに戻る
    /// </summary>
    public void FirstDay() {
        dayIndex  = 0;
        ScrollToCore(nextDaysList[dayIndex], 1.0f);
    }

    /// <summary>
    /// 一番最後の日にち戻る
    /// </summary>
    public void LastDay() {
        //dayIndex = nextDaysList.Count - 1;
        //ScrollToCore(nextDaysList[dayIndex], 1.0f);
        scrollRect.verticalNormalizedPosition = 0f;
    }
    /// <summary>
    /// 1日戻る
    /// </summary>
    public void PrevDay() {
        dayIndex--;
        dayIndex = Mathf.Clamp(dayIndex, 0, nextDaysList.Count - 1);
        ScrollToCore(nextDaysList[dayIndex], 1.0f);
        Debug.Log("nextDaysList" + dayIndex);

    }

    public void NextDay() {
        dayIndex++;
        dayIndex = Mathf.Clamp(dayIndex, 0, nextDaysList.Count - 1);
        ScrollToCore(nextDaysList[dayIndex], 1.0f);
        Debug.Log("nextDaysList" + dayIndex);

    }
    /// <summary>
    /// 指定した日にちに移動
    /// </summary>
    /// <param name="obj">移動したい日にちのゲームオブジェクト</param>
    /// <param name="align">移動したゲームオブジェクトの配置場所、一番下が0.0f　中央が0.5f 一番下が1.0f 基本日付を一番上に配置するので1.0fで使う</param>
    /// <returns></returns>
    private float ScrollToCore(GameObject obj, float align) {
        RectTransform targetRect = obj.GetComponent<RectTransform>();
        //Contentの高さ取得
        float contentHeight = scrollRect.content.rect.height;
        //ContentのPosY
        contentPosY = scrollRect.content.localPosition.y;
        //Viewportの高さを取得
        float viewportHeight = scrollRect.viewport.rect.height;
        
        

        //Contentの高さがViewportの高さより小さい場合にはそれ以上スクロールできないので、スクロール不要
        if(contentHeight < viewportHeight) {
            return 0f;
        }
        Debug.Log("contetntPosY" + contentPosY);
        //Debug.Log("コンテント上辺の位置" + (viewportHeight - contentHeight));
        //ローカル座標がContentHeightの上辺を0として負の値で格納される
        float targetPos = contentHeight + GetPosY(targetRect,contentHeight) + targetRect.rect.height * align;

        //上端～下端合わせたのための調整量
        float gap = viewportHeight * align;

        //差分を計算する
        float normalizedPos = (targetPos - gap) / (contentHeight - viewportHeight);
        //Debug.Log("normalizedPos" + normalizedPos);
        //Clamp01を使ってFloatを０～1にする
        normalizedPos = Mathf.Clamp01(normalizedPos);

        //上記の情報を使ってVerticalNormalizedPositionを実行
        scrollRect.verticalNormalizedPosition = normalizedPos;
        //Debug.Log("contentHeight" + contentHeight);//正しい
        //Debug.Log("viewportHeight" + viewportHeight);//正しい
        Debug.Log("targetPos" + targetPos);
        Debug.Log("GetPosY(targetRect)" + GetPosY(targetRect, contentHeight));
        //Debug.Log("targetRect.rect.height" + targetRect.rect.height);//正しい
        //Debug.Log("gap" + gap);//正しい
        //Debug.Log("normalizedPos" + normalizedPos);
        return normalizedPos;
    }

    /// <summary>
    /// LocalPosison.yのPivotによるずれをRect.yで補正
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    private float GetPosY(RectTransform transform,float contentHeight) {
        
        //Debug.Log(transform.localPosition.y);//おかしい
        //Debug.Log(transform.rect.y);//正しい

        float i = transform.localPosition.y - contentHeight;
        Debug.Log(i);
        return i + transform.rect.y;
    }


}
