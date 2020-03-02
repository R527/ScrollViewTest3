using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/// <summary>
/// テストクラス　消去するかも
/// </summary>
public static class GameRoomProperties 
{

    private const string KeyStartTime = "StartTime";

    private static Hashtable hashtable = new Hashtable();

    public static bool HasStartTime(this Room room) {
        return room.CustomProperties.ContainsKey(KeyStartTime);
    }


    //使っていない
    public static bool TryGetStartTime(this Room room, out float timestamp) {
        bool result = room.CustomProperties.TryGetValue(KeyStartTime, out var value);
        timestamp = (result) ? (float)value : 0;
        return result;
    }

    public static void SetStartTime(this Room room, float timestamp) {
        hashtable[KeyStartTime] = timestamp;
        room.SetCustomProperties(hashtable);
        hashtable.Clear();
    }
    
}
