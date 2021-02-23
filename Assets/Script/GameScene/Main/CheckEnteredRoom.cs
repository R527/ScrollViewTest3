using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

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
        var propertiers = new ExitGames.Client.Photon.Hashtable();
        if (gameManager.GetNum() >= gameManager.numLimit && !PhotonNetwork.IsMasterClient) {
            propertiers.Add("isCheckEmptyRoom", EMPTYROOM.満室.ToString());
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
        } else {
            //満室ではない処理
            propertiers.Add("isCheckEmptyRoom", EMPTYROOM.入室許可.ToString());
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
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
        if (gameManager.GetNum() >= gameManager.numLimit) {
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
            obj.exitText.text = "接続に問題がありました。";
            Destroy(gameObject);
            return;
        }
        //満室でもBanPlayerでもなく部屋が空いていたら
        if (!isCheckEnteredRoom) {
            gameManager.GameManagerSetUp();
            isCheckEnteredRoom = true;
            var propertiers = new ExitGames.Client.Photon.Hashtable();
            propertiers.Add("isCheckEnteredRoom", isCheckEnteredRoom);
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);

            Destroy(gameObject);
        }
    }
}
