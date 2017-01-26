using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System;

public class TrackerOpened : Tracker
{
	public List<String> GetQueue()
	{
		return base.queue;
	}
}

public class TrackerTest {

	[Test]
	public void EditorTest() {

		//Arrange
		var gameObject = new GameObject();

		//Act
		//Try to rename the GameObject
		var newGameObjectName = "My game object";
		gameObject.name = newGameObjectName;

		//Assert
		//The object has a new name
		Assert.AreEqual(newGameObjectName, gameObject.name);

		TrackerOpened t = new TrackerOpened();
		t.Start();
		t.ActionTrace("Verbo", "Type", "ID");

 		string traceWithoutTimestamp = t.GetQueue()[0].Substring(t.GetQueue()[0].IndexOf(',')+1);
		
		Assert.AreEqual(traceWithoutTimestamp, "Verbo,Type,ID");

	}
}
