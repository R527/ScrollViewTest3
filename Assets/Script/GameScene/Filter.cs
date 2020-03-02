using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


/// <summary>
/// フィルターのためにしたのメンバースクロールビューを上下する
/// </summary>
public class Filter : MonoBehaviour
{
    //class
    public ChatListManager chatListManager;
    public InputView inputView;
    //main
    public Text filterButtanText;
    public Button filterButton;
    public Button flodingButton;
    public Button comingOutButton;
    public GameObject menberPopUp;
    public GameObject coPopUp;


    private void Start() {
        filterButton.onClick.AddListener(FilterButton);
    }
    public void FilterButton() {
        Debug.Log("filter");
        if (filterButtanText.text == "フィルター") {
            filterButtanText.text = "解除";
            chatListManager.isfilter = true;
        } else {
            filterButtanText.text = "フィルター";
            chatListManager.isfilter = false;
            chatListManager.OffFilter();
        }
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
            comingOutButton.interactable = false;
            inputView.foldingText.text = "↑";
            StartCoroutine(inputView.PopUpFalse());
        }
    }

}
