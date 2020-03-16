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
    public List<string> ExecutionPlayerList = new List<string>();
    public int mostVotes;
    public string mostVotePlayer;//処刑ナンバー
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
                ExecutionPlayerList.Add(chatSystem.playerNameList[id]);
            } else if(mostVotes < count) {
                mostVotes = count;
                ExecutionPlayerList.Clear();
                ExecutionPlayerList.Add(chatSystem.playerNameList[id]);
            }
        }

        //ランダム処刑処理or処刑処理
        if(ExecutionPlayerList.Count >= 2) {
            mostVotePlayer = ExecutionPlayerList[Random.Range(0, ExecutionPlayerList.Count)];
        } else {
            mostVotePlayer = ExecutionPlayerList[0];
        }
        foreach(Player playerObj in chatSystem.playersList) {
            if (playerObj.playerName == mostVotePlayer) {
                playerObj.live = false;
                executionPlayer = playerObj;
            }
        }
        gameManager.liveNum--;
        if (PhotonNetwork.IsMasterClient) {
            gameManager.SetLiveNum();
        }
    }


}
