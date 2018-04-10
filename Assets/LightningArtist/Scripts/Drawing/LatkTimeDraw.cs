using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON; 

public class LatkTimeDraw : LatkDrawing {

	public enum DrawMode { POINTS, LIVE };
	public DrawMode drawMode = DrawMode.POINTS;
    public enum DrawState { WAIT, DRAW, ERASE };
    public DrawState drawState = DrawState.WAIT;
	public string readFileName = "current.json";
	public float liveDrawSpeed = 1f;
	public int liveDrawFrames = 12;
	public float fps = 12f;
	public int pointStep = 1;
    //public float globalScale = 1f;

    private int strokeCounter = 0;
    private int pointCounter = 0;
	private float consoleUpdateInterval = 0f;
	private JSONNode jsonNode;
	private List<StrokeData> strokesRaw = new List<StrokeData>();
	private bool ready = false;
	private float interval = 0f;
	private bool firstRun = true;

	void Start() {
        StartCoroutine(readLatkStrokes());
        if (createOnStart) drawState = DrawState.DRAW;
	}

	void Update() {
        if (firstRun) {
            if (latk && drawMode == DrawMode.LIVE) {
                for (int i = 0; i < liveDrawFrames; i++) {
                    latk.inputNewFrame();
                }
                latk.inputPlay();
            }
            firstRun = false;
        }

        if (ready) {
            if (drawState == DrawState.DRAW) {
                if (drawMode == DrawMode.POINTS) {
                    if (strokes.Count < 1) makeTimeStroke();
                    doTimeDraw();
                } else if (drawMode == DrawMode.LIVE) {
                    doTimeDrawLive();
                }
            } else if (drawState == DrawState.ERASE) {
                doTimeErase();
            }
		}
	}

	public void makeTimeStroke() {
		LatkStroke b = makeEmpty();
        if (brushMat[(int) brushMode]) b.mat = brushMat[(int) brushMode];
        b.setBrushColor(strokesRaw[strokeCounter].color);
		b.setBrushSize(brushSize);
		strokes.Add(b);
	}

    public void doTimeErase() {
        if (strokes.Count > 0) {

        	interval += Time.deltaTime;
			if (interval > 1f / fps) {
				interval = 0f;
	            int last = strokes.Count - 1;
	            if (strokes[last].points.Count <= pointStep) {
	                Destroy(strokes[last].gameObject);
	                strokes.RemoveAt(last);
	            } else {
	                for (int i=0; i<pointStep; i++) {
	                    strokes[last].points.RemoveAt(strokes[last].points.Count - 1);
	                }
	                strokes[last].isDirty = true;
	            }
	        }
        }
    }

	public void doTimeDraw() {
		if (strokesRaw.Count > 0 && strokesRaw[0].points.Count > 1) {

			interval += Time.deltaTime;
			if (interval > 1f / fps) {
				interval = 0f;

				for (int i=0; i<pointStep; i++) {
					strokes[strokes.Count-1].addPoint(strokesRaw[strokeCounter].points[pointCounter]);
					if (pointCounter < strokesRaw[strokeCounter].points.Count - 1) {
						pointCounter++;
					} else {
						if (strokeCounter < strokesRaw.Count - 1) {
							strokeCounter++;
							makeTimeStroke();
							pointCounter = 0;
						}
						break;
					}
				}
			}
		}
	}

	public void doTimeDrawLive() {
		if (strokesRaw.Count > 0 && strokesRaw[0].points.Count > 1) {
			latk.clicked = true; 
			latk.mainColor = strokesRaw[strokeCounter].color;

			interval += Time.deltaTime;
			if (interval > 1f / fps) {
				interval = 0f;

				latk.target.position = Vector3.Lerp(latk.target.position, strokesRaw[strokeCounter].points[pointCounter], liveDrawSpeed);

				if (pointCounter < strokesRaw[strokeCounter].points.Count - 1) {
					pointCounter++;
				} else {
					if (strokeCounter < strokesRaw.Count - 1) {
						latk.clicked = false;
						strokeCounter++;
						pointCounter = 0;
					} else {
						latk.clicked = false;
					}
				}
			}
		}
	}
		
	public IEnumerator readLatkStrokes() {
		string url;
		
		#if UNITY_ANDROID
		url = Path.Combine("jar:file://" + Application.dataPath + "!/assets/", readFileName);
		#endif

		#if UNITY_IOS
		url = Path.Combine("file://" + Application.dataPath + "/Raw", readFileName);
		#endif

		#if UNITY_EDITOR
		url = Path.Combine("file://" + Application.dataPath, readFileName);		
		#endif 

		#if UNITY_STANDALONE_WIN
		url = Path.Combine("file://" + Application.dataPath, readFileName);		
		#endif 

		#if UNITY_STANDALONE_OSX
		url = Path.Combine("file://" + Application.dataPath, readFileName);		
		#endif 

		WWW www = new WWW(url);
		yield return www;

		Debug.Log ("+++ File reading finished. Begin parsing...");
		yield return new WaitForSeconds (consoleUpdateInterval);

		jsonNode = JSON.Parse(www.text);

		for (int f = 0; f < jsonNode["grease_pencil"][0]["layers"].Count; f++) {
			
			for (int h = 0; h < jsonNode["grease_pencil"][0]["layers"][f]["frames"].Count; h++) {

				for (int i = 0; i < jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"].Count; i++) {
					StrokeData s = new StrokeData();

					float r = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][0].AsFloat;
					float g = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][1].AsFloat;
					float b = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][2].AsFloat;
					Color c = new Color (r, g, b);
					s.color = c;

					for (int j = 0; j < jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"].Count; j++) {
						float x = 0f;
						float y = 0f;
						float z = 0f;

						x = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][0].AsFloat;
						y = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][1].AsFloat;
						z = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][2].AsFloat;

                        //Vector3 p = new Vector3(x, y, z) * globalScale;
                        Vector3 p = applyTransformMatrix(new Vector3(x, y, z));

                        s.points.Add(p);	
					}

					strokesRaw.Add(s);
				}
				yield return new WaitForSeconds (consoleUpdateInterval);
			}
		}
		ready = true;
	}

}

public class StrokeData {

	public List<Vector3> points = new List<Vector3>();

	public Color color = new Color();

	/*
	public List<Vector3> getPoints() {
		List<Vector3> returns = new List<Vector3>();
		for (int i = 0; i < points.Count; i++) {
			returns.Add(points[i]);
		}
		return returns;
	}
	*/

}

