using System;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour {

    public static Score S;

    private int score;
    private UnityEngine.UI.Text uiScore;

	private int maxScore;
	public UnityEngine.UI.Text uiMaxScore;
	public UnityEngine.UI.Text uiListScore;

	public bool showScore = true;
	public bool registerUserScore = false;

	public string gameStorageURL;
	public string prefix;

	private string maxScoreSuffix = "maxScore";
	private SimpleJSON.JSONNode scoreList;

	private Net net;

	private GameStorageNewScorePOSTListener postNewListener;
	private GameStorageScorePOSTListener postListener;
	private GameStorageScoreGETListener getListener;

	private GameStorageNewListPOSTListener postNewListListener;
	private GameStorageListPOSTListener postListListener;
	private GameStorageListGETListener getListListener;

	private Dictionary<string, string> headers;

	void Start()
    {
		if (S == null)
		{
			S = this;

			this.net = new Net(this);
			headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");
			this.postNewListener = new GameStorageNewScorePOSTListener(this);
			this.postListener = new GameStorageScorePOSTListener(this);
			this.getListener = new GameStorageScoreGETListener(this);

			this.postNewListListener = new GameStorageNewListPOSTListener(this);
			this.postListListener = new GameStorageListPOSTListener(this);
			this.getListListener = new GameStorageListGETListener(this);

			this.uiScore = GetComponentInParent<UnityEngine.UI.Text>();
			score = PlayerPrefs.GetInt("LevelScore");
			this.DisplayScore();
		}
		readScoreList();


		if (!showScore)
		{
			uiScore.enabled = false;
		}

		readMaxScore();
    }

    public void AddScore(int s)
    {
        score += s;
        DisplayScore();
        ModifyScore();
		if (score > maxScore)
		{
			updateMaxScore();
		}
    }

    public void SetScore(int s)
    {
        score = s;
        DisplayScore();
        ModifyScore();
    }

    private void DisplayScore()
    {
        this.uiScore.text = "Score : " + score;
    }

    private void ModifyScore()
    {
        if (score != PlayerPrefs.GetInt("LevelScore"))
        {
            PlayerPrefs.SetInt("LevelScore", score);
            PlayerPrefs.Save();
            Tracker.T.setScore(score);
		}
    }

	public void SetMaxScore(int s, string user)
	{
		this.maxScore = s;
		this.uiMaxScore.text = s.ToString();
		if (user != "")
		{
			this.uiMaxScore.text += " by " + user;
		}
	}

	public void updateMaxScore()
	{
		if(score > maxScore)
		{
			maxScore = score;
			this.uiMaxScore.text = maxScore.ToString() + " by " + PlayerPrefs.GetString("User");
			writeMaxScore();
		}
	}

	public void createMaxScore()
	{
		string user = PlayerPrefs.GetString("User");
		string data = "{\"maxScore\":" + maxScore + ",\"user\":\"" + user+ "\"}";
		net.POST(gameStorageURL + "storage/" + prefix + " / " + maxScoreSuffix, System.Text.Encoding.UTF8.GetBytes(data), headers, postNewListener);
	}

	public void writeMaxScore()
	{
		string user = PlayerPrefs.GetString("User");
		string data = "{\"maxScore\":" + maxScore + ",\"user\":\"" + user + "\"}";
		net.POST(gameStorageURL + "storage/update/" + prefix + "/" + maxScoreSuffix, System.Text.Encoding.UTF8.GetBytes(data), headers, postListener);
	}

	public void readMaxScore()
	{
		net.GET(gameStorageURL + "storage/"+ prefix +"/"+ maxScoreSuffix, getListener);
	}

	public void setScoreList(SimpleJSON.JSONNode node)
	{
		bool needUpdate = false;
		String array = "";
		int count = 0; 

		if (node == null)
		{
			setEmptyScore();
			array = "{\"score\":" + score + ",\"day\":\"" + DateTime.Now + "\"},";
			needUpdate = true;
		}
		else
		{
			scoreList = node;

			SimpleJSON.JSONArray list = scoreList.AsArray;
			if (uiListScore != null)
				uiListScore.text = "";
			string aux = "{\"score\":" + score + ",\"day\":\"" + DateTime.Now + "\"}";
			foreach (SimpleJSON.JSONNode n in list)
			{
				if (score > n["score"].AsInt)
				{
					array += aux + ",";
					aux = n.ToString();
					needUpdate = true;
				}

				if (uiListScore != null)
				{
					count++;
					uiListScore.text += count + ".-" + n["score"] + "points at " + n["day"] + '\n' + '\n';
				}
			}
			if (count < 5)
			{
				array += aux + ",";
				needUpdate = true;
			}
		}
		
		if (registerUserScore && needUpdate)
		{
			scoreList = SimpleJSON.JSON.Parse("{\"list\":[" + array.Substring(0, array.Length - 2) + "]}");
			writeScoreList();
		}
	}

	public void readScoreList(string user)
	{
		net.GET(gameStorageURL + "storage/" + prefix + "/" + user, getListListener);
	}

	public void readScoreList()
	{
		string user = PlayerPrefs.GetString("User");
		readScoreList(user);
	}

	public void setEmptyScore()
	{
		if(uiListScore != null)
		{
			uiListScore.text = "NO RESULTS";
		}
	}

	public void writeScoreList()
	{
		string user = PlayerPrefs.GetString("User");
		string data = scoreList.ToString();
		net.POST(gameStorageURL + "storage/update/" + prefix + "/" + user, System.Text.Encoding.UTF8.GetBytes(data), headers, postListListener);
	}

	public void createScoreList()
	{
		string user = PlayerPrefs.GetString("User");
		string data = scoreList.ToString();
		net.POST(gameStorageURL + "storage/" + prefix + "/" + user, System.Text.Encoding.UTF8.GetBytes(data), headers, postNewListListener);
	}
}
