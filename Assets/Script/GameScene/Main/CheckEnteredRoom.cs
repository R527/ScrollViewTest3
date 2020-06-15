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
    private bool checkFullRoom;

    private void Start() {
        //人数制限をセットする
        if (DebugManager.instance.isDebug) {
            gameManager.numLimit = DebugManager.instance.numLimit;
        } else {
            gameManager.numLimit = RoomData.instance.numLimit;

        }

        
        if(gameManager.GetNum() >= gameManager.numLimit) {
            var propertiers = new ExitGames.Client.Photon.Hashtable();
            propertiers.Add("isCheckFullRoom", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
        }

        isSetUp = true;
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

            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "満室です。";
            Destroy(gameObject);
        } else {
            var propertiers = new ExitGames.Client.Photon.Hashtable();
            propertiers.Add("isCheckFullRoom", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] == null) {
            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
            return;
        }

        //自分がキック対象なら自ら退出するPopUpを出す
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]) {
            Debug.Log("退出処理");
            StopCoroutine(NetworkManager.instance.banPlayerKickOutOREnteredRoomCoroutine);
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "接続に問題がありました。";
            Destroy(gameObject);
        }



        Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
        Debug.Log(gameManager.GetNum());
        Debug.Log(gameManager.numLimit);
        //満室でもBanPlayerでもない場合
        if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] && gameManager.GetNum() < gameManager.numLimit) {
            gameManager.GameManagerSetUp();
            Debug.Log("通過");
            Destroy(gameObject);
        }
    }
}
