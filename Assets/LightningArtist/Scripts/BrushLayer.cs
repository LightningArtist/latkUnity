using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushLayer : MonoBehaviour {

	public string name = "";
	[HideInInspector] public List<BrushFrame> frameList;
	[HideInInspector] public int currentFrame = 0;
	[HideInInspector] public int previousFrame = 0;

	public void reset() {
		if (frameList != null) {
			for (int i=0; i<frameList.Count; i++) {
				Destroy(frameList[i].gameObject);
			}
			frameList = new List<BrushFrame>();
		}
	}

}
