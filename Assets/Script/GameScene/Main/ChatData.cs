
/// <summary>
/// ChatNode用のデータ設定クラス
/// </summary>
[System.Serializable]
public class ChatData 
{
    //public int chatID;
    public string inputData;
    public int playerID;
    public int boardColor;
    public int iconNo;
    public string playerName;
    public ROLLTYPE rollType;

    public CHAT_TYPE chatType;//コンストラクタでは代入しない
    public bool chatLive;
    public bool chatWolf;
    public string comingOutText;

    /// <summary>
    /// Class名と同名で戻り値（voidなど）を持たないメソッドはコストラクタになる
    /// インスタンスと同時に自動で処理されるメソッド
    /// </summary>
    /// <param name="chatID"></param>
    /// <param name="inputData"></param>
    /// <param name="playerID"></param>
    /// <param name="boardColor"></param>
    /// <param name="playerName"></param>
    /// <param name="rollType"></param>
    public ChatData(string inputData, int playerID, int boardColor, string playerName, ROLLTYPE rollType, int iconNo) {
        //this.chatID = chatID;
        this.inputData = inputData;
        this.playerID = playerID;
        this.boardColor = boardColor;
        this.playerName = playerName;
        this.rollType = rollType;
        this.iconNo = iconNo;
    }
}
