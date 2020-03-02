using UnityEngine;

public class NGList : MonoBehaviour
{

    public string[] ngWordList;

    // Start is called before the first frame update
    void Start()
    {
        string tempText = "";
        TextAsset textAsset = new TextAsset();
        textAsset = Resources.Load("NGword", typeof(TextAsset)) as TextAsset;
        tempText = textAsset.text;
        ngWordList = tempText.Split(',');
    }

}
