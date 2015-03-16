using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Traveler : MonoBehaviour {

	GameObject blackCover;

	// Use this for initialization
	void Start () {
		blackCover = GameObject.Find ("blackCover");
		StartCoroutine (fadeFromBlack ());
	}
	
	// Update is called once per frame
	void Update () {

	}

	IEnumerator fadeFromBlack () {
		
		while (blackCover.GetComponent<Image>().color.a > 0.1f) {
			blackCover.GetComponent<Image>().color = Color.Lerp(blackCover.GetComponent<Image>().color, new Color(0,0,0,0),1 * Time.deltaTime);
			
			yield return null;
		}
		blackCover.SetActive (false);

		yield return new WaitForSeconds(4f);
		iTween.MoveTo (gameObject, iTween.Hash("path", iTweenPath.GetPath("testPath1"),"time",10));
	}

	void OnTriggerEnter(Collider col){
		if (col.gameObject.tag == "Finish") {
			GameObject.Find("GameMaster").GetComponent<GameMaster>().EndLevel();
			Destroy(col.gameObject);
		}
	}

}
