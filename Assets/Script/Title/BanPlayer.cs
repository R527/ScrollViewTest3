using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BanPlayer : MonoBehaviour
{

    public string banUniqueID;
    public Text userNickNameText;
    public Button DestroyBtn;


    private void Start() {
        DestroyBtn.onClick.AddListener(DeleteBanListButton);
    }
    public void SetUp(string UniqueID, string userNickName) {
        banUniqueID = UniqueID;
        userNickNameText.text = userNickName;
    }


    public void DeleteBanListButton() {
        
        //Listから名前を削除
        PlayerManager.instance.banUserNickNameList.Remove(userNickNameText.text);
        PlayerManager.instance.banUniqueIDList.Remove(banUniqueID);

        

        //Keyの削除
        for (int i = 0; i < 3; i++) {
            PlayerPrefs.DeleteKey(PlayerManager.ID_TYPE.banUniqueID.ToString() + i.ToString());
            PlayerPrefs.DeleteKey(PlayerManager.ID_TYPE.banUserNickName.ToString() + i.ToString());
        }
        //PlayerPrefsの情報書き換え
        for (int i = 0; i < PlayerManager.instance.banUniqueIDList.Count; i++) {
            
            PlayerManager.instance.banIndex = i;
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.banUniqueIDList[i], PlayerManager.ID_TYPE.banUniqueID);
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.banUserNickNameList[i], PlayerManager.ID_TYPE.banUserNickName);

        }


        PlayerManager.instance.banListMaxIndex = PlayerManager.instance.banUniqueIDList.Count;
        PlayerManager.instance.SetIntForPlayerPrefs(PlayerManager.instance.banUniqueIDList.Count, PlayerManager.ID_TYPE.banListMaxIndex);
        //Object削除
        Destroy(gameObject);
    }
}
