using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkDrawing : MonoBehaviour {

    public LightningArtist latk;
    public LatkStroke brushPrefab;
    public enum BrushMode { ADD, SURFACE, UNLIT };
    public BrushMode brushMode = BrushMode.ADD;
    public Material[] brushMat;
    public Color color = new Color(1f, 0f, 0f);
    public float brushSize = 0.05f;
    public float minDistance = 0f;
    public enum LatkSettings { COPY_TO_LATK, COPY_FROM_LATK, IGNORE };
    public LatkSettings latkSettings = LatkSettings.IGNORE;
    public bool createOnStart = true;
    public List<LatkStroke> strokes = new List<LatkStroke>();
    public Matrix4x4 transformMatrix;
    public bool checkSelfDestruct = false;

    private void Awake() {
        if (!latk) latk = GetComponent<LightningArtist>();
        if (latk) {
            if (latkSettings == LatkSettings.COPY_TO_LATK) {
                latk.brushPrefab = brushPrefab;
                latk.mainColor = color;
                latk.brushSize = brushSize;
                latk.minDistance = minDistance;
            } else if (latkSettings== LatkSettings.COPY_FROM_LATK) {
                brushPrefab = latk.brushPrefab;
                color = latk.mainColor;
                brushSize = latk.brushSize;
                minDistance = latk.minDistance;
            }
        }

        updateTransformMatrix();
    }

    private void Update() {
        if (checkSelfDestruct) {
            for (int i = 0; i < strokes.Count; i++) {
                if (strokes[i].selfDestruct && Time.realtimeSinceStartup > strokes[i].birthTime + strokes[i].lifeTime) {
                    Destroy(strokes[i].gameObject);
                    strokes.Remove(strokes[i]);
                }
            }
        }
    }

    public void updateTransformMatrix() {
        transformMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
    }

    public Vector3 applyTransformMatrix(Vector3 p) {
        return transformMatrix.MultiplyPoint3x4(p);
    }

    public List<Vector3> filterMinDistance(List<Vector3> points) {
        List<Vector3> returns = new List<Vector3>();
        //returns.Add(points[0]);
        int lastPoint = 0;
        for (int i = 1; i < points.Count; i++) {
            if (Vector3.Distance(points[i], points[lastPoint]) >= minDistance) {
                returns.Add(points[i]);
                lastPoint = i;
            }
        }
        return returns;
    }

    public void recordToLatk(List<LatkStroke> _strokes) {
        for (int i = 0; i < _strokes.Count; i++) {
            latk.brushSize = brushSize;
            latk.inputInstantiateStroke(_strokes[i].brushColor, _strokes[i].points);
        }
    }

    public void recordToLatk() {
        for (int i = 0; i < strokes.Count; i++) {
            latk.brushSize = brushSize;
            latk.inputInstantiateStroke(strokes[i].brushColor, strokes[i].points);
        }
    }

    public void recordToLatk(int index) {
        latk.brushSize = brushSize;
        latk.inputInstantiateStroke(strokes[index].brushColor, strokes[index].points);
    }

    // ~ ~ ~ ~ ~
    // I. EMPTY

    public LatkStroke makeEmptyCore() {
        LatkStroke brush = Instantiate(brushPrefab);
        if (brushMat[(int)brushMode]) brush.mat = brushMat[(int)brushMode];
        brush.transform.SetParent(transform);
        brush.brushColor = color;
        brush.brushSize = brushSize;
        return brush;
    }

    public LatkStroke makeEmpty() {
        LatkStroke brush = makeEmptyCore();

        strokes.Add(brush);
        return brush;
	}

    // ~ ~ ~ ~ ~
    // II. LINE

    public LatkStroke makeLineCore(Vector3 v1, Vector3 v2) {
        LatkStroke brush = makeEmptyCore();
        brush.points.Add(v1);
        brush.points.Add(v2);
        return brush;
    }

    public LatkStroke makeLine(Vector3 v1, Vector3 v2) {
        LatkStroke brush = makeLineCore(v1, v2);

        strokes.Add(brush);
        return brush;
	}

    public LatkStroke makeLine(Vector3 v1, Vector3 v2, bool selfDestruct, float lifeTime) {
        LatkStroke brush = makeLineCore(v1, v2);
        brush.selfDestruct = selfDestruct;
        brush.lifeTime = lifeTime;

        strokes.Add(brush);
        return brush;
    }

    public LatkStroke makeLine(List<Vector3> points) {
		LatkStroke brush = makeLineCore(points[0], points[points.Count-1]);

        strokes.Add(brush);
        return brush;
    }

    // ~ ~ ~ ~ ~
    // III. CURVE

    public LatkStroke makeCurveCore(List<Vector3> points) {
        LatkStroke brush = makeEmptyCore();
        if (minDistance > 0f) points = filterMinDistance(points);
        brush.points = points;
        return brush;
    }

    public LatkStroke makeCurve(List<Vector3> points, bool selfDestruct, float lifeTime) {
        LatkStroke brush = makeCurveCore(points);
        brush.selfDestruct = selfDestruct;
        brush.lifeTime = lifeTime;

        strokes.Add(brush);
        return brush;
    }

    public LatkStroke makeCurve(List<Vector3> points) {
        LatkStroke brush = makeCurveCore(points);

        strokes.Add(brush);
        return brush;
    }

    public LatkStroke makeCurve(List<Vector3> points, bool closed) { 
        if (closed && points.Count > 10) points.Add(points[1]);
        LatkStroke brush = makeCurveCore(points);


        strokes.Add(brush);
        return brush;
    }

}
