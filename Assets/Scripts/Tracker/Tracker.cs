/// <summary>
/// Gleaner Tracker Unity implementation.
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using System.IO;

public class Tracker : MonoBehaviour
{
	public static DateTime START_DATE = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public interface ITraceFormatter
	{
		string Serialize (List<string> traces);

		void StartData (JSONNode data);
	}

	private Storage mainStorage;
	private LocalStorage backupStorage;
	private ITraceFormatter traceFormatter;
	private bool sending;
	private bool connected;
	private bool connecting;
	private bool flushRequested;
	private bool useMainStorage;
	private List<string> queue = new List<string> ();
	private List<string> sent = new List<string> ();
	private List<string> allTraces = new List<string>();
	private float nextFlush;
	public float flushInterval = -1;
	[Range(3, int.MaxValue)]
	public float checkInterval = 3;
	private float nextCheck;
	public string storageType = "local";
	public string traceFormat = "csv";
	public string host;
	public string trackingCode;
	public Boolean debug = false;
	private StartListener startListener;
	private FlushListener flushListener;
	private static Tracker tracker;
	private String filePath;
	private StartLocalStorageListener startLocalStorageListener;

	public static Tracker T ()
	{
		return tracker;
	}

	public Tracker ()
	{
		flushListener = new FlushListener (this);
		startListener = new StartListener (this);
		startLocalStorageListener = new StartLocalStorageListener(this);
		tracker = this;
	}

	public ITraceFormatter GetTraceFormatter ()
	{
		return this.traceFormatter;
	}

	private void SetMainStorageConnected (bool connected)
	{
		useMainStorage = connected;
		SetConnected (connected || this.connected);
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
		filePath = GeneratePath ();
		switch (storageType) {
		case "net":
			filePath += "Pending";
			mainStorage = new NetStorage (this, host, trackingCode);
			mainStorage.SetTracker (this);
			backupStorage = new LocalStorage (filePath);
			backupStorage.SetTracker (this);
			break;
		default:
			mainStorage = new LocalStorage (filePath);
			mainStorage.SetTracker (this);
			break;
		}
		
		this.startListener.SetTraceFormatter (this.traceFormatter);
		this.Connect ();
		this.nextFlush = flushInterval;

		UnityEngine.Object.DontDestroyOnLoad (this);
	}

	public string GeneratePath ()
	{
		String path = Application.persistentDataPath;
#if UNITY_ANDROID
		AndroidJavaObject env = new AndroidJavaObject ("android.os.Environment");
		AndroidJavaObject file = env.CallStatic<AndroidJavaObject> ("getExternalStorageDirectory");
		path = file.Call<String> ("getAbsolutePath");
#endif
		if (!path.EndsWith ("/")) {
			path += "/";
		}
		path += "traces";
		if (debug) {
			Debug.Log ("Storing traces in " + path);
		}

		return path;
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

		if (checkInterval >= 0) {
			nextCheck -= delta;
			if (!useMainStorage && !connecting && nextCheck <= 0 && mainStorage.IsAvailable ()) {
				connecting = true;
				if (debug) {
					Debug.Log ("Starting main storage");
				}
				mainStorage.Start (startListener);
			}
			while (nextCheck <= 0) {
				nextCheck += checkInterval;
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
				Debug.Log ("Starting local storage ");
			}
			
			if (mainStorage.IsAvailable ()) {
				mainStorage.Start (startListener);
			} 
			if (backupStorage != null) {
				backupStorage.Start (startLocalStorageListener);
			}
		}
	}
	
	private void Flush ()
	{
		if (!connected && !connecting) {
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
			string data = "";
			if (useMainStorage == false && backupStorage != null) {
				if (debug) {
					Debug.Log ("Sending traces via aux storage");
				}
				backupStorage.Send (GetRawTraces (), flushListener);
			} else {
				if (debug) {
					Debug.Log ("Sending traces via main storage");
				}
				allTraces.Clear();
				allTraces.AddRange (sent);
				if(backupStorage!=null)
					allTraces.AddRange (backupStorage.RecoverData ());
				data = traceFormatter.Serialize (allTraces);
				mainStorage.Send(data, flushListener);
			}
			if (debug) {
				Debug.Log(data);
			}
		}
	}

	private string GetRawTraces ()
	{
		string data = "";
		foreach (String trace in sent)
		{
			data += trace + ";";
		}
		return data;
	}

