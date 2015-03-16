using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Collector : MonoBehaviour {

	public string nameOfSubject;
	public bool isText = false;
	public int collectionNumber = 0;
	public Sprite thumbnail;

	// Use this for initialization
	void Awake () {
		bool[] collectionArray = (bool[])Serializer.Load ("subjectsCaught");
		if (!isText) {
			if (collectionArray [collectionNumber] == true) {
				this.GetComponent<Image> ().sprite = thumbnail;
			}
		} else {
			if (collectionArray [collectionNumber] == true) {
				this.GetComponent<Text>().text = nameOfSubject;
			}
		}
	}
}
