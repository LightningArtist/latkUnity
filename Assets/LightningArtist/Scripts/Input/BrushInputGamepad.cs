using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushInputGamepad : MonoBehaviour {

	public LightningArtist lightningArtist;

	private bool blockStickHMax = false;
	private bool blockStickHMin = false;
	private bool blockStickVMax = false;
	private bool blockStickVMin = false;
	private float stickThreshold = 0.9f;

	void Update() {
		lightningArtist.clicked = Input.GetButton("Fire2");

		if (Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.P)) {
			lightningArtist.inputPlay();
		}

		if (Input.GetButtonDown("Jump")) {
			lightningArtist.inputFrameBack();
		}

		if (Input.GetButtonDown("Fire1")) {
			lightningArtist.inputFrameForward();
		}

		if (!blockStickHMax && Input.GetAxis("Horizontal") >= stickThreshold) {
			blockStickHMax = true;
			if (lightningArtist.layerList[lightningArtist.currentLayer].frameList.Count > 0)
				lightningArtist.inputShowFrames();
		} else if (blockStickHMax && Input.GetAxis("Horizontal") < stickThreshold) {
			blockStickHMax = false;
		}

		if (!blockStickHMin && Input.GetAxis("Horizontal") <= -stickThreshold) {
			blockStickHMin = true;
			if (lightningArtist.layerList[lightningArtist.currentLayer].frameList.Count > 0)
				lightningArtist.inputHideFrames();
		} else if (blockStickHMin && Input.GetAxis("Horizontal") > -stickThreshold) {
			blockStickHMin = false;
		}
	}

}
