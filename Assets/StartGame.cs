using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StartGame : MonoBehaviour {

    private Tracker tracker;

    private UnityEngine.UI.Text sex;

    void Start()
    {
        tracker = GameObject.Find("Tracker").GetComponent<TrackerWithLog>();
        sex = GameObject.Find("SelectedSex").GetComponent<UnityEngine.UI.Text>();
    }

    public void InitGame(string sceneName)
    {
        
        if (GameObject.FindObjectOfType<Dropdown>().value != 0)
        {
            PlayerPrefs.SetInt("LevelScore", 0);
            PlayerPrefs.Save();

            tracker.Choice("Selected sex", sex.text);
            tracker.Choice("Start game", "Start");
            tracker.Zone(sceneName);
            Application.LoadLevel(sceneName);
        }
    }
}
