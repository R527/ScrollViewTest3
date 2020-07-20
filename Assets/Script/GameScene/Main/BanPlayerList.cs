using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// BanPlayerListに関するクラス
/// </summary>
public class BanPlayerList : MonoBehaviour
{
    public Button closeBtn;
    

    // Start is called before the first frame update
    void Start()
    {
        closeBtn.onClick.AddListener(ClosePoPUp);
    }
    
    private void ClosePoPUp() {
        GetComponent<CanvasGroup>().alpha = 0;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
