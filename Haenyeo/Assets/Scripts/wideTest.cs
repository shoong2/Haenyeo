using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wideTest : MonoBehaviour
{
    int p_width = Screen.width;
    int p_height = Screen.height;

    float height;
    float width;

    public RectTransform seaTop;

    RectTransform rect;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Screen.width / 160);
        Debug.Log(Mathf.Floor(Screen.width / 160));
        seaTop.localScale = new Vector2(seaTop.localScale.x + ((Screen.width / 160f - Mathf.Floor(Screen.width / 160))/10), seaTop.localScale.y);
        height = 2 * Camera.main.orthographicSize;
        width = height * Camera.main.aspect;
        rect = GetComponent<RectTransform>();
        //  rect.sizeDelta = new Vector2(rect.sizeDelta.x * height /10, rect.sizeDelta.y);
        Debug.Log(Screen.width);
        Debug.Log(Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
