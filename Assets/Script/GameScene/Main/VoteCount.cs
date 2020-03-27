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
    public bool isVoteFlag;
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
    private void SetExecutionPlayer() {
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
    private int GetExecutionPlayer() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("executionID", out object executionIDObj)) {
            executionID = (int)executionIDObj;
        }
        return executionID;
    }
    /// <summary>
    /// 一番投票されたプレイヤーを処刑する
    /// </summary>
    public void Execution() {
        for (int id = 0; id < gameManager.liveNum; id++) {
            int count = voteCountList[id];
            if(mostVotes == count) {
                ExecutionPlayerList.Add(chatSystem.playersList[id]);
            } else if(mostVotes < count) {
                mostVotes = count;
                ExecutionPlayerList.Clear();
                ExecutionPlayerList.Add(chatSystem.playersList[id]);
            }
        }

        //ランダム処刑処理or処刑処理
        if(ExecutionPlayerList.Count >= 2) {
            mostVotePlayer = ExecutionPlayerList[Random.Range(0, ExecutionPlayerList.Count)];
        } else {
            mostVotePlayer = ExecutionPlayerList[0];
        }
        //決定したプレイヤーを処刑処理する
        foreach(Player playerObj in chatSystem.playersList) {
            if (playerObj.playerID == mostVotePlayer.playerID) {
                playerObj.live = false;
                executionPlayer = playerObj;

                if (PhotonNetwork.IsMasterClient) {
                    executionID = executionPlayer.playerID;
                    SetExecutionPlayer();
                    Debug.Log("voteCount.executionID" + executionID);
                } else {
                    executionID = GetExecutionPlayer();
                    Debug.Log("voteCount.executionID" + executionID);
                }

                //処刑されたプレイヤーをリストから削除する
                foreach (Player player in chatSystem.playersList) {
                    if (playerObj.playerID == executionPlayer.playerID) {
                        chatSystem.playersList.Remove(player);
                        break;
                    }
                }
                break;
            }
        }
        gameManager.liveNum--;
        if (PhotonNetwork.IsMasterClient) {

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


}
