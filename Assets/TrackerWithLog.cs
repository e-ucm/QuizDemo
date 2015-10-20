using UnityEngine;
using System.Collections;

public class TrackerWithLog : Tracker {

    private Log log;
    public override void Trace(string trace)
    {
        if(log == null)
        {
            log = GameObject.FindObjectOfType<Log>();
        }
        Debug.Log("->>>>" + trace);
        base.Trace(trace);
        log.AddLogLine(trace);
        Debug.Log("-----" + trace);
    }

}
