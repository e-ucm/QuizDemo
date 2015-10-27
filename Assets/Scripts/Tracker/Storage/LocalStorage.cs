using System;
using System.IO;
using UnityEngine;

public class LocalStorage : Storage
{
		
	private string tracesFile;
		
	public LocalStorage (string tracesFile)
	{
		this.tracesFile = tracesFile;
	}
				
	public void SetTracker (Tracker tracker)
	{
	}
		
	public void Start (Tracker.StartListener startListener)
	{
		try {
			File.AppendAllText (tracesFile, "--new session\n");
			startListener.Result ("");
		} catch (Exception e) {
			startListener.Error (e.Message);
		}
	}

	public void Send (String data, Tracker.FlushListener flushListener)
	{
		try {
            string tmpData = data.Replace("{\"actor", "${\"actor").Replace("[", "").Replace("]", "");
            string[] tmpArray = tmpData.Split('$');
            foreach(string action in tmpArray){
                if (action != "")
                {
                    Log.L().AddLogLine(action);
                }
            }
			File.AppendAllText (tracesFile, data);
			flushListener.Result ("");
		} catch (Exception e) {
			flushListener.Error (e.Message);
		}
	}

}


