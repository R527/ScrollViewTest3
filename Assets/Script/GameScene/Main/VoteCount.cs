using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 投票を集計し、処刑処理をする
/// </summary>
public class VoteCount : MonoBehaviourPunCallbacks {

    //class
    public ChatSystem chatSystem;
    public GameManager gameManager;

    //main
    public Dictionary<int,int> voteCountTable = new Dictionary<int, int>();
    public Dictionary<int, string> voteNameTable = new Dictionary<int, string>();
    public List<Photon.Realtime.Player> ExecutionPlayerList = new List<Photon.Realtime.Player>();
    public int mostVotes;
    public Photon.Realtime.Player mostVotePlayer;//処刑ナンバー
    public Player executionPlayer;
    public string executionPlayerName;
    public int executionID = 999;

    /// <summary>
    /// ListにInt型を人数に合わせて追加する　
    /// </summary>
    /// <param name="numLimit"></param>
    public void VoteCountListSetUp(int numLimit) {

        //投票用のListを作成
        for (int i = 0; i < numLimit; i++) {
            voteCountTable.Add(i + 1,0);
            voteNameTable.Add(i + 1,string.Empty);
        }

    }

    private void Start() {
        //カスタムプロパティ
        if (!gameManager.isOffline) {
            var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                {"executionID",executionID }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
            //Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["executionID"]);
        }
    }

    /// <summary>
    /// 処刑されたプレイヤーをセットする
    /// </summary>
    private void SetExecutionPlayerID() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"executionID", executionID},
            {"executionPlayerName", executionPlayerName },
            {"mostVotes",mostVotes }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log("executionID"+(int)PhotonNetwork.CurrentRoom.CustomProperties["executionID"]);
        Debug.Log("executionPlayerName" + (string)PhotonNetwork.CurrentRoom.CustomProperties["executionPlayerName"]);
        Debug.Log("mostVotes" + (int)PhotonNetwork.CurrentRoom.CustomProperties["mostVotes"]);
    }
    /// <summary>
    /// 処刑されたプレイヤーをもらう
    /// </summary>
    /// <returns></returns>
    public int GetExecutionPlayerID() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("executionID", out object executionIDObj)) {
            executionID = (int)executionIDObj;
        }
        return executionID;
    }
    /// <summary>
    /// 一番投票されたプレイヤーを処刑する
    /// </summary>
    public void Execution() {
        if (PhotonNetwork.IsMasterClient) {

            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {

                //    //投票を他プレイヤーと比べる
                //    for (int id = 0; id < gameManager.liveNum; id++) {
                //int count = voteCountList[id];

                //投票数が他プレイヤーと同数ならリストに追加
                Debug.Log(player.NickName);
                Debug.Log(player.CustomProperties["voteNum"]);
                if (player.CustomProperties["voteNum"] != null) {
                    if (mostVotes == (int)player.CustomProperties["voteNum"]) {
                        ExecutionPlayerList.Add(player);
                        //投票数が他プレイヤーより多いならListから削除して追加
                    } else if (mostVotes < (int)player.CustomProperties["voteNum"]) {
                        mostVotes = (int)player.CustomProperties["voteNum"];
                        ExecutionPlayerList.Clear();

                        ExecutionPlayerList.Add(player);
                    }
                }
            }

        //ランダム処刑処理
            if(ExecutionPlayerList.Count >= 2) {
                mostVotePlayer = ExecutionPlayerList[Random.Range(0, ExecutionPlayerList.Count)];
            } else {
                //最多投票が一人の場合
                mostVotePlayer = ExecutionPlayerList[0];
            }
            //決定したプレイヤーを処刑処理する
            foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                if (player.ActorNumber == mostVotePlayer.ActorNumber) {

                    var properties = new ExitGames.Client.Photon.Hashtable {
                            {"live", false}
                    };
                    player.SetCustomProperties(properties);
                    //プレイヤーの死亡処理
                    //playerObj.live = false;

                    //プレイヤーの共有

                    executionID = mostVotePlayer.ActorNumber;
                    executionPlayerName = mostVotePlayer.NickName;
                    SetExecutionPlayerID();
                    Debug.Log("executionID" + executionID);

                    ////マスターだけがIDをもらう
                    //if (PhotonNetwork.IsMasterClient) {
                    //executionID = GetExecutionPlayerID();
                    //Debug.Log("executionID" + executionID);
                    //}

                    //処刑されたプレイヤーをリストから削除する
                    //foreach (Player player in chatSystem.playersList) {
                    //    if (playerObj.playerID == executionPlayer.playerID) {

                        //chatSystem.playersList.Remove(playerObj);
                break;
                     }
                }
            }

        
        //マスター以外のプレイヤーに処刑したプレイヤーの死亡処理をする
        foreach (Player player in chatSystem.playersList) {
            if(GetExecutionPlayerID() == player.playerID) {
                player.live = false;
                break;
            }
        }
        
        gameManager.liveNum--;
        //生存数を更新
        gameManager.SetLiveNum();

        //生き残ったプレイヤーのVoteCountを０にする
        //foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
        //    var properties = new ExitGames.Client.Photon.Hashtable {
        //        {"voteNum", 0 }
        //    };
        //    player.SetCustomProperties(properties);
        //}


        ////ディクショナリーの初期化
        //foreach(int playerID in voteCountList.Keys) {
        //    voteCountList[playerID] = 0;
        //}



    }
}



