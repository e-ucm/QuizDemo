using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameStorageScoreGETListener : Net.IRequestListener
{

	private Score score;

	public GameStorageScoreGETListener(Score score)
	{
		this.score = score;
	}

	public void Result(string data)
	{
		if (data == "" || data == "{}")
		{
			score.SetMaxScore(0, "");
		} else {
			SimpleJSON.JSONNode json = SimpleJSON.JSON.Parse(data);
			score.SetMaxScore(Int32.Parse(json["maxScore"]), json["user"]);
		}
	}

	public void Error(string error)
	{
		score.SetMaxScore(0, "");
	}
}

public class GameStorageNewScorePOSTListener : Net.IRequestListener
{

	private Score score;

	public GameStorageNewScorePOSTListener(Score score)
	{
		this.score = score;
	}

	public void Result(string data)
	{
		Debug.Log("OK creating the maxScore");
	}

	public void Error(string error)
	{
		Debug.LogWarning("Error creating MaxScore:" + error);
	}
}

public class GameStorageScorePOSTListener : Net.IRequestListener
{

	private Score score;

	public GameStorageScorePOSTListener(Score score)
	{
		this.score = score;
	}

	public void Result(string data)
	{
		Debug.Log("OK writing the maxScore");
	}

	public void Error(string error)
	{
		score.createMaxScore();
	}
}

public class GameStorageListGETListener : Net.IRequestListener
{

	private Score score;

	public GameStorageListGETListener(Score score)
	{
		this.score = score;
	}

	public void Result(string data)
	{
		if (data != "" && data != "{}")
		{
			SimpleJSON.JSONNode json = SimpleJSON.JSON.Parse(data);
			SimpleJSON.JSONNode list = json["list"];
			score.setScoreList(list);
		} else
		{
			score.setScoreList(null);
		}
	}

	public void Error(string error)
	{
		Debug.LogWarning("Error: " + error);
	}
}


public class GameStorageNewListPOSTListener : Net.IRequestListener
{

	private Score score;

	public GameStorageNewListPOSTListener(Score score)
	{
		this.score = score;
	}

	public void Result(string data)
	{
		Debug.Log("OK creating the score list");
	}

	public void Error(string error)
	{
		Debug.LogWarning("Error creating score list:" + error);
	}
}

public class GameStorageListPOSTListener : Net.IRequestListener
{

	private Score score;

	public GameStorageListPOSTListener(Score score)
	{
		this.score = score;
	}

	public void Result(string data)
	{
		Debug.Log("OK writing the score list");
	}

	public void Error(string error)
	{
		score.createScoreList();
	}
}
