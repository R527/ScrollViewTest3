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
    public GameManager gameMnager;

    private void Start() {
        
    }


    //Update is called once per frame
    void Update() {
        //マスタークライアントなら除外
        if (PhotonNetwork.IsMasterClient) {
            gameMnager.GameManagerSetUp();
            Debug.Log("masuta");
            Destroy(gameObject);
            return;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] == null) {
            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
            return;
        }

        //自分がキック対象なら自ら退出するPopUpを出す
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]) {
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "接続に問題がありました。";
            Destroy(gameObject);
        }

        //部屋が満室なら自ら退出するPopUpを出す
        if (gameMnager.GetNum() >= gameMnager.numLimit) {
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "満室です。";
            Destroy(gameObject);
        }

        Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]);
        Debug.Log(gameMnager.GetNum());
        Debug.Log(gameMnager.numLimit);
        //満室でもBanPlayerでもない場合
        if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] && gameMnager.GetNum() <= gameMnager.numLimit) {
            gameMnager.GameManagerSetUp();
            Debug.Log("通過");
            Destroy(gameObject);
        }
    }
}
