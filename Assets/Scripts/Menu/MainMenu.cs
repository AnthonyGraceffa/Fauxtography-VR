using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Collections;

public class MainMenu : MonoBehaviour {

	GameObject mainMenuCanvas;
	GameObject collectionCanvas;
	Color blackColor;
	GameObject blackCover;

	GameObject OVRCam;

	public GameObject leftCam;
	GameObject lastButtonHovered;
	public GameObject selectBar;
	Slider selectSlider;

	//Camera Points
	public GameObject mainMenuCamPoint;
	public GameObject collectionCamPoint;

	float selectTimer = 0;

	// Use this for initialization
	void Awake () {

		//If no game data exists, create it
		InitFirstSave ();

		//Initialize black screen cover for fade effect
		blackColor = GameObject.Find ("blackCover").GetComponent<Image> ().color;
		blackCover = GameObject.Find ("blackCover");
		OVRCam = GameObject.Find ("OVRCameraRig");

		//turn off select button progress slider
		selectSlider = selectBar.GetComponent<Slider>();
		selectBar.SetActive (false);

		//Set and Disable all canvasies but main menu
		mainMenuCanvas = GameObject.Find ("MenuCanvas");
		collectionCanvas = GameObject.Find ("CollectionCanvas");
		collectionCanvas.SetActive (false);

		//begin fade from black
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
						StartCoroutine(loadLevel("testLevel02"));
						break;

					case "Quit":
						Application.Quit();
						break;

					case "AlbumButton":

						break;

					case "CollectionButton":
						GoToCollection();
						break;

					case "OptionsButton":

						break;
					
					case "CollectionBack":
						GoToMainMenu();
						break;
					}

				}
			}

			//if the user looks away from a button reset the timer.
			if(hit.collider.gameObject.tag == "UIBack" && hit.collider != null) {
				ExecuteEvents.Execute(lastButtonHovered, pointer, ExecuteEvents.pointerExitHandler);
				selectTimer = 0;
				selectSlider.value = selectTimer;
				selectBar.SetActive(false);
			}

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
		collectionCanvas.SetActive (false);
		mainMenuCanvas.SetActive (true);
		
		iTween.MoveTo (OVRCam, mainMenuCamPoint.transform.position, 10f);
		//iTween.LookTo (OVRCam, mainMenuCanvas.transform.position, 10f);
	}

	//Fade in from black at start
	IEnumerator fadeFromBlack () {

		while (blackCover.GetComponent<Image>().color.a > 0.1f) {
			blackCover.GetComponent<Image>().color = Color.Lerp(blackCover.GetComponent<Image>().color, new Color(0,0,0,0),1 * Time.deltaTime);

			yield return null;
		}
		blackCover.SetActive (false);
	}

	//Fade to black and laod scene
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
			List<SnapShot> allTestWorldSnaps = new List<SnapShot>();
			List<SnapShot> allForestSnaps = new List<SnapShot>();
			List<SnapShot> allCanyonSnaps = new List<SnapShot>();
			bool[] subjectsCaught = new bool[100];

			Serializer.Save("allTestWorldSnaps", allTestWorldSnaps);
			Serializer.Save("allForestSnaps", allForestSnaps);
			Serializer.Save("allCanyonSnaps", allCanyonSnaps);
			Serializer.Save("subjectsCaught",subjectsCaught);

		}
	}




}
