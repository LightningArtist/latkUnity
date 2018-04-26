using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkLayer : MonoBehaviour {

	public string name = "";
    public Vector3 parentPos = Vector3.zero;

    [HideInInspector] public List<LatkFrame> frameList;
	[HideInInspector] public int currentFrame = 0;
	[HideInInspector] public int previousFrame = 0;
    [HideInInspector] public float lerpSpeed = 0.5f;

	public void reset() {
		if (frameList != null) {
			for (int i=0; i<frameList.Count; i++) {
				Destroy(frameList[i].gameObject);
			}
			frameList = new List<LatkFrame>();
		}
	}

    //private void Update() {
        //transform.position = Vector3.Lerp(transform.position, parentPos, lerpSpeed);
    //}

}
