using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO refactor all text display stuff from LightningArtist core to here.

public class LatkText : MonoBehaviour {

	public TextMesh textMesh;

	private Renderer textMeshRen;

	void Start() {
		if (textMesh != null) textMeshRen = textMesh.gameObject.GetComponent<Renderer>();
	}
	
	public void setText(string s) {
		if (textMesh != null && textMeshRen != null) {
			textMesh.text = s;
		}
	}

	public void showText(bool b) {
		if (textMesh != null && textMeshRen != null) {
			textMeshRen.enabled = b;
		}
	}

}
