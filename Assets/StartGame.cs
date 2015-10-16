using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StartGame : MonoBehaviour {

    private Tracker tracker;

    void Start()
    {
        tracker = GameObject.Find("Tracker").GetComponent<Tracker>();
    }

    public void InitGame(string sceneName)
    {
        
        if (GameObject.FindObjectOfType<Dropdown>().value != 0)
        {
            PlayerPrefs.SetInt("LevelScore", 0);
            PlayerPrefs.Save();
           
            tracker.Choice("Start game", "Start");
            tracker.Zone(sceneName);
            Application.LoadLevel(sceneName);
        }
    }
}
