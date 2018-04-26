using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkInputButtons : MonoBehaviour {

	public LightningArtist latk;
	public bool showButtons = true;

	[HideInInspector] public float LABEL_START_X = 15.0f;
	[HideInInspector] public float LABEL_START_Y = 15.0f;
	[HideInInspector] public float LABEL_SIZE_X = Screen.width;//1920.0f;
	[HideInInspector] public float LABEL_SIZE_Y = 35.0f;
	[HideInInspector] public float LABEL_GAP_Y = 3.0f;
	[HideInInspector] public float BUTTON_SIZE_X = 200f; //250.0f;
	[HideInInspector] public float BUTTON_SIZE_Y = 90f; //130.0f;
	[HideInInspector] public float BUTTON_GAP_X = 5.0f;
	[HideInInspector] public string FLOAT_FORMAT = "F3";
	[HideInInspector] public string FONT_SIZE = "<size=25>";

	private int menuCounter = 1;
	private int menuCounterMax = 1;
	//private bool playButtonBlock = false;

	void Awake() {
		if (latk == null) latk = GetComponent<LightningArtist>();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) showButtons = !showButtons;
	}

	void OnGUI() {
		if (showButtons) {
			string isOn = "";

			if (menuCounter == 1) {
				// 1-1.
				Rect writeButton = new Rect(BUTTON_GAP_X, Screen.height - (1 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
				//isOn = latk.showOnionSkin ? "Off" : "On";
				if (GUI.Button(writeButton, FONT_SIZE + "Write" + "</size>")) {
					latk.armWriteFile = true;
				}

				// 1-2.
				Rect undoButton = new Rect(BUTTON_GAP_X, Screen.height - (2 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
				//isOn = latk.showOnionSkin ? "Off" : "On";
				if (GUI.Button(undoButton, FONT_SIZE + "Undo" + "</size>")) {
					latk.inputEraseLastStroke();
				}

				// 1-3.
				Rect onionButton = new Rect(BUTTON_GAP_X, Screen.height - (3 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
				isOn = latk.showOnionSkin ? "Off" : "On";
				if (GUI.Button(onionButton, FONT_SIZE + "Onion Skin " + isOn + "</size>")) {
					latk.inputOnionSkin();
				}

				// 1-4.
				Rect copyFrameButton = new Rect(BUTTON_GAP_X, Screen.height - (4 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
				//isOn = m_arCameraPostProcess.enabled ? "Off" : "On";
				if (GUI.Button(copyFrameButton, FONT_SIZE + "Copy Frame" + "</size>")) {
					latk.inputNewFrameAndCopy();
				}

				// 1-5.
				Rect newFrameButton = new Rect(BUTTON_GAP_X, Screen.height - (5 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
				//isOn = m_arCameraPostProcess.enabled ? "Off" : "On";
				if (GUI.Button(newFrameButton, FONT_SIZE + "New Frame" + "</size>")) {
					latk.inputNewFrame();
				}

				// 1-6.
				Rect playButton = new Rect(BUTTON_GAP_X, Screen.height - (6 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
				isOn = latk.isPlaying ? "Stop" : "Play";
				if (GUI.Button(playButton, FONT_SIZE + isOn + "</size>")) {
					//if (!latk.isPlaying && !playButtonBlock) {
						latk.inputPlay();
						//playButtonBlock = true;
					//} else {
						//latk.inputFrameBack(); // this is simpler than solving with script execution order
						//playButtonBlock = false;
					//}
				}

				// 1-7.
				Rect rewButton = new Rect(BUTTON_GAP_X, Screen.height - (7 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X/2f, BUTTON_SIZE_Y);
				//isOn = m_arCameraPostProcess.enabled ? "Off" : "On";
				if (GUI.Button(rewButton, FONT_SIZE + "<|" + "</size>")) {
					latk.inputFrameBack();
				}

				Rect ffButton = new Rect(BUTTON_GAP_X + (BUTTON_SIZE_X/2f), Screen.height - (7 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X/2f, BUTTON_SIZE_Y);
				//isOn = m_arCameraPostProcess.enabled ? "Off" : "On";
				if (GUI.Button(ffButton, FONT_SIZE + "|>" + "</size>")) {
					latk.inputFrameForward();
				}
			} else if (menuCounter == 2) {
				// 2-7.
				Rect readButton = new Rect(BUTTON_GAP_X, Screen.height - (7 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
				//isOn = latk.showOnionSkin ? "Off" : "On";
				if (GUI.Button(readButton, FONT_SIZE + "Read" + "</size>")) {
					latk.armReadFile = true;
				}
			}

			// 8.
			Rect menuButton = new Rect(BUTTON_GAP_X, Screen.height - (8 * (BUTTON_SIZE_Y - BUTTON_GAP_X)), BUTTON_SIZE_X, BUTTON_SIZE_Y);
			//isOn = m_arCameraPostProcess.enabled ? "Off" : "On";
			if (GUI.Button(menuButton, FONT_SIZE + "MENU " + menuCounter + "</size>")) {
				menuCounter++;
				if (menuCounter > menuCounterMax) menuCounter = 1;
			}
		}
	}

}
