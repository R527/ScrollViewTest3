using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 意味不明
/// </summary>
public class ChangeDay : MonoBehaviour {

    // とちゅうなりよ
    public NextDay nextDay;
    public GameObject content;
    public TimeController timeController;
    public float x, y;
    public ScrollRect scrollRect;
    public Button previewButton;

    private void Start() {
        previewButton.onClick.AddListener(() => ScrollToPreviewDay(timeController.day));
    }
    public void ScrollToTop() {
        //float PosY = timeController.nextDayList[0].GetComponent<RectTransform>().localPosition.y;
        //timeController.nextDayList[0].position;
        scrollRect.verticalNormalizedPosition = 1.0f;
    }

    public void ScrollToPreviewDay(int day) {
        //Vector3 scrollViewTfm = timeController.nextDayList[0];
        //Debug.Log(scrollViewTfm);
        Debug.Log(day);
        if(day <= 1) {
            return;
        }
        RectTransform scrollViewTfm = timeController.nextDayList[day - 2].GetComponent<RectTransform>();
        Debug.Log(scrollViewTfm.localPosition);
        //float height = scrollViewTfm.localPosition.y;
        //float contentHeight = content.GetComponent<RectTransform>().sizeDelta.y;
        float contentHeight = scrollRect.content.rect.height;//コンテントの高さ
        float viewportHeight = scrollRect.viewport.rect.height;
        if (contentHeight <= viewportHeight) {
            scrollRect.verticalNormalizedPosition = 0f;
            return;
        }

        float targetPos = contentHeight + GetPosY(scrollViewTfm) + scrollViewTfm.rect.height;
        float normalizedPos = 1 - ((contentHeight - viewportHeight) / targetPos);

        normalizedPos = Mathf.Clamp01(normalizedPos);
        scrollRect.verticalNormalizedPosition = normalizedPos;
        Debug.Log(normalizedPos);
        Debug.Log(contentHeight);
        Debug.Log(viewportHeight);
        Debug.Log(targetPos);
        //float potision = 1 - (height / contentHeight);
        //Debug.Log(potision);
        //Debug.Log(height);
        //Debug.Log(contentHeight);

        //scrollRect.verticalNormalizedPosition = potision;



    }

    private float GetPosY(RectTransform transform) {
        return transform.localPosition.y + transform.rect.y;
    }
    public void ScrollToBottom() {
        scrollRect.verticalNormalizedPosition = 0f;
    }
}

