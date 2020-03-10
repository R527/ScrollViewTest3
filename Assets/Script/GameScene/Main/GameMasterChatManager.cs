using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class GameMasterChatManager : MonoBehaviourPunCallbacks {

    //class
    public TimeController timeController;
    public GameManager gameManager;

    //main
    public Button timeSavingButton;
    public bool timeSaving;//時短用　希望の場合true
    public int timeSavingNum;
    // Start is called before the first frame update
    void Start()
    {
        timeSavingButton.onClick.AddListener(() => TimeSavingChat());

        //カスタムプロパティ
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"timeSavingNum",timeSavingNum }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// タイムコントローラのシーンが変更するたびに発言するチャット
    /// </summary>
    public string　TimeManagementChat()
    {
        string gmNode = string.Empty;
        switch (timeController.timeType)
        {
            case TIME.昼:
                gmNode = "おはようございます。" + "昨夜は○○が●されました。";
                break;
            case TIME.投票時間:
                gmNode = "投票の時間です";
                break;
            case TIME.夜の行動:
                gmNode = "占え";
                break;
        }
        return gmNode;
    }

    /// <summary>
    /// 時短希望人数をチェックする
    /// </summary>
    private void CheckTimeSavingNum() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timeSavingNum", out object timeSavingNumObj)) {
            timeSavingNum = (int)timeSavingNumObj;
        }
    }
    /// <summary>
    /// 時短のGMチャットを制御
    /// </summary>
    /// <returns></returns>
    public string TimeSavingChat() {
        string gmNode = string.Empty;
        if (!gameManager.isOffline) {
            CheckTimeSavingNum();
        }
        //キャンセルorまだ希望していない状態なら
        if (!timeSaving) {
            timeSavingNum++;
            gmNode = PhotonNetwork.LocalPlayer.NickName + "さんが時短を希望しました。" + timeSavingNum + "/" + gameManager.liveNum + "※過半数を超えると時短されます。";
        } else {
            timeSavingNum--;
            gmNode = PhotonNetwork.LocalPlayer.NickName + "さんが時短をキャンセルしました。" + timeSavingNum + "/" + gameManager.liveNum + "※過半数を超えると時短されます。";
        }
        //timeSavingNum更新
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"timeSavingNum",timeSavingNum }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["timeSavingNum"]);

        if (gameManager.liveNum / 2 + 1 <= timeSavingNum) {
            timeController.totalTime = 0;
            Debug.Log("時短成立");
        } else {
            Debug.Log("時短不成立");
        }
        return gmNode;
    }

}
