using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 役職説明をするPopUp
/// </summary>
public class RollPopUp : MonoBehaviour
{
    public GameObject vocabulary;

    public Text rollText;
    public Text explanationText;
    public Text statusText;
    public Button backButton;
    public Button maskButton;

    // Start is called before the first frame update
    void Start()
    {
        backButton.onClick.AddListener(Destroy);
        maskButton.onClick.AddListener(Destroy);
    }

    private void Destroy() {
        Destroy(gameObject);
        vocabulary.SetActive(true);
        GraphicRaycastersManager rayCastManagerObj = GameObject.FindGameObjectWithTag("RaycastersManager").GetComponent<GraphicRaycastersManager>();
        rayCastManagerObj.SwitchGraphicRaycasters(true);
    }
}
