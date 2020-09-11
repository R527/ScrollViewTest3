using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// PlayerButtonを押した後に出てくるPoPUｐ
/// </summary>
public class ActionPopUp : MonoBehaviourPunCallbacks {


    public GameManager gameManager;

    public int playerID;
    public string playerName;
    public string myUniqueId;
    public bool fortune;

    public Text actionText;
    public Text buttonText;
    public Button cancelBtn;
    public Button actionBtn;//承諾

    [System.Serializable]
    public enum Action_Type {
        Ban,
        投票,
        強制退場,

        //夜の行動
        占い,
        襲撃,
        護衛
    }

    public Action_Type action_Type;


    // Start is called before the first frame update
    void Start()
    {
        cancelBtn.onClick.AddListener(CanselButton);
        actionBtn.onClick.AddListener(ActionButton);
    }

    /// <summary>
    /// PopUpを閉じるボタン
    /// </summary>
    public void CanselButton() {
        Destroy(gameObject);
    }


    /// <summary>
    /// アクションを決定するボタン
    /// </summary>
    public void ActionButton() {
        switch (action_Type) {

            case Action_Type.強制退場:
                if (PhotonNetwork.IsMasterClient) {
                    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                        if (player.ActorNumber == playerID && gameManager.chatSystem.myPlayer.playerID != playerID) {
                            PlayerManager.instance.roomBanUniqueIdList.Add((string)player.CustomProperties["myUniqueID"]);
                            PlayerManager.instance.roomBanUniqueIdStr += (string)player.CustomProperties["myUniqueID"] + ",";
                            gameManager.gameMasterChatManager.ForcedEvictionRoom(player);
                            break;
                        }
                    }
                }
                break;


            case Action_Type.Ban:
                PlayerManager.instance.banIndex = PlayerManager.instance.banListIndex;
                Debug.Log(PlayerManager.instance.banIndex);
                Debug.Log(PlayerManager.instance.banListIndex);
                PlayerManager.instance.SetStringForPlayerPrefs(myUniqueId, PlayerManager.ID_TYPE.banUniqueID);
                PlayerManager.instance.SetStringForPlayerPrefs(playerName, PlayerManager.ID_TYPE.banUserNickName);
                PlayerManager.instance.banListIndex++;

                PlayerManager.instance.SetIntForPlayerPrefs(PlayerManager.instance.banListIndex, PlayerManager.ID_TYPE.banListMaxIndex);
                break;

            case Action_Type.投票:

                if (!gameManager.chatSystem.myPlayer.isVoteFlag) {
                    gameManager.chatSystem.myPlayer.isVoteFlag = true;

                    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                        //Photonが用意しているActorNumberという数字とPLayerクラスが持っているPlayerIDが合致したら
                        //playerIDはActorNumberからもらっているから合致したら同じ番号を持っているクラスだといえる
                        if (player.ActorNumber == playerID) {

                            //最新の投票数を取得する
                            int voteNum = 0;
                            string voteName = string.Empty;
                            bool votingCompleted = false;
                            //指定されたplayerのキーに登録する
                            if (player.CustomProperties.TryGetValue("voteNum", out object voteCountObj)) {
                                voteNum = (int)voteCountObj;
                            }
                            voteNum++;

                            //投票したプレイヤーの名前を登録します
                            if (player.CustomProperties.TryGetValue("voteName", out object voteNameObj)) {
                                voteName = (string)voteNameObj;
                            }
                            voteName += gameManager.chatSystem.myPlayer.playerName + ",";

                            //一人のプレイヤーが投票を完了したらtrue
                            if (player.CustomProperties.TryGetValue("votingCompleted", out object votingCompletedObj)) {
                                votingCompleted = (bool)votingCompletedObj;
                            }
                            votingCompleted = true;

                            var propertiers = new ExitGames.Client.Photon.Hashtable {
                                    {"voteName", voteName },
                                    {"voteNum", voteNum },
                                    {"votingCompleted",votingCompleted }
                                };

                            player.SetCustomProperties(propertiers);

                            Debug.Log((int)player.CustomProperties["voteNum"]);
                            Debug.Log((string)player.CustomProperties["voteName"]);
                            Debug.Log((bool)player.CustomProperties["votingCompleted"]);

                            //投票数の表示をディクショナリーで管理
                            //voteCount.voteCountTable[playerID] = voteNum;


                            //Debug.Log(voteCount.voteNameTable[playerID]);
                            //投票のチャット表示
                            gameManager.gameMasterChatManager.Voted(player);

                            Debug.Log("投票完了");


                        }
                    }
                }
                break;


            //夜の行動

            case Action_Type.占い:

                    if (!fortune) {
                        gameManager.chatSystem.gameMasterChatManager.gameMasterChat = "【占い結果】\r\n" + playerName + "は人狼ではない（白）です。";
                        Debug.Log("白");
                    } else {
                        gameManager.chatSystem.gameMasterChatManager.gameMasterChat = "【占い結果】\r\n" + playerName + "は人狼（黒）です。";
                        Debug.Log("黒");
                    }
                gameManager.chatSystem.myPlayer.isRollAction = true;

                gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
                Debug.Log("RollAction");
                gameManager.chatSystem.gameMasterChatManager.gameMasterChat = string.Empty;

                break;
            case Action_Type.襲撃:
                gameManager.chatSystem.gameMasterChatManager.bitedID = playerID;
                //Debug.Log("噛んだID" + playerID);
                gameManager.chatSystem.gameMasterChatManager.SetBitedPlayerID();
                //Debug.Log("噛んだID" + gameManager.chatSystem.gameMasterChatManager.bitedID);
                gameManager.chatSystem.gameMasterChatManager.gameMasterChat = playerName + "さんを襲撃します。";
                //Debug.Log(playerName + "襲撃します。");
                gameManager.chatSystem.myPlayer.isRollAction = true;

                gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
                //Debug.Log("RollAction");
                gameManager.chatSystem.gameMasterChatManager.gameMasterChat = string.Empty;

                break;
            case Action_Type.護衛:
                gameManager.chatSystem.gameMasterChatManager.gameMasterChat = playerName + "さんを護衛します。";
                //Debug.Log("守ります");
                //守ったプレイヤーを記録
                gameManager.chatSystem.gameMasterChatManager.protectedID = playerID;
                gameManager.chatSystem.gameMasterChatManager.SetProtectedPlayerID();
                //Debug.Log("守ったID" + gameManager.chatSystem.gameMasterChatManager.protectedID);
                gameManager.chatSystem.myPlayer.isRollAction = true;

                gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
                //Debug.Log("RollAction");
                gameManager.chatSystem.gameMasterChatManager.gameMasterChat = string.Empty;
                break;
        }
        

        Destroy(gameObject);
    }
}
