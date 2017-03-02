using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
			StartCoroutine(WaitForIt(0.2F));
			Tracker.T.accessible.Accessed(nextScene);

        }
		else
		{
			Score.S.AddScore(incorrectScore);
		}
	}

	IEnumerator WaitForIt(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		SceneManager.LoadScene(nextScene);
	}

	public void TrackChoice(string optionId)
	{
        Tracker.T.alternative.Selected(questionId, optionId);
    }
}
