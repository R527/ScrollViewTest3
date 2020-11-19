using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ゲーム中に内部通貨を購入する用のクラス
/// </summary>
public class BuyCurrency : MonoBehaviour
{

    public Button currencyBtn;
    public GameObject currencyImage;
    // Start is called before the first frame update
    void Start()
    {
        currencyBtn.onClick.AddListener(buyCurrency); 
    }

    void buyCurrency() {
        currencyImage.SetActive(true);
    }
}