	private void Sent (bool error)
	{
		if (!error) {
			if (debug) {
				Debug.Log ("Traces received by storage.");
			}
			sent.Clear ();
			if (useMainStorage) {
				backupStorage.CleanFile();
			}
		} else {
			if (debug) {
				Debug.LogError ("Traces dispatch failed");
			}
			if (useMainStorage && backupStorage != null) {
				useMainStorage = false;
				backupStorage.Send (GetRawTraces(), flushListener);
			}
		}
		sending = false;
	}

	public class StartLocalStorageListener : Net.IRequestListener
	{
		protected Tracker tracker;
		private ITraceFormatter traceFormatter;

		public StartLocalStorageListener (Tracker tracker)
		{
			this.tracker = tracker;
		}

		public void Result(string data)
		{
			if (tracker.debug)
			{
				Debug.Log ("Start local storage successfull");
			}
			tracker.SetConnected (true);
		}

		public void Error(string error)
		{
			if (tracker.debug)
			{
				Debug.Log("Error " + error);
			}
			tracker.SetConnected (false);
		}
	}

	public class StartListener : Net.IRequestListener
	{
		protected Tracker tracker;
		private ITraceFormatter traceFormatter;

		public StartListener (Tracker tracker)
		{
			this.tracker = tracker;
		}

		public void SetTraceFormatter (ITraceFormatter traceFormatter)
		{
			this.traceFormatter = traceFormatter;
		}

		public void Result (string data)
		{
			if (tracker.debug) {
				Debug.Log ("Start main storage successfull");
			}
			if (!String.IsNullOrEmpty(data)) {
				try {
					JSONNode dict = JSONNode.Parse (data);
					this.ProcessData (dict);
				} catch (Exception e) {
					Debug.LogError (e);
				}
			}
			tracker.SetMainStorageConnected (true);
		}

		public void Error (string error)
		{
			if (tracker.debug) {
				Debug.Log ("Error " + error);
			}
			tracker.SetMainStorageConnected (false);
		}

