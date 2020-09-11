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

        var propertiers = new ExitGames.Client.Photon.Hashtable();
        //Debug.Log("gameManager.GetNum()" + gameManager.GetNum());
        //Debug.Log("gameManager.numLimit" + gameManager.numLimit);
        //満室ならフラグを立てる
        if (gameManager.GetNum() >= gameManager.numLimit && !PhotonNetwork.IsMasterClient) {
            Debug.Log("満室");
            propertiers.Add("isCheckEmptyRoom", EMPTYROOM.満室.ToString());
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
        } else {
            //満室ではない処理
            propertiers.Add("isCheckEmptyRoom", EMPTYROOM.入室許可.ToString());
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
        }

        isSetUp = true;
    }


    //Update is called once per frame
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

        
        Debug.Log("gameManager.GetNum()" + gameManager.GetNum());
        Debug.Log("gameManager.numLimit" + gameManager.numLimit);

        //部屋が満室なら自ら退出するPopUpを出す
        if (gameManager.GetNum() >= gameManager.numLimit) {
            Debug.Log("満室");
            //PhotonNetwork.CurrentRoom.IsOpen = true;
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "満室です。";
            Destroy(gameObject);
            return;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] == null) {
            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
            return;
        }

        //自分がキック対象なら自ら退出するPopUpを出す
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]) {
            Debug.Log("退出処理");
            PhotonNetwork.CurrentRoom.IsOpen = true;
            Debug.Log(NetworkManager.instance);
            if (NetworkManager.instance.banPlayerKickOutOREnteredRoomCoroutine != null) {
                StopCoroutine(NetworkManager.instance.banPlayerKickOutOREnteredRoomCoroutine);
            }
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "接続に問題がありました。";
            Destroy(gameObject);
            return;
        }


        Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
        Debug.Log(gameManager.GetNum());
        Debug.Log(gameManager.numLimit);
        //満室でもBanPlayerでもなく部屋が空いていたら
        //if (gameManager.GetNum() < gameManager.numLimit ) {

        if (!isCheckEnteredRoom) {
            Debug.Log("isCheckEnteredRoom");
            gameManager.GameManagerSetUp();
            isCheckEnteredRoom = true;
            var propertiers = new ExitGames.Client.Photon.Hashtable();
            propertiers.Add("isCheckEnteredRoom", isCheckEnteredRoom);
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);

            Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isCheckEnteredRoom"]);
            Destroy(gameObject);

        }

    }
}
