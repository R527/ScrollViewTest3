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
    public List<int> voteCountList = new List<int>();
    public List<Player> ExecutionPlayerList = new List<Player>();
    public int mostVotes;
    public Player mostVotePlayer;//処刑ナンバー
    public Player executionPlayer;
    public int executionID = 999;

    /// <summary>
    /// ListにInt型を人数に合わせて追加する　
    /// </summary>
    /// <param name="numLimit"></param>
    public void VoteCountListSetUp(int numLimit) {

        //投票用のListを作成
        for (int i = 0; i < numLimit; i++) {
            voteCountList.Add(0);
        }

    }

    private void Start() {
        //カスタムプロパティ
        if (!gameManager.isOffline) {
            var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                {"executionID",executionID }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
            Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["executionID"]);
        }
    }

    /// <summary>
    /// 処刑されたプレイヤーをセットする
    /// </summary>
    private void SetExecutionPlayerID() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"executionID", executionID}
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["executionID"]);
    }
    /// <summary>
    /// 処刑されたプレイヤーをもらう
    /// </summary>
    /// <returns></returns>
    private int GetExecutionPlayerID() {
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

        foreach (Player playerObj in chatSystem.playersList) {
            
        //    //投票を他プレイヤーと比べる
        //    for (int id = 0; id < gameManager.liveNum; id++) {
        //int count = voteCountList[id];

            //投票数が他プレイヤーと同数ならリストに追加
            if (mostVotes == playerObj.voteNum) {
            ExecutionPlayerList.Add(chatSystem.playersList[playerObj.playerID]);
            //投票数が他プレイヤーより多いならListから削除して追加
            } else if(mostVotes < playerObj.voteNum) {
                mostVotes = playerObj.voteNum;
                ExecutionPlayerList.Clear();
                ExecutionPlayerList.Add(chatSystem.playersList[playerObj.playerID]);
            }
        }

        //ランダム処刑処理or処刑処理
        if(ExecutionPlayerList.Count >= 2) {
            mostVotePlayer = ExecutionPlayerList[Random.Range(0, ExecutionPlayerList.Count)];
        } else {
            //最多投票が一人の場合
            mostVotePlayer = ExecutionPlayerList[0];
        }
        //決定したプレイヤーを処刑処理する
        foreach(Player playerObj in chatSystem.playersList) {
            if (playerObj.playerID == mostVotePlayer.playerID) {
                playerObj.live = false;

                //プレイヤーの共有

                executionID = mostVotePlayer.playerID;
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
                //}
            }
        }
        
        
        gameManager.liveNum--;
        //生存数を更新
        gameManager.SetLiveNum();

        //生き残ったプレイヤーのVoteCountを０にする
        foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            var properties = new ExitGames.Client.Photon.Hashtable {
                {"voteCount", 0 }
            };
            player.SetCustomProperties(properties);
        }

    }
}



