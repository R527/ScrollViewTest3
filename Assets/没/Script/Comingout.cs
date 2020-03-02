using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// COボタンの制御
/// </summary>
public class Comingout : MonoBehaviour  {

    //class
    public ComingoutPrefab COPrefab;

    //main
    public GameObject content;

    public void OnClickCO(string rollName) {
        ComingoutPrefab coObj = Instantiate(COPrefab, content.transform, false);
        coObj.coImage.sprite = Resources.Load<Sprite>("CoImage/" + rollName);
        this.gameObject.SetActive(false);
    }

    public void ComingOutButton() {
        this.gameObject.SetActive(true);
    }
}
