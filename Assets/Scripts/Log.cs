using UnityEngine;
using System.Collections;

public class Log : MonoBehaviour
{

    private GameObject uiText;
    private UnityEngine.UI.Text tracesLog;
    private RectTransform rectT;
    private Vector2 tmpSize;

    private UnityEngine.UI.ScrollRect scroll;

    private static Log log;

    public static Log L()
    {
        return log;
    }

    public Log()
    {
        log = this;
    }

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
        iReallyNeedToPutItOnZero = false;
    }

    public void AddLogLine(string line)
    {
        
        var jumps = line.Split('\n').Length;

        tracesLog.text += '\n' + line;
        tmpSize.x = rectT.sizeDelta.x;
        tmpSize.y = rectT.sizeDelta.y + 16*jumps;
        
        rectT.sizeDelta = tmpSize;

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
