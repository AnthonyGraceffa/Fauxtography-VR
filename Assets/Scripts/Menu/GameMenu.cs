using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class GameMenu : MonoBehaviour {


	public float scrollSpeed = 1f;

	float selectTimer = 0;
	GameObject OVRCam;
	
	public GameObject leftCam;

	GameObject lastButtonHovered;
	public GameObject selectBar;
	Slider selectSlider;
	GameObject reviewSnapAnchor;

	// Use this for initialization
	void Awake () {
		OVRCam = GameObject.Find ("OVRCameraRig");
		//turn off select button progress slider
		selectSlider = selectBar.GetComponent<Slider>();
		selectBar.SetActive (false);
		reviewSnapAnchor = GameObject.Find ("snapReviewSpawn");
	}
	
	void Update () {
		
		Ray ray = new Ray(leftCam.transform.position, leftCam.transform.forward * 10);
		RaycastHit hit;
		Debug.DrawRay(ray.origin, ray.direction * 20, Color.red);
		
		if (Physics.Raycast (ray, out hit, 100)) {
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

						case "toMenuButton":
							Application.LoadLevel("MainMenu");
							break;
					}
					
				}

				switch (lastButtonHovered.name) {
					
				case "scrollLeft":
					reviewSnapAnchor.transform.Translate(Vector3.left * Time.deltaTime * scrollSpeed);
					//if the select bar fills up the snaps scroll  faster
					if(selectTimer > 4) {
						scrollSpeed = 3;
					}
					break;
					
				case "scrollRight":
					reviewSnapAnchor.transform.Translate(Vector3.right * Time.deltaTime * scrollSpeed);
					if(selectTimer > 4) {
						scrollSpeed = 3;
					}
					break;
				}

			}
			
			//if the user looks away from a button reset the timer.
			if(hit.collider.gameObject.tag == "UIBack" && hit.collider != null) {
				ExecuteEvents.Execute(lastButtonHovered, pointer, ExecuteEvents.pointerExitHandler);
				selectTimer = 0;
				selectSlider.value = selectTimer;
				selectBar.SetActive(false);

				//Reset scrollSpeed
				scrollSpeed = 1;
			}
			
		}
	}
}
