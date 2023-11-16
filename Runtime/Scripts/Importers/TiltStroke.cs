using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltStroke : MonoBehaviour {

    private LineRenderer ren;

	private void Awake() {
        ren = GetComponent<LineRenderer>();
	}

	public void init(List<Vector3> _positions, float _brushSize, Color _brushColor) {
        ren.positionCount = _positions.Count;
        ren.SetPositions(_positions.ToArray());
        ren.startWidth = ren.endWidth = _brushSize;
        ren.startColor = ren.endColor = _brushColor;
    }

}