		protected virtual void ProcessData (JSONNode data)
		{
			traceFormatter.StartData (data);
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

		trace = Math.Round (System.DateTime.Now.ToUniversalTime ().Subtract (START_DATE).TotalMilliseconds) + "," + trace;
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

    /* VARIABLES */

    public enum VariableTypes
    {
        Score,
        Currency,
        Health,
        Attempt,
        Preference,
        Position,
        Variable
    }

    /// <summary>
    /// A meaningful variable was updated in the game.
    /// </summary>
    /// <param name="varName">Variable name.</param>
    /// <param name="value">New value for the variable.</param>
    public void Set(string varName, System.Object value)
    {
        Trace("set", VariableTypes.Variable.ToString().ToLower(), varName, value.ToString());
    }

    /// <summary>
    /// A meaningful variable was updated in the game.
    /// </summary>
    /// <param name="varName">Variable name.</param>
    /// <param name="value">New value for the variable.</param>
    /// <param name="type">The variable type.</param>
    public void Set(string varName, System.Object value, VariableTypes type)
    {
        Trace("set", type.ToString().ToLower(), varName, value.ToString());
    }

    /// <summary>
    /// A meaningful variable was increased in the game.
    /// </summary>
    /// <param name="varName">Variable name.</param>
    /// <param name="value">The amount that was increased.</param>
    public void Increased(string varName, float value)
    {
        Trace("increased", VariableTypes.Variable.ToString().ToLower(), varName, value.ToString());
    }

    /// <summary>
    /// A meaningful variable was increased in the game.
    /// </summary>
    /// <param name="varName">Variable name.</param>
    /// <param name="value">The amount that was increased.</param>
    /// <param name="type">The variable type.</param>
    public void Increased(string varName, float value, VariableTypes type)
    {
        Trace("increased", type.ToString().ToLower(), varName, value.ToString(),);
    }

    /// <summary>
    /// A meaningful variable was decreased in the game.
    /// </summary>
    /// <param name="varName">Variable name.</param>
    /// <param name="value">The amount that was decreased.</param>
    public void Decreased(string varName, float value)
    {
        Trace("decreased", VariableTypes.Variable.ToString().ToLower(), varName, value.ToString());
    }

    /// <summary>
    /// A meaningful variable was decreased in the game.
    /// </summary>
    /// <param name="varName">Variable name.</param>
    /// <param name="value">The amount that was decreased.</param>
    /// <param name="type">The variable type.</param>
    public void Decreased(string varName, float value, VariableTypes type)
    {
        Trace("decreased", type.ToString().ToLower(), varName, value.ToString());
    }


    /* ALTERNATIVES */

    public enum AlternativeTypes
    {
        Question,
        Menu,
        Dialog,
        Path,
        Arena,
        Alternative
    }

    /// <summary>
    /// Player selected an option in a presented alternative
    /// </summary>
    /// <param name="alternativeId">Alternative identifier.</param>
    /// <param name="optionId">Option identifier.</param>
    public void Selected(string alternativeId, string optionId)
    {
        Trace("selected", AlternativeTypes.Alternative.ToString().ToLower(), alternativeId,  optionId);
    }

    /// <summary>
    /// Player selected an option in a presented alternative
    /// </summary>
    /// <param name="alternativeId">Alternative identifier.</param>
    /// <param name="optionId">Option identifier.</param>
    /// <param name="type">Alternative type.</param>
    public void Selected(string alternativeId, string optionId, AlternativeTypes type)
    {
        Trace("selected", type.ToString().ToLower(), alternativeId, optionId);
    }

    /* COMPLETABLES */

    public enum CompletableTypes
    {
        Game,
        Session,
        Level,
        Quest,
        Stage,
        Combat,
        StoryNode,
        Race,
        Completable
    }

    /// <summary>
    /// Player started a completable.
    /// </summary>
    /// <param name="completableId">Completable identifier.</param>
    public void Started(string completableId)
    {
        Trace("started", CompletableTypes.Completable.ToString().ToLower(), completableId);
    }

    /// <summary>
    /// Player started a completable.
    /// </summary>
    /// <param name="completableId">Completable identifier.</param>
    /// <param name="type">Completable type.</param>
    public void Started(string completableId, CompletableTypes type)
    {
        Trace("started", type.ToString().ToLower(), completableId);
    }

    /// <summary>
    /// Player progressed a completable.
    /// </summary>
    /// <param name="completableId">Completable identifier.</param>
    /// <param name="value">New value for the completable's progress.</param>
    public void Progressed(string completableId, float value)
    {
        Trace("progressed", CompletableTypes.Completable.ToString().ToLower(), completableId, value.ToString());
    }

    /// <summary>
    /// Player progressed a completable.
    /// </summary>
    /// <param name="completableId">Completable identifier.</param>
    /// <param name="value">New value for the completable's progress.</param>
    /// <param name="type">Completable type.</param>
    public void Progressed(string completableId, float value, CompletableTypes type)
    {
        Trace("progressed", type.ToString().ToLower(), completableId, value.ToString());
    }

    /// <summary>
    /// Player completed a completable.
    /// </summary>
    /// <param name="completableId">Completable identifier.</param>
    public void Completed(string completableId)
    {
        Trace("completed", CompletableTypes.Completable.ToString().ToLower(), completableId);
    }

    /// <summary>
    /// Player completed a completable.
    /// </summary>
    /// <param name="completableId">Completable identifier.</param>
    /// <param name="type">Completable type.</param>
    public void Completed(string completableId, CompletableTypes type)
    {
        Trace("completed", type.ToString().ToLower(), completableId);
    }


    /* REACHABLES */

    public enum ReachableTypes
    {
        Screen,
        Area,
        Zone,
        Cutscene,
        Reachable
    }

    /// <summary>
    /// Player accessed a reachable.
    /// </summary>
    /// <param name="reachableId">Reachable identifier.</param>
    public void Accessed(string reachableId)
    {
        Trace("accessed", ReachableTypes.Reachable.ToString().ToLower(), reachableId);
    }

    /// <summary>
    /// Player accessed a reachable.
    /// </summary>
    /// <param name="reachableId">Reachable identifier.</param>
    /// <param name="type">Reachable type.</param>
    public void Accessed(string reachableId, ReachableTypes type)
    {
        Trace("accessed", type.ToString().ToLower(), reachableId);
    }

    /// <summary>
    /// Player skipped a reachable.
    /// </summary>
    /// <param name="reachableId">Reachable identifier.</param>
    public void Skipped(string reachableId)
    {
        Trace("skipped", ReachableTypes.Reachable.ToString().ToLower(), reachableId);
    }

    /// <summary>
    /// Player skipped a reachable.
    /// </summary>
    /// <param name="reachableId">Reachable identifier.</param>
    /// <param name="type">Reachable type.</param>
    public void Skipped(string reachableId, ReachableTypes type)
    {
        Trace("skipped", type.ToString().ToLower(), reachableId);
    }
}


