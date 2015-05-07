using UnityEngine;
using System.Collections;

public class TutorialSound : MonoBehaviour {

	public AudioClip tut1;
	public AudioClip tut2;
	public AudioClip tut3;
	public AudioClip tut4;
	public AudioClip tut5;
	public AudioClip tut6;
	public AudioClip tut7;

	AudioSource AS;
	bool hasDog = false;
	// Use this for initialization
	void Start () {
		AS = this.GetComponent<AudioSource>();

		AS.clip = tut1;
		AS.Play();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void play2 (){
		AS.clip = tut2;
		AS.Play();
	}

	void OnTriggerEnter(Collider col){
		if(col.name == "tut3"){
			AS.clip = tut3;
			AS.Play();
		}
		if(col.name == "tut4"){
			AS.clip = tut4;
			AS.Play();

			StartCoroutine(play5 ());
		}
		if(col.name == "tutorialFork 1"){
			AS.clip = tut7;
			AS.Play();
		}

	}

	IEnumerator play5() {
		yield return new WaitForSeconds(5.2f);
		AS.clip = tut5;
		AS.Play();
	}
	public void play6 (){
		if(!hasDog){
			AS.clip = tut6;
			AS.Play();
			hasDog = true;
		}
	}

}
