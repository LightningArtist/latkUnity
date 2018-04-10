using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkInputMouse : MonoBehaviour {

	public LightningArtist latk;
	public LatkInputButtons brushInputButtons;
	public float zPos = 1f;

	private void Awake() {
		if (latk == null) latk = GetComponent<LightningArtist>();
		if (brushInputButtons == null) brushInputButtons = GetComponent<LatkInputButtons>();
	}

	private void Update() {
		// draw
		if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
			Vector3 mousePos = Vector3.zero;
			mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zPos));
            latk.target.transform.position = mousePos;
            latk.clicked = true;
		} else {
			latk.clicked = false;
		}
	}

}
