using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainCamera : MonoBehaviour {
    Camera cam;
    public float width = 203f;
    public float height = 406f;
    public float pexelPerUnit = 100f;
    public Text debugText;

    

    void Awake() {
        float screenW = (float)Screen.width;
        float screenH = (float)Screen.height;
        cam = Camera.main;

        Debug.Log("screenW" + screenW + " screenH" + screenH);
        if (screenW / screenH < width / height) {
            //cam.orthographicSize = 10.0f;
            cam.orthographicSize = screenH / (screenW / (width / pexelPerUnit)) / 2;
        } else {
            //cam.orthographicSize = 10.0f;
            cam.orthographicSize = height / 2 / pexelPerUnit;
        }
        Debug.Log("cam.orthographicSize" + cam.orthographicSize);

        float camScalerX = gameObject.GetComponent<CanvasScaler>().referenceResolution.x;
        float camScalerY = gameObject.GetComponent<CanvasScaler>().referenceResolution.y;
        debugText.text = "screenW" + screenW + " screenH" + screenH + " cam.orthographicSize" + cam.orthographicSize + "camScalerX" + camScalerX + "camScalerY" + camScalerY;

        
    }

}

