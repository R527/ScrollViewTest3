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



    public bool folding;//夕方～夜にかけて折り畳みを制限する


    // Start is called before the first frame update
    void Start()
    {
        //フィルターボタンの追加
        filterButton.onClick.AddListener(FilterButton);
       
    }

    ///////////////////////
    ///メソッド関連
    ///////////////////////

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
            flodingButton.interactable = false;
            comingOutButton.interactable = false;
            inputView.foldingText.text = "↓";

            //夕方から夜にかけて上下を制限する
        } else if(!folding){
            inputView.inputRectTransform.DOLocalMoveY(-67, 0.5f);
            inputView.mainRectTransform.DOLocalMoveY(0, 0.5f);
            flodingButton.interactable = true;
            comingOutButton.interactable = true;
            inputView.foldingText.text = "↑";
            StartCoroutine(inputView.PopUpFalse());
        }
    }


}
