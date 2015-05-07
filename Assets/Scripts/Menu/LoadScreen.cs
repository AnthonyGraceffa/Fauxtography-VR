using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadScreen : MonoBehaviour {
	
	public float loadBar = 0;
	string loadLevel;
	Slider loadBarObject;

	void Awake () {
		loadBarObject = GameObject.Find("Load Slider").GetComponent<Slider>();
	}

	// Update is called once per frame
	void Update () {
		loadBar = Application.GetStreamProgressForLevel(loadLevel);
		loadBarObject.value = loadBar;
	}

	IEnumerator Start() {
		AsyncOperation async;
		yield return new WaitForSeconds(2);
		switch (PlayerPrefs.GetInt("loadLevel")){
		case 1:
			loadLevel = "tutorial";
			break;

		case 2:
			loadLevel = "Bidwell";			
			break;

		default:
			loadLevel = "tutorial";
			Debug.Log ("For some reason tutorial level was loaded from LoadScreen when it wasn't supposed to");
			break;
		}

		async = Application.LoadLevelAsync(loadLevel);
			yield return async;
			Debug.Log("Loading complete");

	}

}
