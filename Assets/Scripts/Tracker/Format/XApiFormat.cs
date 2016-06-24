using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class XApiFormat : Tracker.ITraceFormatter
{
	string SCREEN = "screen";
	string ZONE = "zone";
	string CHOICE = "choice";
	string VAR = "var";
    private Dictionary<string, string> verbIds = new Dictionary<string, string>()
    {
        { "started", "http://activitystrea.ms/schema/1.0/started"},
        { "progressed", "http://adlnet.gov/expapi/verbs/progressed"},
        { "completed", "http://adlnet.gov/expapi/verbs/completed"},
        { "accessed", "http://activitystrea.ms/schema/1.0/accessed"},
        { "skipped", "http://id.tincanapi.com/verb/skipped"},
        { "set", "https://rage.e-ucm.es/xapi/seriousgames/verbs/set"},
        { "decreased", "https://rage.e-ucm.es/xapi/seriousgames/verbs/decreased"},
        { "increased", "https://rage.e-ucm.es/xapi/seriousgames/verbs/increased"},
        { "preferred", "http://adlnet.gov/expapi/verbs/preferred"}
    };
    private Dictionary<string, string> objectIds = new Dictionary<string, string>()
    {
        { "started", "http://activitystrea.ms/schema/1.0/started"},
        { "progressed", "http://adlnet.gov/expapi/verbs/progressed"},
        { "completed", "http://adlnet.gov/expapi/verbs/completed"},
        { "accessed", "http://activitystrea.ms/schema/1.0/accessed"},
        { "skipped", "http://id.tincanapi.com/verb/skipped"},
        { "set", "https://rage.e-ucm.es/xapi/seriousgames/verbs/set"},
        { "decreased", "https://rage.e-ucm.es/xapi/seriousgames/verbs/decreased"},
        { "increased", "https://rage.e-ucm.es/xapi/seriousgames/verbs/increased"},
        { "preferred", "http://adlnet.gov/expapi/verbs/preferred"}
    };
    private List<JSONNode> statements = new List<JSONNode> ();
	private string objectId;
	private JSONNode actor;

    public enum Extension
    {
        HEALTH, POSITION, PROGRESS


        public override string ToString()
    {
        switch (this)
        {
            case HEALTH:
                return "https://w3id.org/xapi/seriousgames/extensions/health";
            case POSITION:
                return "https://w3id.org/xapi/seriousgames/extensions/position";
            case PROGRESS:
                return "https://w3id.org/xapi/seriousgames/extensions/progress";
        }
        return "";
    }
}

public class Result
    {
        float score = float.NaN;
        // -1 = NULL, 0 = false, 1 true
        int success = -1;
        int completion;

        string response;
        Dictionary<string, Object> extensions;

        public override string ToString()
        {
            string s = "\"result\":{";
            if (float.IsNaN(score))
            {
                s += "\"score\":{\"raw\":" + score + "},";
            }

            if (success != -1)
            {
                s += "\"success\":" + success + ",";
            }

            if (completion != -1)
            {
                s += "\"completion\":" + completion + ",";
            }

            if (response != null)
            {
                s += "\"response\": \"" + response + "\",";
            }

            if (extensions != null)
            {
                s += "\"extensions\":{";
                foreach (KeyValuePair<string, Object> e in extensions)
                {
                    s += "\"" + e.Key + "\":";
                    if (e.Value is string)
                    {
                        s += "\"" + e.Value + "\"";
                    } else {
                        s += e.Value.ToString();
                    }
                s += ",";
            }
            s = s.Substring(0, s.Length - 1) + "},";
        }
			return s.Substring(0, s.Length - 1) + "},";
		}
    }

public void StartData (JSONNode data)
	{
		actor = data ["actor"];
		objectId = data ["objectId"].ToString ();
		if (!objectId.EndsWith ("/")) {
			objectId += "/";
		}
	}

	public string Serialize (List<string> traces)
	{
		statements.Clear ();

		foreach (string trace in traces) {
			statements.Add (CreateStatement (trace));
		}

		string result = "[";
		foreach (JSONNode statement in statements) {
			result += statement.ToString () + ",";
		}
		return result.Substring (0, result.Length - 1).Replace ("[\n\t]", "").Replace (": ", ":") + "]";
	}

	private JSONNode CreateStatement (string trace)
	{
		string[] parts = trace.Split (',');
		string timestamp = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds (long.Parse (parts [0])).ToString ("yyyy-MM-ddTHH:mm:ss.fffZ");

		JSONNode statement = JSONNode.Parse ("{\"timestamp\": \"" + timestamp + "\"}");

		if (actor != null) {
			statement.Add ("actor", actor);		      
		}
		statement.Add ("verb", CreateVerb (parts [2]));


		statement.Add ("object", CreateActivity (parts));

		if (parts.Length > 3) {
			JSONNode extensions = JSONNode.Parse ("{}");
			JSONNode extensionsChild = JSONNode.Parse ("{}");
			extensionsChild.Add (EXT_PREFIX + "value", parts [3]);

			extensions.Add ("extensions", extensionsChild);

			statement.Add ("result", extensions);
		}

		return statement;
	}

	private JSONNode CreateVerb (string ev)
	{

        string id = ev;
        bool found = verbIds.TryGetValue(ev, out id);

		JSONNode verb = JSONNode.Parse ("{ id : }");
		verb ["id"] = id;

		return verb;
	}

	private JSONNode CreateActivity (string[] parts)
	{
		string ev = parts [2];
		string id;
		if (CHOICE.Equals (ev)) {
			id = "choice";
		} else if (SCREEN.Equals (ev)) {
			id = "screen";
		} else if (ZONE.Equals (ev)) {
			id = "zone";
		} else if (VAR.Equals (ev)) {
			id = "variable";
		} else {
			id = ev;
		}
		return JSONNode.Parse ("{ id : " + objectId + id + "/" + parts [2] + "}");
	}
}


