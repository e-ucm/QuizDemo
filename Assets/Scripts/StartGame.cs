using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
	public UnityEngine.UI.InputField userName;
	public UnityEngine.UI.Text errorText;
	private UnityEngine.UI.Text sex;

	void Start()
	{
		Tracker.T.accessible.Accessed("Menu");
		sex = GameObject.Find("SelectedSex").GetComponent<UnityEngine.UI.Text>();
	}

	public void InitGame(string sceneName)
	{
		Debug.Log(userName);
		if (userName.text.Equals(""))
		{
			errorText.text = "The user name box is empty";
			return;
		}
		else if (GameObject.FindObjectOfType<Dropdown>().value == 0)
		{
			errorText.text = "Select a sex in the dropbox";
			return;
		}

		PlayerPrefs.SetInt("LevelScore", 0);
		PlayerPrefs.SetString("User", userName.text);
		PlayerPrefs.Save();

		Tracker.T.alternative.Selected("Selected sex", sex.text);
		Tracker.T.alternative.Selected("Start game", "Start");
		Tracker.T.accessible.Accessed(sceneName);
		SceneManager.LoadScene(sceneName);
	}
}
