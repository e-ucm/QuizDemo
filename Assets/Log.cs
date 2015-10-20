using UnityEngine;
using System.Collections;

public class Log : MonoBehaviour
{

    private GameObject uiText;
    private UnityEngine.UI.Text tracesLog;
    private RectTransform rectT;
    private Vector2 tmpSize;
    private Vector2 tmpPos;

    private UnityEngine.UI.ScrollRect scroll;

    // Use this for initialization
    void Start()
    {
        UnityEngine.Object.DontDestroyOnLoad(this);
        uiText = GameObject.FindGameObjectWithTag("TracesLog");
        tracesLog = uiText.GetComponent<UnityEngine.UI.Text> ();

        GameObject scrollImg = GameObject.Find("ScrollImage");
        scroll = scrollImg.GetComponent<UnityEngine.UI.ScrollRect>();

        rectT =  uiText.GetComponent<RectTransform>();
        Debug.Log(tracesLog.text);
        tmpSize = new Vector2(0, 0);
        tmpPos = new Vector2(0, 0);
        iReallyNeedToPutItOnZero = false;

    }

    public void AddLogLine(string line)
    {
        tracesLog.text += '\n' + line;
        tmpSize.x = rectT.sizeDelta.x;
        tmpSize.y = rectT.sizeDelta.y + 16;
        rectT.sizeDelta = tmpSize;

        tmpPos.x = rectT.position.x;
        tmpPos.y = rectT.position.y-16;
        rectT.position = tmpPos;

        scroll.verticalScrollbar.value = 0.0f;
        iReallyNeedToPutItOnZero = true;
    }

    private bool iReallyNeedToPutItOnZero = false;

    void Update()
    {
        if (scroll.verticalScrollbar.value > 0.0f && iReallyNeedToPutItOnZero)
        {
            scroll.verticalScrollbar.value = 0.0f;
            iReallyNeedToPutItOnZero = false;
        }
    }
  
}
