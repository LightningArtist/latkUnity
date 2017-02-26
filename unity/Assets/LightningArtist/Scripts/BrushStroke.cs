using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrushStroke : MonoBehaviour {

	public enum BrushMode { ADD, ALPHA };
	public BrushMode brushMode = BrushMode.ADD;
	public Material[] mat;
	public float brushSize = 0.008f;
	public Color brushColor = new Color(0.5f, 0.5f, 0.5f);
	public float brushBrightness = 1f;
	public Vector3 globalScale = new Vector3 (1f, 1f, 1f);
	public Vector3 globalOffset = Vector3.zero;
	public bool useScaleAndOffset = false;

	[HideInInspector] public bool isDirty = false;
	[HideInInspector] public LineRenderer lineRenderer;
	[HideInInspector] public List<Vector3> points;

	void Start() {
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.enabled = false;
		lineRenderer.sharedMaterial = mat[(int) brushMode];
	}

	void Update() {
		//if (isDirty) 
		refresh();
	}

	public void refresh() {
		if (lineRenderer != null) {
			setBrushSize();
			setBrushColor();
		}

		if (points != null) {
			if (points.Count > 1) lineRenderer.enabled = true;

			lineRenderer.SetVertexCount(points.Count);
            //for (int i=0; i<points.Count; i++) {
            //lineRenderer.SetPosition(i, points[i]);
            //}
            lineRenderer.SetPositions(points.ToArray());
		}	

		isDirty = false;
	}

	public void setBrushSize() {
		//brushSize = _f;
		lineRenderer.SetWidth(brushSize, brushSize);
	}

	public void setBrushSize(float f) {
		brushSize = f;
		lineRenderer.SetWidth(brushSize, brushSize);
	}

	public void setBrushColor() {
		brushMaterialColorChanger();
	}

	public void setBrushColor(Color c) {
		brushColor = c;
		brushMaterialColorChanger();
	}

	public void setBrushBrightness(float f) {
		brushBrightness = f;
		brushMaterialColorChanger();
	}

	public void brushMaterialColorChanger() {
		string colorString = "";

		if (brushMode == BrushMode.ADD) {
			colorString = "_TintColor";
		} else if (brushMode == BrushMode.ALPHA) {
			colorString = "_Color";
		}

		if (lineRenderer) lineRenderer.material.SetColor(colorString, changeBrightness(brushColor, brushBrightness));		
	}

	Color changeBrightness(Color c, float f) {
		Color returns = c;
		returns.r *= f;
		returns.g *= f;
		returns.b *= f;
		return returns;
	}

}
