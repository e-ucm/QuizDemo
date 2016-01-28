using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{

	private UnityEngine.UI.Text sex;

	void Start()
	{
		Tracker.T().Screen("Menu");
		sex = GameObject.Find("SelectedSex").GetComponent<UnityEngine.UI.Text>();
	}

	public void InitGame(string sceneName)
	{

		if (GameObject.FindObjectOfType<Dropdown>().value != 0)
		{
			PlayerPrefs.SetInt("LevelScore", 0);
			PlayerPrefs.Save();

			Tracker.T().Choice("Selected sex", sex.text);
			Tracker.T().Choice("Start game", "Start");
			Tracker.T().Zone(sceneName);
			SceneManager.LoadScene(sceneName);
		}
	}
}
