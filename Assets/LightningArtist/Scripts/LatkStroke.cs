using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LatkStroke : MonoBehaviour {

    public float brushSize = 0.008f;
	public Color brushColor = new Color(0.5f, 0.5f, 0.5f);
	public Color brushEndColor = new Color(0.5f, 0.5f, 0.5f);
	public float brushBrightness = 1f;
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
    private MeshFilter mf;
    private Mesh mesh;
    private MeshRenderer meshRen;

    private void Awake() {
        lineRen = GetComponent<LineRenderer>();
        colorID = Shader.PropertyToID("_Color");
        block = new MaterialPropertyBlock();
        try {
            mf = GetComponent<MeshFilter>();
            mesh = new Mesh();
            meshRen = GetComponent<MeshRenderer>();
        } catch (UnityException e) { }
    }

    void Start() {
		lineRen.enabled = false;
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
            lineRen.enabled = points.Count > 1;
            lineRen.positionCount = points.Count;
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
		if (lineRen) {
            block.SetColor(colorID, changeBrightness(brushColor, brushBrightness));
            lineRen.SetPropertyBlock(block);
            if (meshRen) meshRen.SetPropertyBlock(block);
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

    public void fillMesh() {
        StartCoroutine(generateFill());
    }

    private IEnumerator generateFill() {
        //List<Vector3> verticesList = new List<Vector3>(points);
        //verticesList.Add(points[0]);
        Vector3[] vertices = points.ToArray();// verticesList.ToArray();

        //Vector2[] uvs = new Vector2[vertices.Length];
        //for (int i=0; i<uvs.Length; i++) {
            //uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
        //}

        Triangulator tr = new Triangulator(vertices);
        int[] triangles = tr.Triangulate();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //mesh.uv = uvs;
        mf.mesh = mesh;

        yield return null;
    }


}
