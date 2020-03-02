using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


/// <summary>
/// Input部分の折り畳みボタンとCOボタンスタンプボタン等を制御
/// </summary>
public class InputView : MonoBehaviour {

    //main
    public Text foldingText; //折り畳みボタンのテキスト
    public RectTransform mainRectTransform;
    public RectTransform inputRectTransform;
    public Button filterButton;
    public Button stampButton;
    public Button foldingButton;
    public Button comingButton;
    public GameObject menberViewPopUpObj;
    public GameObject coPopUpPbj;
    public bool change;


    private void Start() {
        foldingButton.onClick.AddListener(FoldingPosition);
        comingButton.onClick.AddListener(ComingOut);
    }
    /// <summary>
    /// 折り畳みボタンの制御
    /// </summary>
    public void FoldingPosition() {
        if (coPopUpPbj.activeSelf && foldingText.text == "↓") {
            //change = false;
            coPopUpPbj.SetActive(false);
            menberViewPopUpObj.SetActive(true);
            return;
        } else if (menberViewPopUpObj.activeSelf && foldingText.text == "↓") {
            inputRectTransform.DOLocalMoveY(-67, 0.5f);
            mainRectTransform.DOLocalMoveY(0, 0.5f);
            filterButton.interactable = true;
            stampButton.interactable = true;
            foldingText.text = "↑";
            //change = true; 
            StartCoroutine(PopUpFalse());
        } else if (foldingText.text == "↑") {
            //change = false;
            menberViewPopUpObj.SetActive(true);
            inputRectTransform.DOLocalMoveY(0, 0.5f);
            mainRectTransform.DOLocalMoveY(72, 0.5f);
            filterButton.interactable = false;
            stampButton.interactable = false;
            foldingText.text = "↓";
        }

        //Unityで用意されている数字などは直接変更してはいけない。
        //float PosY = 84;
        //Vector3 temp = rectTransform.localPosition;
        //temp.y = PosY;
        //rectTransform.localPosition = temp;
    }

    /// <summary>
    /// カミングアウトボタンを制御
    /// </summary>
    public void ComingOut() {
        if (menberViewPopUpObj.activeSelf && foldingText.text == "↓") {
            //change = false;
            coPopUpPbj.SetActive(true);
            menberViewPopUpObj.SetActive(false);
        }else if (coPopUpPbj.activeSelf && foldingText.text == "↓") {
            inputRectTransform.DOLocalMoveY(-67, 0.5f);
            mainRectTransform.DOLocalMoveY(0, 0.5f);
            filterButton.interactable = true;
            stampButton.interactable = true;
            foldingText.text = "↑";
            //change = true;
            StartCoroutine(PopUpFalse());
        } else if (foldingText.text == "↑") {
            //change = false;
            coPopUpPbj.SetActive(true);
            inputRectTransform.DOLocalMoveY(0, 0.5f);
            mainRectTransform.DOLocalMoveY(72, 0.5f);
            filterButton.interactable = false;
            stampButton.interactable = false;
            foldingText.text = "↓";
        }
    }

    public IEnumerator PopUpFalse() {
        yield return new WaitForSeconds(0.5f);
        menberViewPopUpObj.SetActive(false);
        coPopUpPbj.SetActive(false);
    }

}
