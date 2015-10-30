using UnityEngine;
using System.Collections;

public class AnsQuestion : MonoBehaviour
{
	public string nextScene;
	public string questionId;
	public int correctScore = 50;
	public int incorrectScore = -15;

	public void AnswerTheQuestion(bool result)
	{
		if (result)
		{
			Score.S.AddScore(correctScore);
			Application.LoadLevel(nextScene);
			Tracker.T().Zone(nextScene);

		}
		else
		{
			Score.S.AddScore(incorrectScore);
		}
	}

	public void TrackChoice(string optionId)
	{
		Tracker.T().Choice(questionId, optionId);
	}
}
