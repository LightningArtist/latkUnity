using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkBone : LatkDrawing {

    public Transform[] joint;
    public float[] offset;
    public float fps = 12f;
    public int numStrokes = 10;
    public float spread = 0.5f;
    public bool overrideBrush = false;
    public float smear = 1f;

    private float interval = 0f;

    void Start() {
        if (!overrideBrush || offset.Length < joint.Length) {
            offset = new float[joint.Length];
            for (int i = 0; i < offset.Length; i++) {
                offset[i] = spread;
            }
        }

        for (int i = 0; i < numStrokes; i++) {
            strokes.Add(makeCurve(generatePoints()));
            for (int j = 0; j < 2; j++) {
                strokes[strokes.Count - 1].splitStroke(strokes[strokes.Count - 1].points);
                strokes[i].points = strokes[i].smoothStroke(strokes[i].points);
            }
        }
    }

    void Update() {
        interval += Time.deltaTime;
        if (interval > 1f / fps) {
            interval = 0f;

            for (int i = 0; i < strokes.Count; i++) {
                strokes[i].points = movePoints(ref strokes[i].points);
                for (int j = 0; j < 8; j++) {
                    strokes[i].points = strokes[i].smoothStroke(strokes[i].points);
                }
                strokes[i].isDirty = true;
            }
        }
 }

    public Vector3 getPosition(Vector3 pos, float offset) {
        float x = pos.x + Random.Range(-offset, offset);
        float y = pos.y + Random.Range(-offset, offset);
        float z = pos.z + Random.Range(-offset, offset);
        return new Vector3(x, y, z);
    }

    public List<Vector3> generatePoints() {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < joint.Length; i++) {
            points.Add(getPosition(joint[i].position, offset[i]));
        }
        return points;
    }

    public List<Vector3> movePoints(ref List<Vector3> points) {
        List<int> hits = new List<int>();
        for (int i = 0; i < points.Count; i++) {
            for (int j = 0; j < joint.Length; j++) {
                if (i == j) {
                    points[i] = getPosition(joint[j].position, offset[j]);
                    hits.Add(i);
                    break;
                }
            }
        }
        for (int i = 0; i < points.Count; i++) {
            bool doMove = true;
            for (int j=0; j<hits.Count; j++) {
                if (i==j) {
                    doMove = false;
                    break;
                }
            }
            if (doMove) {
                Vector3 lerpIn = points[i];
                Vector3 lerpOut = points[i];
                if (i > 0) lerpIn = points[i - 1];
                if (i < points.Count -1) lerpOut = points[i + 1];

                points[i] = Vector3.Lerp(lerpIn, lerpOut, Random.Range(0f, smear));
            }
        }
        return points;
    }

}
