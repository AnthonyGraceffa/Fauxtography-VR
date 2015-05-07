using UnityEngine;
using System.Collections;

public class Tutorial : MonoBehaviour {

	public GameObject fork1;

	// Use this for initialization
	void Awake () {
		StartCoroutine(waitForFork1());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator waitForFork1 () {
		yield return new WaitForSeconds(10);

		fork1.SetActive(true);

	}
}
