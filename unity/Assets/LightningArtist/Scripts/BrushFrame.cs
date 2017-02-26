using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrushFrame : MonoBehaviour {

	[HideInInspector] public List<BrushStroke> brushStrokeList;
	[HideInInspector] public bool isDuplicate = false;
	[HideInInspector] public bool isDirty = false;

	/*
	[HideInInspector] public List<LineRenderer> lineRendererList;

	void Update() {
		if (brushStrokeList.Count != lineRendererList.Count) {
			getLineRenderers();
		}
	}

	public void getLineRenderers() {
		lineRendererList = new List<LineRenderer>();

		for (int i = 0; i < brushStrokeList.Count; i++) {
			LineRenderer lren = brushStrokeList[i].GetComponent<LineRenderer>();
			lineRendererList.Add(lren);
		}
	}

	*/

	void Update() {
		if (isDirty) refresh();
	}

	public void showFrame(bool _b) {
		//for (int i = 0; i < lineRendererList.Count; i++) {
			//lineRendererList[i].enabled = _b;
		//}
		for (int i = 0; i < brushStrokeList.Count; i++) {
			brushStrokeList[i].gameObject.SetActive(_b);
			//brushStrokeList[i].lineRenderer.enabled = _b;
			brushStrokeList[i].isDirty = isDirty;
		}

		isDirty = false;
	}

	public void refresh() {
		if (brushStrokeList != null) {
			for (int i=0; i<brushStrokeList.Count; i++) {
				brushStrokeList[i].isDirty = true;
			}
		}

		isDirty = false;
	}

	public void reset() {
		if (brushStrokeList != null) {
			for (int i=0; i<brushStrokeList.Count; i++) {
				Destroy(brushStrokeList[i].gameObject);
			}
			brushStrokeList = new List<BrushStroke>();
		}
	}

}
