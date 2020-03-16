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
    private int listCount;
    public bool isVoteFlag;
    public List<Player> ExecutionPlayerList = new List<Player>();
    public int mostVotes;
    public Player mostVotePlayer;//処刑ナンバー
    public Player executionPlayer;

    /// <summary>
    /// ListにInt型を人数に合わせて追加する　
    /// </summary>
    /// <param name="numLimit"></param>
    public void VoteCountListSetUp(int numLimit) {
        listCount = numLimit;
        for (int i = 0; i < listCount; i++) {
            voteCountList.Add(0);
        }
    }
    /// <summary>
    /// Playerが押した相手に投票処理をする
    /// </summary>
    /// <param name="id"></param>
    public void VoteCountList(int id, bool live) {
        if(isVoteFlag == false && live == true) {
            if (chatSystem.myPlayer.playerID == id) {
                Debug.Log("押せません");
                return;
            }
            voteCountList[id]++;
            isVoteFlag = true;
        }
        Debug.Log("押せません");
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

                //処刑されたプレイヤーをリストから削除する
                foreach(Player player in chatSystem.playersList) {
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
