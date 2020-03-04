
/// <summary>
/// ChatNode用のデータ設定クラス
/// </summary>
[System.Serializable]
public class ChatData 
{
    public int chatID;
    //public ChatRoll roll = ChatRoll.EXT;
    public string inputData;
    //public string rollName;
    public int playerID;
    public int boardColor;
    public string playerName;
    public ROLLTYPE rollType;
    public CHAT_TYPE chatType;//コンストラクタでは代入しない


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
    public ChatData(int chatID, string inputData, int playerID, int boardColor, string playerName, ROLLTYPE rollType) {
        this.chatID = chatID;
        //this.roll = roll;
        this.inputData = inputData;
        //this.rollName = rollName;
        this.playerID = playerID;
        this.boardColor = boardColor;
        this.playerName = playerName;
        this.rollType = rollType;
    }
}
