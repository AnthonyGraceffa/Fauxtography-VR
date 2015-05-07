using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PictureTaker : MonoBehaviour {

	public Camera VRCamLeft;
	public Camera VRCamRight;
	public Camera SnapShotCam;
	public List<SnapShot> snapsTakenThisLevel;
	List<string> subjectsInLastSnap;					//Only contains subjects in center of frame
	public List<string> newSubjectsCaptured;
	GameObject GameMaster;
	public static GameMaster GM;

	public RenderTexture overviewTexture;
	public string path = "";
	public bool[] collectionArray;

	//Black thingies for snap animation
	GameObject topBlackSnapper;
	GameObject bottomBlackSnapper;
	Animation snapAnimation;

	//Audio
	AudioSource speaker;

	//Game States
	bool canSnap = true;



	// Used for initialization
	void Awake () {
		GameMaster = GameObject.Find ("GameMaster");
		GM = GameMaster.GetComponent<GameMaster> ();
		snapsTakenThisLevel = new List<SnapShot> ();
		subjectsInLastSnap = new List<string> ();
		collectionArray = (bool[])Serializer.Load ("subjectsCaught");
		SnapShotCam.gameObject.SetActive (false);

		//hide and set snap animation thingies
		topBlackSnapper = GameObject.Find ("snapAni1");
		bottomBlackSnapper = GameObject.Find ("snapAni2");
		topBlackSnapper.SetActive (false);
		bottomBlackSnapper.SetActive (false);
		snapAnimation = GameObject.Find ("UIPhotoHUD").GetComponent<Animation> ();

		//Audio
		speaker = GetComponent<AudioSource> ();

	}

	// LateUpdate is called once per frame after update
	void LateUpdate () {

		//If player clicks, take a snapshot
		if(Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")){
			//CanSnap check doesn't do anything right now
			if(canSnap){
				canSnap = false;
				StartCoroutine(playSnapAnimation());
				speaker.Play();
				StartCoroutine(TakeScreenShot());
				Debug.Log("PictureTaker:LateUpdate - Took Snap!");
			}
		}

	}

	//Create a new Vector3 inbetween both cameras
	Vector3 GetCenterOfCameras () {
		Vector3 newPos;
		newPos.y = (VRCamLeft.transform.position.y + VRCamRight.transform.position.y)/2;
		newPos.x = (VRCamLeft.transform.position.x + VRCamRight.transform.position.x)/2;
		newPos.z = (VRCamLeft.transform.position.z + VRCamRight.transform.position.z)/2;
		return newPos;
	}

	//Rotate snapshot cam to something close to where it should be
	Quaternion GetNewCameraRotation () {
		Quaternion newRotation = VRCamLeft.transform.rotation;
		return newRotation;
	}
	
	//Set filename for screenshots
	string fileName(int width, int height) {
		return string.Format("screen_{0}x{1}_{2}.png",
		                     width, height,
		                     System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}

	//Take screenshot from snapcam and save to persistant filepath
	public IEnumerator TakeScreenShot() {
		yield return new WaitForEndOfFrame();

		SnapShotCam.gameObject.SetActive (true);
		SnapShotCam.transform.position = GetCenterOfCameras ();
		SnapShotCam.transform.rotation = GetNewCameraRotation ();


		//Get frame from rendertex
		RenderTexture currentRT = RenderTexture.active;
		
		RenderTexture.active = SnapShotCam.targetTexture;
		SnapShotCam.Render();
		Texture2D imageOverview = new Texture2D(SnapShotCam.targetTexture.width, SnapShotCam.targetTexture.height, TextureFormat.RGB24, false);
		imageOverview.ReadPixels(new Rect(0, 0, SnapShotCam.targetTexture.width, SnapShotCam.targetTexture.height), 0, 0);
		imageOverview.Apply();
		
		RenderTexture.active = currentRT;
		
		
		// Encode texture into PNG
		byte[] bytes = imageOverview.EncodeToPNG();
		
		// Save in memory
		string filename = fileName(Convert.ToInt32(imageOverview.width), Convert.ToInt32(imageOverview.height));
		path = Application.persistentDataPath  + filename;


		System.IO.File.WriteAllBytes(path, bytes);

		//Create game data
		SnapShot newSnap = new SnapShot (Application.persistentDataPath  + filename, "Bidwell");
		newSnap.capturedSubjects = subjectsInLastSnap;

		//Fill targetsInFrame with every gameobject marked as a snap target that a camera can see
		List<GameObject> targetsInFrame = CalculateSnapSubjects ();

		//Vectors for calculating angle of picture targets
		//Vector3 leftCamVector = -SnapShotCam.transform.right;
		Vector3 forwardCamVector = SnapShotCam.transform.forward;
		//Vector3 downCamVector = -SnapShotCam.transform.up;
		Vector3 towardsTarget = Vector3.zero;

		//Iterate through targets in frame list and find the angles and name for each one, then add it
		//to the subjects in frame list in the newSnap
		Transform middleCam = VRCamLeft.transform;
		middleCam.position = GetCenterOfCameras ();

		//for checking if subject is centered
		bool isCentered = false;
		
		foreach (GameObject curTarget in targetsInFrame) {
			towardsTarget = curTarget.transform.position - SnapShotCam.transform.position;

			Ray ray = new Ray(middleCam.position, towardsTarget);
			RaycastHit hit;

			//if the object is less that 20 degrees away from center then mark it as centered
			isCentered = false;
			if(Vector3.Angle(towardsTarget,forwardCamVector) < 20f){
				isCentered = true;
			}


			//if ray hits snap target then it is in LoS
			if (Physics.Raycast(ray, out hit)) {
				if(hit.collider.gameObject.tag == "SnapTarget" && hit.collider != null) {
					Debug.Log ("PictureTaker: TakeScreenShot() - subject has been hit by ray");
					newSnap.subjectsInFrame.Add(new SnapShot.SnapSubject(curTarget.name,towardsTarget.magnitude,isCentered));
					UpdateCollectionArray(curTarget.name);
				}
			}

		}


		//Add new subjects to new subject list
		addNewSubjectsToList (newSnap);

		//Update UI stuff
		snapsTakenThisLevel.Add (newSnap);
		GM.numOfSnapsTaken ++;
		GM.UpdateSnapCount ();

	}

	//Saves the game relevant information about all the snaps taken during this level
	void SaveSnapInfo () {
		Serializer.Save ("snapsLastLevel", snapsTakenThisLevel);
	}

	//Find camera targets in frame and return a list of all of them
	List<GameObject> CalculateSnapSubjects () {

		//Find what cam targets are actually seen by any camera
		List<GameObject> targetsInFrame = new List<GameObject>();

		foreach (GameObject curTar in GameMaster.GetComponent<GameMaster>().allCamTargets) {
			if(curTar.GetComponent<Renderer>().isVisible){
				targetsInFrame.Add(curTar);
			}
		}

		return targetsInFrame;
	}

	//Find unique subjects in snaps and add them to list if they are new
	void addNewSubjectsToList (SnapShot snap) {
		foreach (SnapShot.SnapSubject subject in snap.subjectsInFrame){
			if (!newSubjectsCaptured.Contains(subject.name)){
				newSubjectsCaptured.Add(subject.name);
			}
		}
	}

	//If a new subject is caught save it to areray
	void UpdateCollectionArray (string subject) {
		int num = 0;
		switch (subject) {
		case "Puppy":
			num = 1;
			this.GetComponent<Traveler>().finishTutorial();
			break;
		case "Seagull":
			num = 2;
			break;
		case "Metal Squirrel":
			num = 3;
			break;
		case "Brown Squirrel":
			num = 4;
			break;
		case "Brown Horse":
			num = 5;
			break;
		case "Brown Bear":
			num = 6;
			break;
		case "Magpie":
			num = 7;
			break;
		case "Deer":
			num = 8;
			break;
		case "Doe":
			num = 9;
			break;
		case "Shiba Puppy":
			num = 10;
			break;
		case "Eagle":
			num = 11;
			break;
		//greyhound
		case "Greyhound":
			num = 12;
			break;
		//Molten Dog
		case "spine1_loResSpine5":
			num = 13;
			break;
		case "TyannosaurusRex":
			num = 14;
			break;
		default:
			num = 0;
			break;
		}
		collectionArray [num] = true;

	}

	//Plays picture snap animation
	IEnumerator playSnapAnimation () {
		topBlackSnapper.SetActive (true);
		bottomBlackSnapper.SetActive (true);

		snapAnimation.Play ();
		while (snapAnimation.isPlaying) {
			yield return new WaitForEndOfFrame ();
		}
		canSnap = true;
		topBlackSnapper.SetActive (false);
		bottomBlackSnapper.SetActive (false);

	}

}
