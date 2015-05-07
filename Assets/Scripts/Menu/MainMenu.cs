using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Collections;

public class MainMenu : MonoBehaviour {

	//Canvasies
	GameObject mainMenuCanvas;
	GameObject collectionCanvas;
	GameObject albumCanvas;
	GameObject blackCover;

	GameObject OVRCam;

	public GameObject leftCam;
	GameObject lastButtonHovered;
	public GameObject selectBar;
	Slider selectSlider;

	//Camera Points
	public GameObject mainMenuCamPoint;
	public GameObject collectionCamPoint;
	public GameObject albumCamPoint;

	//Collections
	public GameObject testWorldCollection;
	public GameObject forestCollection;

	//Album Image Review
	public GameObject reviewIMGPrefab;
	GameObject albumPreview;
	string currentAlbumPage;
	public float scrollSpeed = 1f;
	GameObject reviewIMGSpawn;
	bool visitedAlbum = false;					//This is temp until more levels are in the game

	//Menu Other
	float selectTimer = 0;

	// Use this for initialization
	void Awake () {

		//If no game data exists, create it
		InitFirstSave ();

		//set gameobjects
		reviewIMGSpawn = GameObject.Find ("albumSpawn");

		//Initialize black screen cover for fade effect
		blackCover = GameObject.Find ("blackCover");
		OVRCam = GameObject.Find ("OVRCameraRig");

		//turn off select button progress slider
		selectSlider = selectBar.GetComponent<Slider>();
		selectBar.SetActive (false);

		//Set and Disable all canvasies but main menu
		mainMenuCanvas = GameObject.Find ("MenuCanvas");
		collectionCanvas = GameObject.Find ("CollectionCanvas");
		albumCanvas = GameObject.Find ("AlbumCanvas");
		albumPreview = GameObject.Find("albumPreview");
		albumCanvas.SetActive(false);
		collectionCanvas.SetActive (false);

		//Begin fade from black
		StartCoroutine (fadeFromBlack());
	}
	
