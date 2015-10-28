/// <summary>
/// Gleaner Tracker Unity implementation.
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;

public class Tracker : MonoBehaviour
{
	public interface ITraceFormatter
	{
		string Serialize (List<string> traces);

		void StartData (JSONNode data);
	}
	
	private Storage storage;
	private ITraceFormatter traceFormatter;
	private bool sending;
	private bool connected;
	private bool connecting;
	private bool flushRequested;
	private List<string> queue = new List<string> ();
	private List<string> sent = new List<string> ();
	private float nextFlush;
	public float flushInterval = -1;
	public string storageType = "local";
	public string traceFormat = "csv";
	public string host;
	public string trackingCode;
	public Boolean debug = false;
	private StartListener startListener;
	private FlushListener flushListener;
	private static Tracker tracker;

	public static Tracker T(){
		return tracker;
	}

	public Tracker ()
	{
		this.flushListener = new FlushListener (this);
		this.startListener = new StartListener (this);
		tracker = this;
	}

	private void SetConnected (bool connected)
	{
		this.connected = connected;
		connecting = false;
	}

	public void Start ()
	{
		switch (traceFormat) {
		    case "json":
			    this.traceFormatter = new SimpleJsonFormat ();
			    break;
		    case "xapi":
                this.traceFormatter = new XApiFormat ();
			    break;
		    default:
                this.traceFormatter = new DefaultTraceFromat ();
			    break;
		}
        switch (storageType)
        {
            case "net":
                CreateNetStorage();
                break;
            default:
                String path = Application.persistentDataPath;
                if (!path.EndsWith("/"))
                {
                    path += "/";
                }
                path += "traces-" + traceFormat;
                if (debug)
                {
                    Debug.Log("Storing traces in " + path);
                }
                storage = new LocalStorage(path);
                storage.SetTracker(this);
                break;
        }
        this.startListener.SetTraceFormatter (this.traceFormatter);
		this.nextFlush = flushInterval;
		//this.Connect ();
		UnityEngine.Object.DontDestroyOnLoad (tracker);
	}

    private void CreateNetStorage()
    {
        Debug.Log("--Exist the file--> "+ Application.dataPath + "/Assets/track.txt ??");
        string filePath = Application.dataPath + "/Assets/track.txt";

        //WWW www = new WWW(filePath);
        //local, windows files
        WWW www = new WWW("file:///"+filePath);
        this.StartCoroutine(WaitForRequest(www));
    }

    private IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            string line = www.text;
            string[] readLine = line.Split(';');
            if (readLine.Length == 2)
            {
                host = readLine[0];
                trackingCode = readLine[1];
            }
            storage = new NetStorage(this, host, trackingCode);
            storage.SetTracker(this);
            this.Connect();
        }
        else
        {
            Debug.LogError("--ErrorWaitForRequest--> "+www.error);
        }
    }

    public void Update ()
	{
		float delta = Time.deltaTime;
		if (flushInterval >= 0) {
			nextFlush -= delta;
			if (nextFlush <= 0) {
				flushRequested = true;
			}
			while (nextFlush <= 0) {
				nextFlush += flushInterval;
			}
		}
			
		if (connected && flushRequested) {
			Flush ();
		}
	}

	/// <summary>
	/// Flush the traces queue in the next update.
	/// </summary>
	public void RequestFlush ()
	{
		flushRequested = true;
	}

	private void Connect ()
	{
		if (!connected && !connecting) {
			connecting = true;
			if (debug) {
				Debug.Log ("Connecting to collector... in host"+host);
			}
			storage.Start (this.startListener);
		}
	}

	private void Flush ()
	{
		if (!connected) {
			if (debug) {
				Debug.Log ("Not connected. Trying to connect");
			}
			Connect ();
		} else if (queue.Count > 0 && !sending) {
			if (debug) {
				Debug.Log ("Flushing...");
			}
			sending = true;
			sent.AddRange (queue);
			queue.Clear ();
			flushRequested = false;
			string data = traceFormatter.Serialize (sent);
			if (debug) {
				Debug.Log (data);
			}
			storage.Send (data, flushListener);
		}
	}

	private void Sent (bool error)
	{
		if (!error) {
			if (debug) {
				Debug.Log ("Traces received by storage.");
			}
			sent.Clear ();
		} else if (debug) {
			Debug.LogError ("Traces dispatch failed");
		}
		sending = false;
	}

	public class StartListener : Net.IRequestListener
	{

		private Tracker tracker;
		private ITraceFormatter traceFormatter;

		public StartListener (Tracker tracker)
		{
			this.tracker = tracker;
        }

		public void SetTraceFormatter (ITraceFormatter traceFormatter)
		{
            Debug.Log("--SetTraceFormatter--> "+traceFormatter.ToString());
			this.traceFormatter = traceFormatter;
        }

		public void Result (string data)
		{
			if (tracker.debug) {
				Debug.Log ("Start successfull");
			}
			try {
				JSONNode dict = JSONNode.Parse (data);
                this.ProcessData (dict);			            
			} catch (Exception e) {
                Debug.LogError("--ErrorTrackerResult--> "+e);
            }
            tracker.SetConnected (true);
		}
		
		public void Error (string error)
		{
			if (tracker.debug) {
				Debug.Log ("Error " + error);
			}
			tracker.SetConnected (false);
		}

		protected void ProcessData (JSONNode data)
		{
            Debug.Log("--ProcessData-->" + data.ToString());
            if (traceFormatter == null)
            {
                Debug.Log("--ProcessDataTraceFormatterNull--> " + traceFormatter);
                traceFormatter = new XApiFormat();
            }
            traceFormatter.StartData(data);
            
        }
	}

	public class FlushListener : Net.IRequestListener
	{

		private Tracker tracker;

		public FlushListener (Tracker tracker)
		{
			this.tracker = tracker;
		}

		public void Result (string data)
		{
			tracker.Sent (false);
		}

		public void Error (string error)
		{
			tracker.Sent (true);
		}
	}

	/* Traces */

        /// <summary>
        /// Adds a trace to the queue.
        /// </summary>
        /// <param name="trace">A comma separated string with the values of the trace</param>
    public void Trace (string trace)
	{
		if (debug) {
			Debug.Log ("'" + trace + "' added to the queue.");
		}
		queue.Add (trace);
	}

	/// <summary>
	/// Adds a trace with the specified values
	/// </summary>
	/// <param name="values">Values of the trace.</param>
	public void Trace (params string[] values)
	{
		string result = "";
		foreach (string value in values) {
			result += value + ",";
		}
		Trace (result);
	}

	/// <summary>
	/// Player entered in a screen.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	public void Screen (string screenId)
	{
		Trace ("screen", screenId);
	}

	/// <summary>
	/// Player entered in a zone inside the game world.
	/// </summary>
	/// <param name="zoneid">Zone identifier.</param>
	public void Zone (string zoneId)
	{
		Trace ("zone", zoneId);
	}

	/// <summary>
	/// Player selected an option in a presented choice
	/// </summary>
	/// <param name="choiceId">Choice identifier.</param>
	/// <param name="optionId">Option identifier.</param>
	public void Choice (string choiceId, string optionId)
	{
		Trace ("choice", choiceId, optionId);
	}

	/// <summary>
	/// A meaningful variable was updated in the game.
	/// </summary>
	/// <param name="varName">Variable name.</param>
	/// <param name="value">New value for the variable.</param>
	public void Var (string varName, System.Object value)
	{
		Trace ("var", varName, value.ToString ());
	}
}

