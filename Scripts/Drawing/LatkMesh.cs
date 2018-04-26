using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LatkMesh : LatkDrawing {

    public enum MeshMode { EDGE, FILL, EDGE_FILL}
    public MeshMode meshMode = MeshMode.EDGE_FILL;
    public bool alwaysUpdate = true;
    public GetVertices getVertices;
    public int fillLines = 100;
    public int smoothReps = 200;
    public float randomize = 0.1f;
    public float fps = 12f;
    
    private float interval = 0f;

    private void Awake() {
        if (!getVertices) getVertices = GetComponent<GetVertices>();
    }

    private void Start() {
        if (createOnStart) {
            doMesh();
        }
    }

    private void Update() {
        if (alwaysUpdate) {
            interval += Time.deltaTime;
            if (interval > 1f / fps) {
                interval = 0f;
                doMesh();
            }
        }
    }
    
    public void clearStrokes() {
        for (int i=0; i<strokes.Count; i++) {
            Destroy(strokes[i].gameObject);
            strokes.RemoveAt(i);
        }
    }

    public void doMesh() {
        clearStrokes();

        if (meshMode == MeshMode.EDGE) {
            makeMeshEdge();
        } else if (meshMode == MeshMode.FILL) {
            makeMeshFill();
        } else if (meshMode == MeshMode.EDGE_FILL) {
            makeMeshEdge();
            makeMeshFill();
        }
    }

    public void makeMeshEdge() {
        List<Vector3> points = getVertices.getSource();
        Debug.Log(points.Count);
        LatkStroke b = makeCurve(points);
        //b.refine();
        for (int i=0; i<smoothReps; i++) {
            //b.smoothStroke(b.points);
        }
        //b.reduceStroke(b.points);
        strokes.Add(b);
    }

    public void makeMeshFill() {
        List<Vector3> points = getVertices.getSource();
        Debug.Log(points.Count);
        for (int h = 0; h < fillLines; h++) {
            Vector3 p1 = points[(int) Random.Range(0f, points.Count)];
            Vector3 p2 = points[(int) Random.Range(0f, points.Count)];
            Vector3 p3 = (p1 + p2) / 2f;
            Vector3[] newLine = { p1, p2, p3 };
            LatkStroke b = makeLine(newLine.ToList());
            b.refine();
            b.randomize(randomize);
            b.refine();
            for (int i = 0; i < smoothReps/10; i++) {
                b.smoothStroke(b.points);
            }
            //b.reduceStroke(b.points);
            strokes.Add(b);
        }
    }

}
