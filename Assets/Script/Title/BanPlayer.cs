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
        
        PlayerManager.instance.banUserNickNameList.Remove(userNickNameText.text);
        PlayerManager.instance.banUniqueIDList.Remove(banUniqueID);

        Destroy(gameObject);
    }
}
