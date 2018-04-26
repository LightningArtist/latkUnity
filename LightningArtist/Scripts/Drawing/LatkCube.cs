using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkCube : LatkDrawing {

    public float size = 1f;

    private void Start() {
        if (createOnStart) {
            makeCube(transform.position, size);
            recordToLatk(strokes);
        }
    }

    public List<LatkStroke> makeCube(Vector3 pos, float size) {
        float s = size / 2f;

        Vector3 p1 = new Vector3(-s, -s, s) + pos;
        Vector3 p2 = new Vector3(-s, s, s) + pos;
        Vector3 p3 = new Vector3(s, -s, s) + pos;
        Vector3 p4 = new Vector3(s, s, s) + pos;
        Vector3 p5 = new Vector3(-s, -s, -s) + pos;
        Vector3 p6 = new Vector3(-s, s, -s) + pos;
        Vector3 p7 = new Vector3(s, -s, -s) + pos;
        Vector3 p8 = new Vector3(s, s, -s) + pos;

        strokes.Add(makeLine(p1, p2));
        strokes.Add(makeLine(p2, p4));
        strokes.Add(makeLine(p3, p1));
        strokes.Add(makeLine(p4, p3));

        strokes.Add(makeLine(p5, p6));
        strokes.Add(makeLine(p6, p8));
        strokes.Add(makeLine(p7, p5));
        strokes.Add(makeLine(p8, p7));

        strokes.Add(makeLine(p1, p5));
        strokes.Add(makeLine(p2, p6));
        strokes.Add(makeLine(p3, p7));
        strokes.Add(makeLine(p4, p8));

        return strokes;
    }

}
