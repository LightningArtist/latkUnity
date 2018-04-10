using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkInputGamepad : MonoBehaviour {

	public LightningArtist latk;

	private bool blockStickHMax = false;
	private bool blockStickHMin = false;
	private bool blockStickVMax = false;
	private bool blockStickVMin = false;
	private float stickThreshold = 0.9f;

	void Update() {
		latk.clicked = Input.GetButton("Fire2");

		if (Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.P)) {
			latk.inputPlay();
		}

		if (Input.GetButtonDown("Jump")) {
			latk.inputFrameBack();
		}

		if (Input.GetButtonDown("Fire1")) {
			latk.inputFrameForward();
		}

		if (!blockStickHMax && Input.GetAxis("Horizontal") >= stickThreshold) {
			blockStickHMax = true;
			if (latk.layerList[latk.currentLayer].frameList.Count > 0)
				latk.inputShowFrames();
		} else if (blockStickHMax && Input.GetAxis("Horizontal") < stickThreshold) {
			blockStickHMax = false;
		}

		if (!blockStickHMin && Input.GetAxis("Horizontal") <= -stickThreshold) {
			blockStickHMin = true;
			if (latk.layerList[latk.currentLayer].frameList.Count > 0)
				latk.inputHideFrames();
		} else if (blockStickHMin && Input.GetAxis("Horizontal") > -stickThreshold) {
			blockStickHMin = false;
		}
	}

}
