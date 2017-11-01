using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;

public class BrushStroke : MonoBehaviour {

    //public enum BrushMode { ADD, ALPHA };
    //public BrushMode brushMode = BrushMode.ADD;
    //public Material[] mat;
    //public Material mat;
    public float brushSize = 0.008f;
	public Color brushColor = new Color(0.5f, 0.5f, 0.5f);
	public Color brushEndColor = new Color(0.5f, 0.5f, 0.5f);
	public float brushBrightness = 1f;
	//public Vector3 globalScale = new Vector3 (1f, 1f, 1f);
	//public Vector3 globalOffset = Vector3.zero;
	//public bool useScaleAndOffset = false;
	public float birthTime = 0f;
    public float lifeTime = 5f;
    public bool selfDestruct = false;

	public bool isDirty = true;
	[HideInInspector] public LineRenderer lineRen;
	[HideInInspector] public List<Vector3> points;
    [HideInInspector] public Material mat;

    private int colorID;
    private MaterialPropertyBlock block;
    private int splitReps = 2;
    private int smoothReps = 10;
    private int reduceReps = 0;

    private void Awake() {
        lineRen = GetComponent<LineRenderer>();
        colorID = Shader.PropertyToID("_Color");
        block = new MaterialPropertyBlock();
    }

    void Start() {
		lineRen.enabled = false;
        //lineRen.sharedMaterial = mat[(int)brushMode];
        if (mat) lineRen.sharedMaterial = mat;
        birthTime = Time.realtimeSinceStartup;
	}

	void Update() {
		if (isDirty) refresh();
	}

	public void refresh() {
		if (lineRen != null) {
			setBrushSize();
			setBrushColor();
		}

		if (points != null) {
            //if (points.Count > 1) lineRen.enabled = true;
            lineRen.enabled = points.Count > 1;

            //lineRen.SetVertexCount(points.Count);
            lineRen.positionCount = points.Count;
            //for (int i=0; i<points.Count; i++) {
            //lineRen.SetPosition(i, points[i]);
            //}
            lineRen.SetPositions(points.ToArray());
		}	

		isDirty = false;
	}

    public void setPoints(List<Vector3> p) {
        points = new List<Vector3>();
        for (int i=0; i<p.Count; i++) {
            points.Add(new Vector3(p[i].x, p[i].y, p[i].z));
        }
    }

	public void addPoint(Vector3 p) {
		points.Add(p);
		isDirty = true;
	}

	public void setBrushSize() {
        //brushSize = _f;
        lineRen.startWidth = brushSize;
        lineRen.endWidth = brushSize;
    }

	public void setBrushSize(float f) {
		brushSize = f;
        lineRen.startWidth = brushSize;
        lineRen.endWidth = brushSize;
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
		/*
        string colorString = "";

		if (brushMode == BrushMode.ADD) {
			colorString = "_TintColor";
		} else if (brushMode == BrushMode.ALPHA) {
			colorString = "_Color";
		}
        */

		if (lineRen) {
            //lineRen.sharedMaterial.SetColor(colorString, changeBrightness(brushColor, brushBrightness));
            //lineRen.material.SetColor(colorID, changeBrightness(brushColor, brushBrightness));
            block.SetColor(colorID, changeBrightness(brushColor, brushBrightness));
            lineRen.SetPropertyBlock(block);
            //lineRen.startColor = brushEndColor;
			//lineRen.endColor = brushColor;
		}
	}

	Color changeBrightness(Color c, float f) {
		Color returns = c;
		returns.r *= f;
		returns.g *= f;
		returns.b *= f;
		return returns;
	}

    // ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~

    public List<Vector3> smoothStroke(List<Vector3> pl) {
        float weight = 18f;
        float scale = 1f / (weight + 2f);
        Vector3 lower, upper, center;

		for (int i = 1; i < pl.Count - 2; i++) {
            lower = pl[i - 1];
            center = pl[i];
            upper = pl[i + 1];

            center.x = (lower.x + weight * center.x + upper.x) * scale;
            center.y = (lower.y + weight * center.y + upper.y) * scale;
            center.z = (lower.z + weight * center.z + upper.z) * scale;

            pl[i] = center;
        }
        return pl;
    }

    public List<Vector3> splitStroke(List<Vector3> pl) {
        for (int i = 1; i < pl.Count; i += 2) {
            Vector3 center = pl[i];
            Vector3 lower = pl[i - 1];
            float x = (center.x + lower.x) / 2f;
            float y = (center.y + lower.y) / 2f;
            float z = (center.z + lower.z) / 2f;
            Vector3 p = new Vector3(x, y, z);
            pl.Insert(i, p);
        }
        return pl;
    }

    public List<Vector3> reduceStroke(List<Vector3> pl) {
        for (int i = 1; i < pl.Count - 1; i += 2) {
            pl.RemoveAt(i);
        }
        return pl;
    }

    public void refine() {
        //Vector3[] pa = new Vector3[lineRen.positionCount];
        //lineRen.GetPositions(pa);
        //List<Vector3> pl = pa.ToList<Vector3>();

        for (int i = 0; i < splitReps; i++) {
            points = splitStroke(points);
            points = smoothStroke(points);
        }
        for (int i = 0; i < smoothReps - splitReps; i++) {
            points = smoothStroke(points);
        }
        for (int i = 0; i < reduceReps; i++) {
            points = reduceStroke(points);
        }

        isDirty = true;
    }

    public void randomize(float spread) {
        for (int i=0; i<points.Count; i++) {
            Vector3 r = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));
            points[i] += r;
        }
        isDirty = true;
    }

    public void moveStroke(Vector3 p) {
        for (int i=0; i<points.Count; i++) {
            points[i] += p;
        }
    }

}
