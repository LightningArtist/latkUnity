using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrushFrame : MonoBehaviour {

	[HideInInspector] public List<BrushStroke> brushStrokeList;
	[HideInInspector] public bool isDuplicate = false;
	[HideInInspector] public bool isDirty = false;

	void Update() {
		if (isDirty) refresh();
	}

	public void showFrame(bool _b) {
		for (int i = 0; i < brushStrokeList.Count; i++) {
			brushStrokeList[i].gameObject.SetActive(_b);
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

    public void setFrameBrightness(float _f) {
        for (int i = 0; i < brushStrokeList.Count; i++) {
            brushStrokeList[i].setBrushBrightness(_f);
        }
    }

}
