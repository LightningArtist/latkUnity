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

public class LatkInputKeys : MonoBehaviour {

	public LightningArtist latk;

	void Awake() {
		if (latk == null) latk = GetComponent<LightningArtist>();
	}

	void Update() {
		// new frame
		if (Input.GetKeyDown(KeyCode.F)) {
			latk.inputNewFrame();
			Debug.Log("Ctl: New Frame");
		} else if (Input.GetKeyDown(KeyCode.G)) {
			latk.inputNewFrameAndCopy();
			Debug.Log("Ctl: New Frame Copy");
		}

		// play
		if (Input.GetKeyDown(KeyCode.P)) {
			latk.inputPlay();
			Debug.Log("Ctl: Play");
		}

		// ~ ~ ~ ~ ~ ~ ~ ~ ~

		// frame back
		if (Input.GetKeyDown(KeyCode.Comma)) {
			latk.inputFrameBack();
		}

		// frame forward
		if (Input.GetKeyDown(KeyCode.Period)) {
			latk.inputFrameForward();
		}

		// show / hide all frames
		if (Input.GetKeyDown(KeyCode.T)) {
			latk.showOnionSkin = !latk.showOnionSkin;
			if (latk.showOnionSkin) {
				latk.inputShowFrames();
			} else {
				latk.inputHideFrames();
			}
		}

		// ~ ~ ~ ~ ~ ~ ~ ~ ~

		if (Input.GetKeyDown(KeyCode.Z)) { // reset all
			latk.resetAll(); 
		}

		if (Input.GetKeyDown(KeyCode.X)) { // reset
			latk.layerList[latk.currentLayer].frameList[latk.layerList[latk.currentLayer].currentFrame].reset(); 
		}

		/*
		if (Input.GetKeyDown(KeyCode.T)) { // random
			//resetAll();
			latk.testRandomStrokes();
		}
		*/

		/*
	    if (Input.GetKeyDown(KeyCode.O)) { // scale
			latk.applyScaleAndOffset();
		}
		*/

		if (Input.GetKeyDown(KeyCode.I) && !latk.isReadingFile) {
			latk.armReadFile = true;
		}

		if (Input.GetKeyDown(KeyCode.O) && !latk.isWritingFile) {
			latk.armWriteFile = true;
		}

		if (Input.GetKeyDown(KeyCode.K)) {
			latk.inputNextLayer();
		}

		if (Input.GetKeyDown(KeyCode.L)) {
			latk.inputNewLayer();
		}
	}

}
