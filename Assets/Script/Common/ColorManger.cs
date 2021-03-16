using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManger : MonoBehaviour
{
    public static ColorManger instance;
    public List<Color> iconColorList;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
