using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoard : MonoBehaviour {

	public Score score;
	public UnityEngine.UI.InputField userName;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void updateScoreBoard(){
		score.readScoreList(userName.text);
	}
}
