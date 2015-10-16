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
        tracker = GameObject.Find("Tracker").GetComponent<Tracker>();
    }

    public void AnswerTheQuestion(bool result)
    {
        tracker.Zone(nextScene);
        if (result)
        {
            Score.S.AddScore(correctScore);
            Application.LoadLevel(nextScene);

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
