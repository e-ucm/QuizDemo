using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {

    public static Score S;

    private int score;
    private UnityEngine.UI.Text uiScore;
    private Tracker tracker;

    void Start()
    {
        S = this;
        this.uiScore = GetComponentInParent<UnityEngine.UI.Text>();
        score = PlayerPrefs.GetInt("LevelScore");
        this.DisplayScore();
        tracker = GameObject.Find("Tracker").GetComponent<Tracker>();
    }

    public void AddScore(int s)
    {
        score += s;
        DisplayScore();
        ModifyScore();
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
            tracker.Var("Score", score);
        }
    }
}
