using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameMaster : MonoBehaviour {

	public GameObject[] allCamTargets;
	[System.NonSerialized]
	public int numOfSnapsTaken = 0;
	[System.NonSerialized]
	public string currentLevel;

	public GameObject endLevelCanvas;
	public GameObject reviewIMGPrefab;

	GameObject photoHUD;
	GameObject snapCounter;
	GameObject player;
	Transform reviewIMGSpawn;
	GameObject menuCanvas;
	

	int subjectsCaptured = 0;


	Text snapCounterText;

	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Player");
		photoHUD = GameObject.Find("UIPhotoHUD");
		//turn off menu navigation script
		player.GetComponent<GameMenu> ().enabled = false;
		menuCanvas = GameObject.Find ("Crosshair");
		menuCanvas.SetActive (false);

		//turn off end level screen
		endLevelCanvas.SetActive (false);

		//find all cam targets
		allCamTargets = GameObject.FindGameObjectsWithTag ("SnapTarget");

		snapCounter = GameObject.Find ("snapsTakenText");
		snapCounterText = snapCounter.GetComponent<Text> ();

		if (Application.loadedLevelName == "tutorial") {
			currentLevel = "Tutorial";
		}
		if (Application.loadedLevelName == "Bidwell") {
			currentLevel = "Bidwell Park";
		}

	}


	//Update snap counter in HUD'
	public void UpdateSnapCount () {
		snapCounterText.text = "Snaps Taken: " + numOfSnapsTaken;
	}

	//Save game after level complete
	public void EndLevelSaveData () {

		//Confirms that a save has been made
		PlayerPrefs.SetInt ("SaveData", 1);

		//Save collections list
		Serializer.Save("subjectsCaught",player.GetComponent<PictureTaker>().collectionArray);

		//saves snaps taken this level to appropraite lists of snaps in playerprefs
		if(currentLevel == "Bidwell Park"){
			List<SnapShot> tempBidwellSnaps = Serializer.Load("allForestSnaps") as List<SnapShot>;
			tempBidwellSnaps.AddRange(player.GetComponent<PictureTaker>().snapsTakenThisLevel);
			Serializer.Save("allForestSnaps", tempBidwellSnaps);
		}
		if(currentLevel == "Tutorial"){
			List<SnapShot> tempBidwellSnaps = Serializer.Load("allTestWorldSnaps") as List<SnapShot>;
			tempBidwellSnaps.AddRange(player.GetComponent<PictureTaker>().snapsTakenThisLevel);
			Serializer.Save("allTestWorldSnaps", tempBidwellSnaps);
		}

	}

	//called when player hits finish collider at end of level
	public void EndLevel () {

		//Turn on menu navigation script 
		player.GetComponent<GameMenu> ().enabled = true;

		//enable menu canvas
		menuCanvas.SetActive (true);

		//Turn on end level canvas
		endLevelCanvas.SetActive (true);

		//load in the snapshot thumb nails for player review
		StartCoroutine(loadReviewSnapThumbs());


		//turn off normal HUD
		GameObject.Find ("UIPhotoHUD").SetActive (false);

		GameObject.Find ("totalSnapsText").GetComponent<Text> ().text = "Total Snaps: " + numOfSnapsTaken;

		subjectsCaptured = player.GetComponent<PictureTaker>().newSubjectsCaptured.Count;
		GameObject.Find ("subjectsCaughtText").GetComponent<Text> ().text = "Subjects Caught: " + subjectsCaptured;

		//disable picture taker
		player.GetComponent<PictureTaker> ().enabled = false;

		//Save game
		EndLevelSaveData ();

	}

	//called when player hits a path fork
	public void SelectPath () {

		//turn off normal HUD
		photoHUD.SetActive (false);

		//enable menu canvas
		menuCanvas.SetActive (true);

		//Turn on menu navigation script 
		player.GetComponent<GameMenu> ().enabled = true;

		//disable picture taker
		player.GetComponent<PictureTaker> ().enabled = false;
	}

	//called when gameplay stops and needs to resume again
	public void reEnableGameplay () {
		//Turn off menu navigation script 
		player.GetComponent<GameMenu> ().enabled = false;

		//enable menu canvas
		menuCanvas.SetActive (false);

		//turn onnormal HUD
		photoHUD.SetActive (true);

		//Enable picture taker
		player.GetComponent<PictureTaker> ().enabled = true;

		//Hide the path prompt canvases
		player.GetComponent<Traveler>().DisablePathPrompts();
	}

	//Displays all the snaps taken on the end screen
	public IEnumerator loadReviewSnapThumbs () {
		float newIMGoffset = 0;		//how far right to spawn the next thumb
		reviewIMGSpawn = GameObject.Find ("snapReviewSpawn").transform;		//position to spawn 
		foreach (SnapShot snap in player.GetComponent<PictureTaker> ().snapsTakenThisLevel) {

			//Find all screenshots taken this and set then to the texture of the prefab being spawned
			string fileName = snap.fileName;
			Byte[] bytes = System.IO.File.ReadAllBytes (fileName);
			Texture2D newImg = new Texture2D (1920, 1080);
			newImg.LoadImage (bytes);

			//Spawn image and then add 200 to offset so the next one appears to the right of the last one
			Vector3 newPos = reviewIMGSpawn.transform.position;
			newPos.x = newPos.x + newIMGoffset;
			newPos.z = 0;
			GameObject newReviewIMG = Instantiate(reviewIMGPrefab,newPos,Quaternion.identity) as GameObject;
			newReviewIMG.transform.SetParent(reviewIMGSpawn);
			newReviewIMG.transform.localPosition = newPos;
			newReviewIMG.transform.Rotate(new Vector3(0,0,0));

			//make sprite with new texture from file
			newReviewIMG.GetComponent<Image> ().sprite = Sprite.Create (newImg, new Rect (0, 0, 1920, 1080), new Vector2 (1, 1));
			newIMGoffset += 140;



		}
		yield return new WaitForEndOfFrame();
	}

	void Update () {
		if(Input.GetButtonDown("Fire2")){
			EndLevelSaveData();
		}
	}
}