	// Update is called once per frame
	void Update () {

		Ray ray = new Ray(leftCam.transform.position, leftCam.transform.forward * 10);
		RaycastHit hit;
		Debug.DrawRay(ray.origin, ray.direction * 20, Color.red);

		if (Physics.Raycast (ray, out hit, 25)) {
			var pointer = new PointerEventData(EventSystem.current);

			//start doing things if a Ui button is being stared at
			if(hit.collider.gameObject.tag == "UIButton" && hit.collider != null) {
				selectBar.SetActive(true);
				ExecuteEvents.Execute(hit.collider.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
				lastButtonHovered = hit.collider.gameObject;
				selectTimer += 2 * Time.deltaTime;

				selectSlider.value = selectTimer;

				//Do a different thing based on which button was stared at intently
				if(selectTimer > 4){
					switch (lastButtonHovered.name) {

					case "TestWorldButton":
						PlayerPrefs.SetInt("loadLevel",1);
						StartCoroutine(loadLevel("Loading"));
						break;

					case "Bidwell Button":
						PlayerPrefs.SetInt("loadLevel",2);
						StartCoroutine(loadLevel("Loading"));
						break;

					case "Quit":
						Application.Quit();
						break;

					case "AlbumButton":
						GoToAlbum();
						break;

					case "CollectionButton":
						GoToCollection();
						break;

					case "OptionsButton":

						break;
					
					case "CollectionBack":
						GoToMainMenu();
						break;
					
					case "testWorldCollection":
						forestCollection.SetActive(false);
						testWorldCollection.SetActive(true);
						break;

					case "BidwellCollection":
						testWorldCollection.SetActive(false);
						forestCollection.SetActive(true);
						break;

					case "CanyonCollection":
						forestCollection.SetActive(false);
						testWorldCollection.SetActive(false);
						break;
					case "albumBidwell":
						loadBidwellSnapThumbs();
						break;
					case "albumBack":
						GoToMainMenu();
						break;
					
					}


				}

				switch(lastButtonHovered.name){
				case "scrollLeft":
					reviewIMGSpawn.transform.Translate(Vector3.left * Time.deltaTime * scrollSpeed);
					//if the select bar fills up the snaps scroll  faster
					if(selectTimer > 4) {
						scrollSpeed = 3;
					}
					break;
					
				case "scrollRight":
					reviewIMGSpawn.transform.Translate(Vector3.right * Time.deltaTime * scrollSpeed);
					if(selectTimer > 4) {
						scrollSpeed = 3;
					}
					break;
				}
			}
			if(hit.collider.gameObject.tag == "UIAlbum" && hit.collider != null) {
				ExecuteEvents.Execute(hit.collider.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
				lastButtonHovered = hit.collider.gameObject;

				albumPreview.GetComponent<Image>().sprite = hit.collider.GetComponent<Image>().sprite;
			}
			//if the user looks away from a button reset the timer.
			if(hit.collider.gameObject.tag == "UIBack" && hit.collider != null) {
				ExecuteEvents.Execute(lastButtonHovered, pointer, ExecuteEvents.pointerExitHandler);
				selectTimer = 0;
				selectSlider.value = selectTimer;
				selectBar.SetActive(false);
			}

		}

		if (Input.GetKeyDown ("k")) {
			ClearSaveData();
			Debug.Log ("MainMenu:Update() - Reset Save Data");
		}
	}

	void GoToCollection () {
		//Set correct canvas on
		mainMenuCanvas.SetActive (false);
		collectionCanvas.SetActive (true);

		iTween.MoveTo (OVRCam, collectionCamPoint.transform.position, 10f);
		iTween.LookTo (OVRCam, collectionCanvas.transform.position, 10f);
	}

	void GoToMainMenu () {
		//Set correct canvas on
		albumCanvas.SetActive(false);
		collectionCanvas.SetActive (false);
		mainMenuCanvas.SetActive (true);
		
		iTween.MoveTo (OVRCam, mainMenuCamPoint.transform.position, 10f);
		//iTween.LookTo (OVRCam, mainMenuCanvas.transform.position, 10f);
	}

	void GoToAlbum () {
		mainMenuCanvas.SetActive (false);
		albumCanvas.SetActive(true);
		if(!visitedAlbum){
			StartCoroutine(loadBidwellSnapThumbs());
			visitedAlbum = true;
		}

		iTween.MoveTo (OVRCam, albumCamPoint.transform.position, 10f);
	}

	//Fade in from black at start
	IEnumerator fadeFromBlack () {

		while (blackCover.GetComponent<Image>().color.a > 0.1f) {
			blackCover.GetComponent<Image>().color = Color.Lerp(blackCover.GetComponent<Image>().color, new Color(0,0,0,0),1 * Time.deltaTime);

			yield return null;
		}
		blackCover.SetActive (false);
	}

	//Fade to black and load scene
	IEnumerator loadLevel (string scene) {
		blackCover.SetActive (true);
		while (blackCover.GetComponent<Image>().color.a > 254f) {
			blackCover.GetComponent<Image> ().color = Color.Lerp (blackCover.GetComponent<Image> ().color, new Color (0, 0, 0, 255), 1 * Time.deltaTime);

			yield return null;
		}

		yield return new WaitForSeconds(1f);
		Application.LoadLevel (scene);
	}

	//If there is no Save data create a blank save
	public void InitFirstSave () {
		if (!PlayerPrefs.HasKey ("SaveData")) {
			List<SnapShot> allTestWorldSnaps = new List<SnapShot> ();
			List<SnapShot> allForestSnaps = new List<SnapShot> ();
			List<SnapShot> allCanyonSnaps = new List<SnapShot> ();
			bool[] subjectsCaught = new bool[100];

			Serializer.Save ("allTestWorldSnaps", allTestWorldSnaps);
			Serializer.Save ("allForestSnaps", allForestSnaps);
			Serializer.Save ("allCanyonSnaps", allCanyonSnaps);
			Serializer.Save ("subjectsCaught", subjectsCaught);

		}
	}

	//Saves a clean file
	public void ClearSaveData () {
		List<SnapShot> allTestWorldSnaps = new List<SnapShot> ();
		List<SnapShot> allForestSnaps = new List<SnapShot> ();
		List<SnapShot> allCanyonSnaps = new List<SnapShot> ();
		bool[] subjectsCaught = new bool[100];
		
		Serializer.Save ("allTestWorldSnaps", allTestWorldSnaps);
		Serializer.Save ("allForestSnaps", allForestSnaps);
		Serializer.Save ("allCanyonSnaps", allCanyonSnaps);
		Serializer.Save ("subjectsCaught", subjectsCaught);

	}

	//Displays line of snaps on slbum page to scroll through
	public IEnumerator loadBidwellSnapThumbs () {

		//create lists of snaps from snaps stored in PP
		List<SnapShot> bidwellSnaps = Serializer.Load("allForestSnaps") as List<SnapShot>;

		bool firstImage = true;
		float newIMGoffset = 0;		//how far right to spawn the next thumb
		Transform spawnTrans = reviewIMGSpawn.transform;		//position to spawn 
		foreach (SnapShot snap in bidwellSnaps) {
			
			//Find all screenshots on file and load their images as sprites
			string fileName = snap.fileName;
			Byte[] bytes = System.IO.File.ReadAllBytes (fileName);
			Texture2D newImg = new Texture2D (1920, 1080);
			newImg.LoadImage (bytes);
			
			//Spawn image and then add 200 to offset so the next one appears to the right of the last one
			Vector3 newPos = reviewIMGSpawn.transform.position;
			newPos.x = newPos.x + newIMGoffset;
			newPos.z = 0;
			GameObject newReviewIMG = Instantiate(reviewIMGPrefab,newPos,Quaternion.identity) as GameObject;
			newReviewIMG.transform.SetParent(reviewIMGSpawn.transform);
			newReviewIMG.transform.localPosition = newPos;
			newReviewIMG.transform.Rotate(new Vector3(0,90,0));
			newReviewIMG.transform.localScale /=6;
			
			//make sprite with new texture from file
			newReviewIMG.GetComponent<Image> ().sprite = Sprite.Create (newImg, new Rect (0, 0, 1920, 1080), new Vector2 (1, 1));
			newIMGoffset += 140;

			//if were on the first element in the list, set the first album preview to the image
			if(firstImage){
				albumPreview.GetComponent<Image>().sprite = newReviewIMG.GetComponent<Image>().sprite;
				firstImage = false;
			}
			
		}

		yield return new WaitForEndOfFrame();
	}


}
