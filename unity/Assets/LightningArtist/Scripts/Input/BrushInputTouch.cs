using UnityEngine;
using System.Collections;

public class BrushInputTouch : MonoBehaviour {

	public LightningArtist lightningArtist;
	public enum DrawMode { FREE, FIXED }
	public DrawMode drawMode = DrawMode.FREE;
	public float zPos = 1f;

	[HideInInspector] public bool touchActive = false;
	[HideInInspector] public bool touchDown = false;
	[HideInInspector] public bool touchUp = false;

	void Awake() {
		if (lightningArtist == null) lightningArtist = GetComponent<LightningArtist>();
	}

	void Start() {
		if (drawMode == DrawMode.FIXED)	lightningArtist.target.transform.SetParent(Camera.main.transform, true);
	}

	void Update() {
		touchDown = false;
		touchUp = false;

		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && GUIUtility.hotControl == 0) { 
			touchActive = true;
			touchDown = true;
		} else if (Input.touchCount < 1 || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) {
			touchActive = false;
			touchUp = true;
		}

		if (touchActive) {
			Vector3 p = lightningArtist.target.transform.position;

			if (drawMode == DrawMode.FREE) {
				p = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, zPos));
			} else if (drawMode == DrawMode.FIXED) {
				// no change
			}
			lightningArtist.target.transform.position = p;
		}

		lightningArtist.clicked = touchActive;

		//if (touchDown) {
		//lightningArtist.inputPlay();
		//}
	}

}
