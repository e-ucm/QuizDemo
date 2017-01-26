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

	private TrackerOpened t;

	[Test]
	public void ActionTraceTest() {

		t = new TrackerOpened();
		t.Start();


		t.ActionTrace("Verb", "Type", "ID");
		CheckGenericTrace("Verb,Type,ID");
		t.GetQueue().Clear();

		t.ActionTrace("Verb", "Ty,pe", "ID");
		CheckGenericTrace("Verb,Ty/,pe,ID");
		t.GetQueue().Clear();

		t.ActionTrace("Verb", "Type", "I,D");
		CheckGenericTrace("Verb,Type,I/,D");
		t.GetQueue().Clear();

		t.ActionTrace("Ve,rb", "Type", "ID");
		CheckGenericTrace("Ve/,rb,Type,ID");
		t.GetQueue().Clear();

		//Check that null and empty string throw a controled exception
		Assert.Throws(typeof(NullReferenceException), delegate { t.ActionTrace(null, "Type", "ID"); });
		Assert.Throws(typeof(NullReferenceException), delegate { t.ActionTrace("Verb", null, "ID"); });
		Assert.Throws(typeof(NullReferenceException), delegate { t.ActionTrace("Verb", "Type", null); });

		Assert.Throws(typeof(NullReferenceException), delegate { t.ActionTrace("", "Type", "ID"); });
		Assert.Throws(typeof(NullReferenceException), delegate { t.ActionTrace("Verb", "", "ID"); });
		Assert.Throws(typeof(NullReferenceException), delegate { t.ActionTrace("Verb", "Type", ""); });

	}

	[Test]
	public void AlternativeTraceTest()
	{

		t = new TrackerOpened();
		t.Start();


		t.alternative.Selected("question", "alternative");
		CheckGenericTrace("selected,alternative,question,response,alternative");
		t.GetQueue().Clear();
	}

	private void CheckGenericTrace(String espectedValue)
	{
		string traceWithoutTimestamp = t.GetQueue()[0].Substring(t.GetQueue()[0].IndexOf(',') + 1);

		Assert.AreEqual(traceWithoutTimestamp, espectedValue);
		
	}
}
