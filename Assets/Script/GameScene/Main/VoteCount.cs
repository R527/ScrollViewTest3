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
    public List<Photon.Realtime.Player> ExecutionPlayerList = new List<Photon.Realtime.Player>();
    public int mostVotes;
    public Photon.Realtime.Player mostVotePlayer;//処刑したプレイヤー
    public string executionPlayerName;
    public int executionID = 999;

    //private void Start() {
    //    NetworkManager.instance.SetCustomPropertesOfRoom<int>("executionID",executionID);
    //    //var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
    //    //    {"executionID",executionID }
    //    //};
    //    //PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    //}

    
    /// <summary>
    /// 一番投票されたプレイヤーを処刑する
    /// </summary>
    public void Execution() {
        if (PhotonNetwork.IsMasterClient) {
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                //投票数が他プレイヤーと同数ならリストに追加
                if (player.CustomProperties["voteNum"] != null && (int)player.CustomProperties["voteNum"] != 0) {
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
            if (ExecutionPlayerList.Count >= 2) {
                mostVotePlayer = ExecutionPlayerList[Random.Range(0, ExecutionPlayerList.Count)];
            } else if (ExecutionPlayerList.Count == 0) {
                //全てのプレイヤーが投票が0票だった場合　生存しているプレイヤーからランダムに処刑する
                foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                    if ((bool)player.CustomProperties["live"]) {
                        ExecutionPlayerList.Add(player);
                    }
                }
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
                    //プレイヤーの共有

                    executionID = mostVotePlayer.ActorNumber;
                    executionPlayerName = mostVotePlayer.NickName;
                    SetExecutionPlayerID();
                    break;
                    }
                }
            }

        
        //マスター以外のプレイヤーに処刑したプレイヤーの死亡処理をする
        foreach (Player player in chatSystem.playersList) {
            if(NetworkManager.instance.GetCustomPropertesOfRoom<int>("executionID") == player.playerID) {
                player.live = false;
                break;
            }
        }

        //PlayerButtonの死亡処理
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerButton");
        foreach (GameObject player in objs) {
            PlayerButton playerObj = player.GetComponent<PlayerButton>();
            if (NetworkManager.instance.GetCustomPropertesOfRoom<int>("executionID") == playerObj.playerID) {
                playerObj.live = false;
                playerObj.playerInfoText.text = gameManager.timeController.day + "日目\n\r処刑";

            }
        }

        //生存数を更新
        gameManager.liveNum--;
        NetworkManager.instance.SetCustomPropertesOfRoom<int>("liveNum", gameManager.liveNum);
        //gameManager.SetLiveNum();
    }

    ////////////////////////////
    ///カスタムプロパティ関連
    /////////////////////////////

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
    }
    ///// <summary>
    ///// 処刑されたプレイヤーをもらう
    ///// </summary>
    ///// <returns></returns>
    //public int GetExecutionPlayerID() {
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("executionID", out object executionIDObj)) {
    //        executionID = (int)executionIDObj;
    //    }
    //    return executionID;
    //}
}



