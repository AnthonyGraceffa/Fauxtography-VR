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


	// Used for initialization
	void Awake () {
		GameMaster = GameObject.Find ("GameMaster");
		GM = GameMaster.GetComponent<GameMaster> ();
		snapsTakenThisLevel = new List<SnapShot> ();
		subjectsInLastSnap = new List<string> ();

		SnapShotCam.gameObject.SetActive (false);

	}

	//Called once per frame
	void Update () {
		//for debugging, prints file name of last snap
		if (Input.GetButtonDown ("Jump")) {
			//Debug.Log (snapsTakenThisLevel[snapsTakenThisLevel.Count -1].fileName);
			int count = 0;
			foreach (SnapShot curSnap in snapsTakenThisLevel){
				count ++;
				Debug.Log("Snap Number: " + count);
				foreach (SnapShot.SnapSubject curSubject in curSnap.subjectsInFrame) {
					Debug.Log ("Name: " + curSubject.name);
				}
			}
		}
	}

	// LateUpdate is called once per frame after update
	void LateUpdate () {

		//If player clicks, take a snapshot
		if(Input.GetButtonDown("Fire1")){
			StartCoroutine(TakeScreenShot());
			Debug.Log("PictureTaker:LateUpdate - Took Snap!");
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
		path = Application.persistentDataPath + "/Snapshots/" + filename;


		System.IO.File.WriteAllBytes(path, bytes);

		//Create game data
		SnapShot newSnap = new SnapShot (Application.persistentDataPath + "/Snapshots/" + filename, "TestLevel");
		newSnap.capturedSubjects = subjectsInLastSnap;

		//Fill targetsInFrame with every gameobject marked as a snap target that a camera can see
		List<GameObject> targetsInFrame = CalculateSnapSubjects ();

		//Vectors for calculating angle of picture targets
		Vector3 leftCamVector = -SnapShotCam.transform.right;
		Vector3 forwardCamVector = SnapShotCam.transform.forward;
		Vector3 downCamVector = -SnapShotCam.transform.up;
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
			if (Physics.Raycast(ray, out hit, 25)) {
				if(hit.collider.gameObject.tag == "SnapTarget" && hit.collider != null) {
					newSnap.subjectsInFrame.Add(new SnapShot.SnapSubject(curTarget.name,towardsTarget.magnitude,isCentered));
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
}
