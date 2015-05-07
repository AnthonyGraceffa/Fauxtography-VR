using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Traveler : MonoBehaviour {

	public GameObject blackCover;
	public GameObject pathPrompt1;
	public GameObject pathprompt2;
	
	bool atCapPoint = false;
	// Use this for initialization
	void Start () {
		pathPrompt1.SetActive(false);
		pathprompt2.SetActive(false);
		//blackCover = GameObject.Find ("blackCover");
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

	}

	void OnTriggerEnter(Collider col){
		if (col.gameObject.tag == "Finish") {
			GameObject.Find("GameMaster").GetComponent<GameMaster>().EndLevel();
			Destroy(col.gameObject);
		}

		if (col.gameObject.name == "pathFork1") {
			GameObject.Find("GameMaster").GetComponent<GameMaster>().SelectPath();
			pathPrompt1.SetActive(true);
			pathprompt2.SetActive(true);
			Destroy(col.gameObject);
		}
		if (col.gameObject.name == "eventTrigger1") {
			GameObject.Find ("Brown Horse Ani").GetComponent<Animator>().SetBool ("walkTime",true);


		}
		if(col.gameObject.name == "tutorialFork"){
			atCapPoint = true;
		}

	}

	public void DisablePathPrompts () {
		pathPrompt1.SetActive(false);
		pathprompt2.SetActive(false);
	}

	public void finishTutorial () {
		if(atCapPoint){
			this.GetComponent<Animator>().SetInteger("pathNumber",3);
			GameObject.Find ("tutSound").GetComponent<TutorialSound>().play6();
		}
	}
	

}
