using UnityEngine;
using System.Collections;

public class AnsQuestion : MonoBehaviour
{
    public string nextScene;
    public string questionId;
    public int correctScore = 50;
    public int incorrectScore = -15;

    private Tracker tracker;

    void Start()
    {
        tracker = GameObject.Find("Tracker").GetComponent<TrackerWithLog>();
    }

    public void AnswerTheQuestion(bool result)
    {
        if (result)
        {
            Score.S.AddScore(correctScore);
            Application.LoadLevel(nextScene);
            tracker.Zone(nextScene);

        } else
        {
            Score.S.AddScore(incorrectScore);
        }
    }

    public void TrackChoice(string optionId)
    {
        tracker.Choice(questionId, optionId);
    }
}
