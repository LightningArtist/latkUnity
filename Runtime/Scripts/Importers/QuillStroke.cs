using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuillStroke : MonoBehaviour {

    private LineRenderer ren;

	private void Awake() {
        ren = GetComponent<LineRenderer>();
	}

	public void init(List<Vector3> _positions, List<Color> _colors, List<float> _widths) {
        // Every Quill vertex stores color and pressure, but as a shortcut you can take the middle color and width value for the stroke
        float brushSize = _widths[(int) (_widths.Count / 2)];
        Color brushColor = _colors[(int) (_colors.Count / 2)];

        
        ren.positionCount = _positions.Count;
        ren.SetPositions(_positions.ToArray());
        ren.startWidth = ren.endWidth = brushSize;
        ren.startColor = ren.endColor = brushColor;
    }

}
