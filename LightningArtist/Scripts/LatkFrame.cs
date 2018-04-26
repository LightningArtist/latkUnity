using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LatkFrame : MonoBehaviour {

    public Vector3 parentPos = Vector3.zero;
    public LatkLayer parentLayer;

    [HideInInspector] public List<LatkStroke> brushStrokeList;
	[HideInInspector] public bool isDuplicate = false;
	[HideInInspector] public bool isDirty = false;

    private void Awake() {
        //parentLayer = transform.parent.GetComponent<LatkLayer>();
    }

    private void Update() {
		if (isDirty) refresh();
	}

	public void showFrame(bool _b) {
		for (int i = 0; i < brushStrokeList.Count; i++) {
			brushStrokeList[i].gameObject.SetActive(_b);
			brushStrokeList[i].isDirty = isDirty;
		}

        //parentLayer.parentPos = new Vector3(parentPos.x, parentPos.y, parentPos.z);

        isDirty = false;
	}

	public void refresh() {
		if (brushStrokeList != null) {
			for (int i=0; i<brushStrokeList.Count; i++) {
				brushStrokeList[i].isDirty = true;
			}
		}

        //parentLayer.parentPos = new Vector3(parentPos.x, parentPos.y, parentPos.z);

        isDirty = false;
	}

	public void reset() {
		if (brushStrokeList != null) {
			for (int i=0; i<brushStrokeList.Count; i++) {
				Destroy(brushStrokeList[i].gameObject);
			}
			brushStrokeList = new List<LatkStroke>();
		}

        //parentPos = Vector3.zero;
	}

    public void setFrameBrightness(float _f) {
        for (int i = 0; i < brushStrokeList.Count; i++) {
            brushStrokeList[i].setBrushBrightness(_f);
        }
    }

}
