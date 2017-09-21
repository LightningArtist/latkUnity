using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
*** Keys ***
F: New Frame
G: Copy Frame
P: Play/Stop
,: Frame Back
.: Frame Forward
T: Show/Hide Onion Skin
Z: Clear Layer
X: Clear Frame
I: Read File
O: Write File
L: New Layer
K: Cycle Layers

*** Reserved ***
WASD
Arrow Keys
3

// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
*/

public class BrushInputKeys : MonoBehaviour {

	public LightningArtist lightningArtist;

	void Awake() {
		if (lightningArtist == null) lightningArtist = GetComponent<LightningArtist>();
	}

	void Update() {
		// new frame
		if (Input.GetKeyDown(KeyCode.F)) {
			lightningArtist.inputNewFrame();
			Debug.Log("Ctl: New Frame");
		} else if (Input.GetKeyDown(KeyCode.G)) {
			lightningArtist.inputNewFrameAndCopy();
			Debug.Log("Ctl: New Frame Copy");
		}

		// play
		if (Input.GetKeyDown(KeyCode.P)) {
			lightningArtist.inputPlay();
			Debug.Log("Ctl: Play");
		}

		// ~ ~ ~ ~ ~ ~ ~ ~ ~

		// frame back
		if (Input.GetKeyDown(KeyCode.Comma)) {
			lightningArtist.inputFrameBack();
		}

		// frame forward
		if (Input.GetKeyDown(KeyCode.Period)) {
			lightningArtist.inputFrameForward();
		}

		// show / hide all frames
		if (Input.GetKeyDown(KeyCode.T)) {
			lightningArtist.showOnionSkin = !lightningArtist.showOnionSkin;
			if (lightningArtist.showOnionSkin) {
				lightningArtist.inputShowFrames();
			} else {
				lightningArtist.inputHideFrames();
			}
		}

		// ~ ~ ~ ~ ~ ~ ~ ~ ~

		if (Input.GetKeyDown(KeyCode.Z)) { // reset all
			lightningArtist.resetAll(); 
		}

		if (Input.GetKeyDown(KeyCode.X)) { // reset
			lightningArtist.layerList[lightningArtist.currentLayer].frameList[lightningArtist.layerList[lightningArtist.currentLayer].currentFrame].reset(); 
		}

		/*
		if (Input.GetKeyDown(KeyCode.T)) { // random
			//resetAll();
			lightningArtist.testRandomStrokes();
		}
		*/

		/*
	    if (Input.GetKeyDown(KeyCode.O)) { // scale
			lightningArtist.applyScaleAndOffset();
		}
		*/

		if (Input.GetKeyDown(KeyCode.I) && !lightningArtist.isReadingFile) {
			lightningArtist.armReadFile = true;
		}

		if (Input.GetKeyDown(KeyCode.O) && !lightningArtist.isWritingFile) {
			lightningArtist.armWriteFile = true;
		}

		if (Input.GetKeyDown(KeyCode.K)) {
			lightningArtist.inputNextLayer();
		}

		if (Input.GetKeyDown(KeyCode.L)) {
			lightningArtist.inputNewLayer();
		}
	}

}
