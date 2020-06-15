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

    private void Start() {
        //人数制限をセットする
        if (DebugManager.instance.isDebug) {
            gameManager.numLimit = DebugManager.instance.numLimit;
        } else {
            gameManager.numLimit = RoomData.instance.numLimit;

        }
        
        //満室ならフラグを立てる
        if(gameManager.GetNum() >= gameManager.numLimit) {
            var propertiers = new ExitGames.Client.Photon.Hashtable();
            propertiers.Add("isCheckEmptyRoom", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
        }

        isSetUp = true;

        //マスター以外が入室したら一旦部屋を閉じる
        if (!PhotonNetwork.IsMasterClient) {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }


    //Update is called once per frame
    void Update() {

        //マスタークライアントなら除外
        if (PhotonNetwork.IsMasterClient) {
            gameManager.GameManagerSetUp();
            Destroy(gameObject);
            return;
        }

        //Startが終わるまでリターン
        if (!isSetUp) {
            return;
        }

        Debug.Log(gameManager.GetNum());
        Debug.Log(gameManager.numLimit);

        //部屋が満室なら自ら退出するPopUpを出す
        if (gameManager.GetNum() >= gameManager.numLimit) {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "満室です。";
            Destroy(gameObject);
        } else {
            var propertiers = new ExitGames.Client.Photon.Hashtable();
            propertiers.Add("isCheckEmptyRoom", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] == null) {
            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
            return;
        }

        //自分がキック対象なら自ら退出するPopUpを出す
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]) {
            Debug.Log("退出処理");
            PhotonNetwork.CurrentRoom.IsOpen = true;
            StopCoroutine(NetworkManager.instance.banPlayerKickOutOREnteredRoomCoroutine);
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "接続に問題がありました。";
            Destroy(gameObject);
        }


        Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
        Debug.Log(gameManager.GetNum());
        Debug.Log(gameManager.numLimit);
        //満室でもBanPlayerでもなく部屋が空いていたら
        //if (gameManager.GetNum() < gameManager.numLimit ) {
            gameManager.GameManagerSetUp();
            Destroy(gameObject);
        //} 
    }
}
