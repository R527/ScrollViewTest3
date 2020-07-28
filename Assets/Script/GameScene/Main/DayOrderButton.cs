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

    public List<GameObject> nextDaysList = new List<GameObject>();
    public int dayIndex;
    public ScrollRect scrollRect;

    //Button
    public Button topButton;
    public Button bottomButton;
    public Button nextDayButton;//翌日指定
    public Button prevDayButton;//前日指定

    // Start is called before the first frame update
    void Start()
    {
        topButton.onClick.AddListener(FirstDay);
        bottomButton.onClick.AddListener(LastDay);
        nextDayButton.onClick.AddListener(NextDay);
        prevDayButton.onClick.AddListener(PrevDay);
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
        dayIndex = nextDaysList.Count - 1;
        ScrollToCore(nextDaysList[dayIndex], 1.0f);

    }
    /// <summary>
    /// 1日戻る
    /// </summary>
    public void PrevDay() {
        dayIndex--;
        dayIndex = Mathf.Clamp(dayIndex, 0, nextDaysList.Count - 1);
        ScrollToCore(nextDaysList[dayIndex], 1.0f);

    }

    public void NextDay() {
        dayIndex++;
        dayIndex = Mathf.Clamp(dayIndex, 0, nextDaysList.Count - 1);
        ScrollToCore(nextDaysList[dayIndex], 1.0f);
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
        //Viewportの高さを取得
        float viewportHeight = scrollRect.viewport.rect.height;

        //Contentの高さがViewportの高さより小さい場合にはそれ以上スクロールできないので、スクロール不要
        if(contentHeight < viewportHeight) {
            return 0f;
        }

        //ローカル座標がContentHeightの上辺を0として負の値で格納される
        float targetPos = contentHeight + GetPosY(targetRect) + targetRect.rect.height * align;

        //上端～下端合わせたのための調整量
        float gap = viewportHeight * align;

        //差分を計算する
        float normalizedPos = (targetPos - gap) / (contentHeight - viewportHeight);
        Debug.Log("normalizedPos" + normalizedPos);
        //Clamp01を使ってFloatを０～1にする
        normalizedPos = Mathf.Clamp01(normalizedPos);

        //上記の情報を使ってVerticalNormalizedPositionを実行
        scrollRect.verticalNormalizedPosition = normalizedPos;
        Debug.Log("contentHeight" + contentHeight);
        Debug.Log("viewportHeight" + viewportHeight);
        Debug.Log("targetPos" + targetPos);
        Debug.Log("GetPosY(targetRect)" + GetPosY(targetRect));
        Debug.Log("targetRect.rect.height" + targetRect.rect.height);
        Debug.Log("gap" + gap);
        Debug.Log("normalizedPos" + normalizedPos);
        return normalizedPos;
    }

    /// <summary>
    /// LocalPosison.yのPivotによるずれをRect.yで補正
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    private float GetPosY(RectTransform transform) {
        Debug.Log(transform.localPosition.y);
        Debug.Log(transform.rect.y);
        return - (transform.localPosition.y + transform.rect.y);
    }
}
