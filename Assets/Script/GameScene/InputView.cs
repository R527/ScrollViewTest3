using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

/// <summary>
/// Input部分の折り畳みボタンとCOボタンスタンプボタン等を制御
/// </summary>
public class InputView : MonoBehaviour {

    //class
    public ChatListManager chatListManager;

    //main
    public Text foldingText; //折り畳みボタンのテキスト
    public RectTransform viewport;
    public RectTransform inputRectTransform;
    public Button filterButton;
    public Button stampButton;
    public Button foldingButton;
    public Button comingButton;
    public GameObject menberViewPopUpObj;//メンバーの入ってるPrefab
    public GameObject coPopUpObj;//Coの入っているPrefab
    public Button comingOutButton;

    //wolfMode（狼チャットの制御関連
    public Button wolfModeButton;
    public Text wolfModeButtonText;
    public bool wolfMode;//trueで狼モード


    //superChatButton
    public Button superChatButton;
    public Text superChatButtonText;
    public bool superChat;
    public GameObject moneyImage;


    private void Start() {
        foldingButton.onClick.AddListener(FoldingPosition);
        comingButton.onClick.AddListener(ComingOut);

        wolfModeButton.onClick.AddListener(WolfMode);
        superChatButton.onClick.AddListener(SuperChat);

        if (PhotonNetwork.IsMasterClient) {
            superChatButton.interactable = true;
        }
    }
    /// <summary>
    /// 折り畳みボタンの制御
    /// </summary>
    public void FoldingPosition() {

        //COPopUp
        if (coPopUpObj.activeSelf && foldingText.text == "↓") {
            coPopUpObj.SetActive(false);
            menberViewPopUpObj.SetActive(true);

            //ボタン有効にする
            availabilityButton();

            return;
        } else if (menberViewPopUpObj.activeSelf && foldingText.text == "↓" ) {
            inputRectTransform.DOLocalMoveY(-67, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 342f), 0.5f);
            filterButton.interactable = true;

            //ボタン有効にする
            availabilityButton();

            //stampButton.interactable = true;
            foldingText.text = "↑";
            StartCoroutine(PopUpFalse());
        } else if (foldingText.text == "↑") {
            menberViewPopUpObj.SetActive(true);
            inputRectTransform.DOLocalMoveY(0, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 270f), 0.5f);
            filterButton.interactable = true;
            //stampButton.interactable = false;
            foldingText.text = "↓";
        }
    }



    /// <summary>
    /// カミングアウトボタンを制御
    /// </summary>
    public void ComingOut() {
        //メンバーが表示されている状態でCOボタン押したときの処理
        if (menberViewPopUpObj.activeSelf && foldingText.text == "↓") {
            coPopUpObj.SetActive(true);
            menberViewPopUpObj.SetActive(false);

            //ほかのボタン無効
            InvalidButton();

            //COPopUpがアクティブ状態の時の処理
        } else if (coPopUpObj.activeSelf && foldingText.text == "↓") {
            inputRectTransform.DOLocalMoveY(-67, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 342f), 0.5f);
            filterButton.interactable = true;
            //stampButton.interactable = true;
            foldingText.text = "↑";

            //ボタン有効にする
            availabilityButton();

            StartCoroutine(PopUpFalse());

            //InPutViewが閉じている時
        } else if (foldingText.text == "↑") {
            coPopUpObj.SetActive(true);
            inputRectTransform.DOLocalMoveY(0, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 270f), 0.5f);
            filterButton.interactable = false;

            //ほかのボタン無効
            InvalidButton();
            //stampButton.interactable = false;
            foldingText.text = "↓";
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

    /// <summary>
    /// 青チャットの制御
    /// </summary>
    public void SuperChat() {

        //On
        if (superChatButtonText.text == "通常") {

            int currency = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.currency.ToString(), 0);
            //利用額とゲーム内通貨の残高を比較して購入できないなら別のPopUpを呼び出す
            //仮で10消費する
            if (10 > currency) {
                moneyImage.SetActive(true);
                return;
            }

            superChatButtonText.text = "青";
            superChat = true;
            Debug.Log("superChat" + superChat);
            //Off
        } else {
            superChatButtonText.text = "通常";
            superChat = false;
            Debug.Log("superChat" + superChat);

        }
    }

    public IEnumerator PopUpFalse() {
        yield return new WaitForSeconds(0.5f);
        menberViewPopUpObj.SetActive(false);
        coPopUpObj.SetActive(false);
    }


    /// <summary>
    /// Coボタン押したときにそれぞれのボタンを無効化する
    /// </summary>
    private void InvalidButton() {
        //狼ボタンとスパーチャットボタンを無効にする
        wolfModeButtonText.text = "市民";
        wolfMode = false;
        wolfModeButton.interactable = false;
        superChatButtonText.text = "通常";
        superChat = false;
        superChatButton.interactable = false;
    }

    private void availabilityButton() {
        //ボタン有効にする

        //プレイ中かつ狼ならtrueにする
        if(chatListManager.gameManager.timeController.isPlay && chatListManager.gameManager.chatSystem.myPlayer.wolfChat) {
            wolfModeButton.interactable = true;

        }
        superChatButton.interactable = true;
    }
}
