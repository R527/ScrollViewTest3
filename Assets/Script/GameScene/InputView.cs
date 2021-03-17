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
    public RectTransform viewport;
    public RectTransform inputRectTransform;
    public Button filterButton;
    public Button foldingButton;
    public Button comingButton;
    public GameObject menberViewPopUpObj;//メンバーの入ってるPrefab
    public GameObject coPopUpObj;//Coの入っているPrefab
    public Button comingOutButton;

    //折畳ボタン
    public Sprite upBtnSprite;
    public Sprite downBtnSprite;
    public Image foldingImage;
    public bool folding;//trueなら上向き

    //wolfMode（狼チャットの制御関連
    public Button wolfModeButton;
    public Image wolfBtnImage;
    public Text wolfModeButtonText;
    public bool wolfMode;//trueで狼モード


    //superChatButton
    public Button superChatButton;
    public bool superChat;
    public GameObject moneyImage;
    public Image superChatBtnImage;

    //
    public Color[] btnColor;

    private void Start() {
        foldingButton.onClick.AddListener(FoldingPosition);
        comingButton.onClick.AddListener(ComingOut);

        wolfModeButton.onClick.AddListener(WolfMode);
        superChatButton.onClick.AddListener(() => StartCoroutine(SuperChat()));

        if (PhotonNetwork.IsMasterClient) {
            superChatButton.interactable = true;
        }
    }
    /// <summary>
    /// 折り畳みボタンの制御
    /// </summary>
    public void FoldingPosition() {

        //COPopUpが有効でかつ
        if (coPopUpObj.activeSelf && !folding) {
            coPopUpObj.SetActive(false);
            menberViewPopUpObj.SetActive(true);
            //ボタン有効にする
            availabilityButton();
            return;
        } else if (menberViewPopUpObj.activeSelf && !folding) {
            //InputViewを下げるボタン
            inputRectTransform.DOLocalMoveY(-70, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 330f), 0.5f);
            filterButton.interactable = true;
            
            //ボタン有効にする
            availabilityButton();

            //stampButton.interactable = true;
            folding = true;
            foldingImage.sprite = upBtnSprite;
            StartCoroutine(PopUpFalse());
        } else if (folding) {

            //InputViewをあげるボタン
            menberViewPopUpObj.SetActive(true);
            inputRectTransform.DOLocalMoveY(0, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 258f), 0.5f);
            filterButton.interactable = true;
            folding = false;
            foldingImage.sprite = downBtnSprite;
        }
    }



    /// <summary>
    /// カミングアウトボタンを制御
    /// </summary>
    public void ComingOut() {
        Debug.Log("ComingOut");
        //メンバーが表示されている状態でCOボタン押したときの処理
        if (menberViewPopUpObj.activeSelf && !folding) {
            Debug.Log("メンバーが表示されている状態でCOボタン押したときの処理");

            coPopUpObj.SetActive(true);
            menberViewPopUpObj.SetActive(false);

            //ほかのボタン無効
            InvalidButton();

            //COPopUpがアクティブ状態の時の処理
        } else if (coPopUpObj.activeSelf && !folding) {
            Debug.Log("COPopUpがアクティブ状態の時の処理");

            inputRectTransform.DOLocalMoveY(-70, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 330f), 0.5f);
            filterButton.interactable = true;
            folding = true;
            foldingImage.sprite = upBtnSprite;

            //ボタン有効にする
            availabilityButton();

            StartCoroutine(PopUpFalse());

            //InPutViewが閉じている時
        } else if (folding) {
            Debug.Log("InPutViewが閉じている時");
            coPopUpObj.SetActive(true);
            inputRectTransform.DOLocalMoveY(0, 0.5f);
            viewport.DOSizeDelta(new Vector2(202f, 258f), 0.5f);
            filterButton.interactable = false;

            //ほかのボタン無効
            InvalidButton();
            folding = false;
            foldingImage.sprite = downBtnSprite;
        }
    }

    /// <summary>
    /// 狼チャットのONOF
    /// </summary>
    public void WolfMode() {

        //On
        if (wolfModeButtonText.text == "市民") {
            wolfBtnImage.color = btnColor[2];
            wolfModeButtonText.text = "狼";
            wolfMode = true;
            chatListManager.OnWolfMode();
            //Off
        } else {
            wolfBtnImage.color = btnColor[0];
            wolfModeButtonText.text = "市民";
            wolfMode = false;
            chatListManager.OffWolfMode();
        }
    }

    /// <summary>
    /// 青チャットの制御
    /// </summary>
    public IEnumerator SuperChat() {

        //Onにするとき
        if (!superChat) {
            chatListManager.gameManager.InstantiateCurrencyTextPopUP("superChatStr");
            yield return new WaitUntil(() => chatListManager.gameManager.showPopUp);

            //利用額とゲーム内通貨の残高を比較して購入できないなら別のPopUpを呼び出す
            if (chatListManager.gameManager.superChatCurrency > PlayerManager.instance.currency && (chatListManager.gameManager.chatSystem. superChatCount >= 3 || PlayerManager.instance.subscribe)) {
                moneyImage.SetActive(true);
                yield break;
            }

            superChatBtnImage.color = btnColor[1];
            superChat = true;
            //Offにするとき
        } else {
            superChatBtnImage.color = btnColor[0];
            superChat = false;
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
        wolfBtnImage.color = btnColor[0];
        wolfModeButtonText.text = "市民";
        wolfMode = false;
        wolfModeButton.interactable = false;

        superChatBtnImage.color = btnColor[0];
        superChat = false;
        superChatButton.interactable = false;
    }

    private void availabilityButton() {
        //ボタン有効にする

        //プレイ中かつ狼ならtrueにする
        if(chatListManager.gameManager.timeController.isPlay && chatListManager.gameManager.chatSystem.myPlayer.wolfChat && chatListManager.gameManager.chatSystem.myPlayer.live) {
            wolfModeButton.interactable = true;
        }
        superChatButton.interactable = true;
    }
}
