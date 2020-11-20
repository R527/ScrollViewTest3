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

    public bool ischeackCloseInputView = true;

    public Color[] filterColor;
    public Image filterImage;
    //public bool folding;//夕方～夜にかけて折り畳みを制限する


    // Start is called before the first frame update
    void Start()
    {
        //フィルターボタンの追加
        filterButton.onClick.AddListener(() => StartCoroutine(FilterButton()));
       
    }

    ///////////////////////
    ///メソッド関連
    ///////////////////////

    /// <summary>
    /// フィルター機能のONOFFを制御
    /// </summary>
    public IEnumerator FilterButton() {

        //PopUpを消す処理が終わるまで押せない
        if (!ischeackCloseInputView) {
            yield break;
        }

        //フィルターボタン
        if (!chatListManager.isfilter) {
            filterButtanText.text = "解除";
            chatListManager.isfilter = true;
            filterImage.color = filterColor[1];
            //既にInputViewが上にあるなら下の処理をしない
            if (!inputView.folding) {
                yield break;
            }
            //テキストが解除ならフィルターを解除する
        } else {
            filterButtanText.text = "フィルター";
            chatListManager.isfilter = false;
            filterImage.color = filterColor[0];
            chatListManager.OffFilter();
        }

        //ボタンのテキストが上向きならInputViewを上にあげる
        if (inputView.folding) {
            Debug.Log("上に");
            inputView.menberViewPopUpObj.SetActive(true);
            inputView.inputRectTransform.DOLocalMoveY(0, 0.5f);
            inputView.viewport.DOSizeDelta(new Vector2(202f, 270f), 0.5f);
            flodingButton.interactable = false;
            comingOutButton.interactable = false;

            inputView.folding = false;
            //inputView.foldingText.text = "↓";
            inputView.foldingImage.sprite = inputView.downBtnSprite;
            //フィルター中に押したらInputViewを閉じる
        } else if(!inputView.folding) {
            Debug.Log("下に");
            ischeackCloseInputView = false;
            StartCoroutine(CloseInputView());

            inputView.inputRectTransform.DOLocalMoveY(-70, 0.5f);
            //inputView.inputRectTransform.DOLocalMoveY(-67, 0.5f);
            inputView.viewport.DOSizeDelta(new Vector2(202f, 342f), 0.5f);
            flodingButton.interactable = true;
            comingOutButton.interactable = true;

            inputView.folding = true;
            //inputView.foldingText.text = "↑";
            inputView.foldingImage.sprite = inputView.upBtnSprite;
            StartCoroutine(inputView.PopUpFalse());
        }
    }


    public IEnumerator CloseInputView() {
        yield return new WaitForSeconds(0.5f);
        ischeackCloseInputView = true;
    }

}
