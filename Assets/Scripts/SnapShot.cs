using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class SnapShot  {

	public string fileName;								//File name and size used for retrieving 

	public string level;								//Level where snap was taken
	public List<string> capturedSubjects;				//Not used anymore I think
	public List<SnapSubject> subjectsInFrame;			//List of all subjects caught in snap
	
	public SnapShot (string file, string curLevel) {
		fileName = file;
		level = curLevel;
		subjectsInFrame = new List<SnapSubject> ();
	}

	//Holds info for snap subjects caught in a snapshot
	[System.Serializable]
	public class SnapSubject {
		public string name;			//Name of subject
		public bool centered;		//If subject is in center of picture
		public float distance;		//The distance the subject is from the camera

		float horizontalAngle;		//Angle in frame from left side
		float verticalAngle;		//Angle in frame from down

		public SnapSubject (string n, float dist, bool center) {
			name = n;
			distance = dist;
			centered = center;
		}
	}
}
