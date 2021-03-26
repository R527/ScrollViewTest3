using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;

/// <summary>
/// 部屋に入った直後にBanList、満室チェックなどを行う
/// </summary>
public class CheckEnteredRoom : MonoBehaviourPunCallbacks {

    public ExitPopUp exitPopUp;
    public Transform tran;
    public GameManager gameManager;
    private bool isSetUp;

    public bool isCheckEnteredRoom;//入室許可用　PlayerButton作成可能にする

    private void Start() {
        //マスター以外が入室したら一旦部屋を閉じる
        if (!PhotonNetwork.IsMasterClient) {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        //人数制限をセットする
        if (DebugManager.instance.isTimeController) {
            gameManager.numLimit = (int)PhotonNetwork.CurrentRoom.CustomProperties["numLimit"];
        } else {
            gameManager.numLimit = (int)PhotonNetwork.CurrentRoom.CustomProperties["numLimit"];
        }

        //満室ならフラグを立てる
        if (NetworkManager.instance.GetCustomPropertesOfRoom<int>("num") >= gameManager.numLimit && !PhotonNetwork.IsMasterClient) {
            NetworkManager.instance.SetCustomPropertesOfPlayer("isCheckEmptyRoom", EMPTYROOM.満室, PhotonNetwork.LocalPlayer);
        } else {
            //満室ではない処理
            NetworkManager.instance.SetCustomPropertesOfPlayer("isCheckEmptyRoom", EMPTYROOM.入室許可, PhotonNetwork.LocalPlayer);
        }
        isSetUp = true;
    }


    void Update() {


        //Startが終わるまでリターン
        if (!isSetUp) {
            return;
        }
        //マスタークライアントなら除外
        if (PhotonNetwork.IsMasterClient) {
            gameManager.GameManagerSetUp();
            Destroy(gameObject);
            return;
        }


        //部屋が満室なら自ら退出するPopUpを出す
        if (NetworkManager.instance.GetCustomPropertesOfRoom<int>("num") >= gameManager.numLimit) {
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "満室です。";
            Destroy(gameObject);
            return;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] == null) {
            return;
        }

        //自分がキック対象なら自ら退出するPopUpを出す
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]) {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            if (NetworkManager.instance.banPlayerKickOutOREnteredRoomCoroutine != null) {
                StopCoroutine(NetworkManager.instance.banPlayerKickOutOREnteredRoomCoroutine);
            }
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "接続に問題が\r\nありました。";
            
            Destroy(gameObject);
            return;
        }
        //満室でもBanPlayerでもなく部屋が空いていたら
        if (!isCheckEnteredRoom) {
            NetworkManager.instance.SetCustomPropertesOfPlayer("isForcedExit", false, PhotonNetwork.LocalPlayer);
            gameManager.GameManagerSetUp();
            isCheckEnteredRoom = true;
            NetworkManager.instance.SetCustomPropertesOfPlayer("isCheckEnteredRoom", isCheckEnteredRoom, PhotonNetwork.LocalPlayer);
            Destroy(gameObject);
        }
    }
}
