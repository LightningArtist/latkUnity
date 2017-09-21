using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushInputMouse : MonoBehaviour {

	public LightningArtist lightningArtist;
	public BrushInputButtons brushInputButtons;
	public float zPos = 1f;

	private void Awake() {
		if (lightningArtist == null) lightningArtist = GetComponent<LightningArtist>();
		if (brushInputButtons == null) brushInputButtons = GetComponent<BrushInputButtons>();
	}

	private void Update() {
		// draw
		if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
			Vector3 mousePos = Vector3.zero;
			mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zPos));
			lightningArtist.target.transform.position = mousePos;
			lightningArtist.clicked = true;
		} else {
			lightningArtist.clicked = false;
		}
	}

}
