using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{

	private UnityEngine.UI.Text sex;

	void Start()
	{
		Tracker.T.accessible.Accessed("Menu");
		sex = GameObject.Find("SelectedSex").GetComponent<UnityEngine.UI.Text>();
	}

	public void InitGame(string sceneName)
	{

		if (GameObject.FindObjectOfType<Dropdown>().value != 0)
		{
			PlayerPrefs.SetInt("LevelScore", 0);
			PlayerPrefs.Save();

			Tracker.T.alternative.Selected("Selected sex", sex.text);
			Tracker.T.alternative.Selected("Start game", "Start");
			Tracker.T.accessible.Accessed(sceneName);
			Application.LoadLevel(sceneName);
		}
	}
}
