using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fillter : MonoBehaviour
{

    //class
    public ChatListManager chatListManager;
    public InputView inputView;

    //fillter
    public Text filterButtanText;
    public Button filterButton;
    public Button flodingButton;
    public Button comingOutButton;

    //wolfMode（狼チャットの制御関連
    public Button wolfModeButton;
    public Text wolfModeButtonText;
    public bool wolfMode;//trueで狼モード
 


    // Start is called before the first frame update
    void Start()
    {
        //フィルターボタンの追加
        filterButton.onClick.AddListener(FilterButton);
        wolfModeButton.onClick.AddListener(WolfMode);
    }

    /// <summary>
    /// フィルター機能のONOFFを制御
    /// </summary>
    public void FilterButton() {
        Debug.Log("filter");

        //フィルターボタン
        if (filterButtanText.text == "フィルター") {
            filterButtanText.text = "解除";
            chatListManager.isfilter = true;
            if (inputView.foldingText.text == "↓") {
                return;
            }
        } else {
            filterButtanText.text = "フィルター";
            chatListManager.isfilter = false;
            chatListManager.OffFilter();
        }

        //MenbarViewの上下ボタン
        if (inputView.foldingText.text == "↑") {
            inputView.menberViewPopUpObj.SetActive(true);
            inputView.inputRectTransform.DOLocalMoveY(0, 0.5f);
            inputView.mainRectTransform.DOLocalMoveY(72, 0.5f);
            inputView.stampButton.interactable = false;
            flodingButton.interactable = false;
            comingOutButton.interactable = false;
            inputView.foldingText.text = "↓";
        } else {
            inputView.inputRectTransform.DOLocalMoveY(-67, 0.5f);
            inputView.mainRectTransform.DOLocalMoveY(0, 0.5f);
            inputView.stampButton.interactable = true;
            flodingButton.interactable = true;
            comingOutButton.interactable = true;
            inputView.foldingText.text = "↑";
            StartCoroutine(inputView.PopUpFalse());
        }
    }

    /// <summary>
    /// 狼チャットのONOF
    /// </summary>
    public void WolfMode() {

        //On
        if (wolfModeButtonText.text == "市民") {
            wolfModeButtonText.text = "狼";
            wolfMode = true;

            chatListManager.OnWolfMode();
            //Off
        } else {
            wolfModeButtonText.text = "市民";
            wolfMode = false;
            chatListManager.OffWolfMode();

        }
    }
}
