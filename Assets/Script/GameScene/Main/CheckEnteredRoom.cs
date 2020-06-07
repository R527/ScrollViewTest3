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


    // Update is called once per frame
    void Update()
    {
        //マスタークライアントなら除外
        if (PhotonNetwork.IsMasterClient) {
            return;
        }

        if(PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"] == null) {
            return;
        }

        //自分がキック対象なら自ら退出するPopUpを出す
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]) {
            ExitPopUp obj = Instantiate(exitPopUp, tran, false);
            obj.exitText.text = "接続に問題がありました。";
            Destroy(gameObject);
        }

        //部屋が満室なら自ら退出するPopUpを出す
        //if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isBanPlayer"]) {
        //    ExitPopUp obj = Instantiate(exitPopUp, tran, false);
        //    obj.exitText.text = "満室です。";
        //    Destroy(gameObject);
        //}
    }
}
