/*
LIGHTNING ARTIST TOOLKIT v1.0.0

The Lightning Artist Toolkit was developed with support from:
   Canada Council on the Arts
   Eyebeam Art + Technology Center
   Ontario Arts Council
   Toronto Arts Council
   
Copyright (c) 2017 Nick Fox-Gieg
http://fox-gieg.com

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# 
#     http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;  

public class LightningArtist : MonoBehaviour {

    public enum BrushMode { ADD, SURFACE, UNLIT };
    public BrushMode brushMode = BrushMode.ADD;
    public Material[] brushMat;
    public Transform target;
	public BrushLayer layerPrefab;
	public BrushFrame framePrefab;
    public BrushStroke brushPrefab;
    public TextMesh textMesh;
	public AudioSource audio;
	public Animator animator;
	public Renderer floorRen;
	public Color mainColor = new Color(0.5f, 0.5f, 0.5f);
	public Color endColor = new Color(0.5f, 0.5f, 0.5f);
	public bool useEndColor = false;
	public int drawTrailLength = 4;
	public float strokeLife = 5f;
	public bool killStrokes = false;

	public string readFileName = "brushstrokes-saved.json";
	public bool readOnStart = false;
	public bool playOnStart = false;
	public string writeFileName = "brushstrokes-saved.json";
	public bool useTimestamp = false;
	public bool createFrameWithLayer = false;
	public bool newLayerOnRead = false;
    public bool drawWhilePlaying = false;
    public bool refineStrokes = false;

    // NONE plays empty frames as empty. 
    // WRITE copies last good frame into empty frame (will save out). 
    // DISPLAY holds last good frame but doesn't copy (won't save out).
    public enum FillEmptyMethod { NONE, WRITE, DISPLAY }; 
	public FillEmptyMethod fillEmptyMethod = FillEmptyMethod.DISPLAY;

	public float minDistance = 0.0001f;
	public float brushSize = 0.008f;
	//public Vector3 globalScale = new Vector3 (1f, 1f, 1f);
	//public Vector3 globalOffset = Vector3.zero;
	//public bool useScaleAndOffsetRead = true;
	//public bool useScaleAndOffsetWrite = false;
	public float frameInterval = 12f;
	public float eraseRange = 0.05f;
	public float pushRange = 0.05f;
	public float colorPickRange = 0.05f;
	public float pushSpeed = 0.01f;
	public int onionSkinRange = 5;

	public Renderer[] animatorRen;

	[HideInInspector] public List<BrushLayer> layerList;
	[HideInInspector] public int currentLayer = 0;
	[HideInInspector] public bool isDrawing = false;
	[HideInInspector] public JSONNode jsonNode;
	[HideInInspector] public bool clicked = false;
	[HideInInspector] public bool isReadingFile = false;
	[HideInInspector] public bool isWritingFile = false;
	[HideInInspector] public bool isPlaying = false;
	[HideInInspector] public bool showOnionSkin = false;

	[HideInInspector] public bool armReadFile = false;
	[HideInInspector] public bool armWriteFile = false;

	private bool firstRun = true;
	private float lastFrameTime = 0f;
	private Renderer textMeshRen;
	private int rememberFrame = 0;
	private float markTime = 0f;
	public float frameBrightNormal = 0.5f;
	public float frameBrightDim = 0.05f;

	private float normalizedFrameInterval = 0f;
	private string clipName = "Take 001";
	private int clipLayer = 0;
	private float animVal = 0f;
	private Vector3 lastTargetPos = Vector3.zero;

	private float consoleUpdateInterval = 0f;
	private int debugTextCurrentFrame = 0;
	private int debugTextLastFrame = 0;
	private int longestLayer = 0;
    private float brushSizeDelta = 0f;

    private Matrix4x4 transformMatrix;

    public void updateTransformMatrix() {
        transformMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
    }

    public Vector3 applyTransformMatrix(Vector3 p) {
        return transformMatrix.MultiplyPoint3x4(p);
    }

    void Awake() {
        updateTransformMatrix();
		if (textMesh != null) textMeshRen = textMesh.gameObject.GetComponent<Renderer>();
	}

	void Start() {
        if (!target) target = transform;

		if (floorRen != null) floorRen.enabled = false;

		frameInterval = 1f / frameInterval;
		if (animator != null) normalizedFrameInterval = frameInterval / animator.GetCurrentAnimatorStateInfo(clipLayer).length;

		instantiateLayer();
		instantiateFrame();

		if (readOnStart) {
			armReadFile = true;
		}

        brushSizeDelta = brushSize / 100f;
	}

	void Update() {
		if (armReadFile) {
			if (textMesh != null) textMesh.text = "READING...";
			StartCoroutine(readBrushStrokes());
			armReadFile = false;
		} else if (armWriteFile) {
			if (textMesh != null) textMesh.text = "WRITING...";
			StartCoroutine(writeBrushStrokes());
			armWriteFile = false;
		} else if (!isReadingFile && !isWritingFile) {
			for (int i = 0; i < layerList.Count; i++) {
				if (layerList[i].frameList.Count > 0 && !layerList[i].frameList[layerList[i].currentFrame].isDuplicate) {
					layerList[i].previousFrame = layerList[i].currentFrame;
				}
			}

			if (isPlaying) {
				float t = 0f;
				if (audio != null) {
					t = markTime + audio.time;
				} else {
					t = Time.realtimeSinceStartup;
				}

				if ((audio != null && Time.realtimeSinceStartup > markTime + audio.clip.length) || (layerList[longestLayer].frameList.Count > 1 && t > lastFrameTime + frameInterval)) {
                    if (drawWhilePlaying) {
                        endStroke();
                        isDrawing = false;
                    }

                    lastFrameTime = t;

					animVal += normalizedFrameInterval;
					if (animVal > 1f) animVal = 1f;

					for (int i = 0; i < layerList.Count; i++) {
						// ~ ~ ~ ~ ~
						if (killStrokes) {
							if (layerList[i].frameList[layerList[i].currentFrame].brushStrokeList.Count > 0 && Time.realtimeSinceStartup > layerList[i].frameList[layerList[i].currentFrame].brushStrokeList[0].birthTime + strokeLife) {
								inputEraseFirstStroke(); 
							}
						}
						// ~ ~ ~ ~ ~
						layerList[i].frameList[layerList[i].currentFrame].showFrame (false);
						layerList[i].currentFrame++;

						if (layerList[i].currentFrame > layerList[i].frameList.Count - 1) {
							layerList[i].currentFrame = 0;

							if (i == longestLayer) {
								animVal = 0f;
								markTime = Time.realtimeSinceStartup;
								if (audio != null) {
									audio.time = 0f;
									audio.Stop ();
									audio.Play ();
								}
							}
						}

						if (fillEmptyMethod == FillEmptyMethod.DISPLAY && layerList[i].frameList[layerList[i].currentFrame].isDuplicate) {
							layerList[i].frameList[layerList[i].previousFrame].showFrame (true);
						} else {
							layerList[i].frameList[layerList[i].previousFrame].showFrame (false);
							layerList[i].frameList[layerList[i].currentFrame].showFrame (true);
						}
					}
				}
			} else {
				if (audio != null && audio.isPlaying && Time.realtimeSinceStartup > markTime + frameInterval) audio.Stop ();
			}

			if (animator != null) animator.Play(clipName, clipLayer, animVal);

			// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~

			try {
				if (isDrawing && Vector3.Distance(lastTargetPos, target.position) > minDistance) {
					buildStroke();
				}
			} catch (System.Exception e) {
				Debug.Log (e.Data);
			}

			if (clicked && !isDrawing) {
				beginStroke();
                if (drawWhilePlaying && isPlaying && layerList[currentLayer].frameList.Count > 1 && layerList[currentLayer].frameList[layerList[currentLayer].previousFrame].brushStrokeList.Count > 0) {
                    BrushStroke lastStroke = layerList[currentLayer].frameList[layerList[currentLayer].previousFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].previousFrame].brushStrokeList.Count - 1];
					for (int pts = lastStroke.points.Count / drawTrailLength; pts < lastStroke.points.Count - 1; pts++) {
                        layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].points.Add(lastStroke.points[pts]);
                    }
                }
			}

			if (!clicked && isDrawing) {
				endStroke();
			}

			if (textMesh != null) textMesh.text = "frame " + (layerList [currentLayer].currentFrame + 1) + " / " + layerList [currentLayer].frameList.Count;

            if (layerList.Count > 1 && textMesh != null) textMesh.text = "" + (currentLayer + 1) + ". " + textMesh.text;

            if (layerList[currentLayer].frameList.Count < layerList[longestLayer].frameList.Count && textMesh != null) textMesh.text += " (" + layerList [longestLayer].frameList.Count + ")";
		}

		lastTargetPos = target.position;
	}

	int getLongestLayer() {
		int returns = 0;
		for (int i = 0; i < layerList.Count; i++) {
			if (layerList[i].frameList.Count > layerList[returns].frameList.Count) returns = i;
		}
		return returns;
	}

	void beginStroke() {
		if (!drawWhilePlaying) isPlaying = false;
		isDrawing = true;
		instantiateStroke(mainColor);
		layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].setBrushColor(mainColor);
	}

	void buildStroke() {
		layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].isDirty = true;

		layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count-1].points.Add(target.position);
	}

	void endStroke() {
        if (!isPlaying && refineStrokes) layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].refine();
        isDrawing = false;
	}

	public void testRandomStrokes() {
		int numStrokes = 10;
		int numSegments = 4;
		float range = 5f;

		for (int i=0; i<numStrokes; i++) {
			instantiateStroke(mainColor);
			randomStroke(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count-1], numSegments, range);
		}
	}

	void randomStroke(BrushStroke _b, int _num, float _range) {
		for (int i=0; i<_num; i++) {
			Vector3 v = new Vector3(Random.Range(-_range, _range),Random.Range(-_range, _range),Random.Range(-_range, _range));
			_b.points.Add(v); 
		}
	}

	public void resetAll() {
		for (int i = 0; i < layerList[currentLayer].frameList.Count; i++) {
			layerList[currentLayer].frameList[i].reset();
			Destroy (layerList[currentLayer].frameList[i].gameObject);
		}

		layerList[currentLayer].frameList = new List<BrushFrame>();
		instantiateFrame();
		refreshFrame();
	}

	/*
    public void applyScaleAndOffset() {
		if (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList != null) {
			for (int i=0; i<layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
				layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushSize = brushSize;

				layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].useScaleAndOffset = true;
				//layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].globalScale = globalScale;
				//layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].globalOffset = globalOffset;
			}
		}	
	}
    */

	void changeFrame(int index) {
		isPlaying = false;

		frameMotor(index);
	}

	void frameMotor(int index) {
		for (int i = 0; i < layerList.Count; i++) {
			Debug.Log ("Layer " + i + ", frame " + layerList [i].currentFrame);
			if (layerList [i].frameList.Count > 0) {
				layerList [i].frameList [layerList [i].currentFrame].showFrame (false);

				layerList [i].currentFrame += index;
				if (layerList [i].currentFrame < 0) {
					layerList [i].currentFrame = layerList [i].frameList.Count - 1;
				} else if (layerList [i].currentFrame > layerList [i].frameList.Count - 1) {
					layerList [i].currentFrame = 0;
				}
				layerList [i].frameList [layerList [i].currentFrame].showFrame (true);
			}

			if (i == longestLayer) animVal = normalizedFrameInterval * layerList[longestLayer].currentFrame; // TODO should this be longest layer?
		}
	}

	void jumpToFrame(int index) {
		int diff = 0;

		for (int i = 0; i < layerList.Count; i++) {
			
			if (index > layerList [i].currentFrame) {
				diff = index - layerList [i].currentFrame;
			} else if (index < layerList [i].currentFrame) {
				diff = layerList [i].currentFrame - index;
			}

			Debug.Log ("currentFrame: " + layerList [i].currentFrame + "   index: " + index + "   diff: " + diff);
			changeFrame (diff);
		}
	}

	void showAllFrames(bool _b) {
		for (int i=0; i < layerList[currentLayer].frameList.Count; i++) {
			if (i >= layerList[currentLayer].currentFrame - onionSkinRange && i <= layerList[currentLayer].currentFrame + onionSkinRange) {
				layerList[currentLayer].frameList[i].showFrame(_b);
			}

			if (_b) {
				if (i != layerList[currentLayer].currentFrame) {
					layerList[currentLayer].frameList[i].setFrameBrightness(frameBrightDim);
				} else {
                    layerList[currentLayer].frameList[i].setFrameBrightness(frameBrightNormal);
				}
			} else {
                layerList[currentLayer].frameList[i].setFrameBrightness(frameBrightNormal);
			}
		}

		if (!_b) layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].showFrame(true);
	}

	void instantiateStroke(Color c) {
        BrushStroke b = Instantiate(brushPrefab);
        //b.brushMode = (BrushStroke.BrushMode) brushMode;
        if (brushMat[(int)brushMode]) b.mat = brushMat[(int)brushMode];
		b.brushSize = brushSize;
		b.brushColor = c;
		if (useEndColor) {
			b.brushEndColor = endColor;
		} else {
			b.brushEndColor = c;
		}
		b.transform.SetParent(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].transform);
		layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Add(b);	
	}

    public void inputInstantiateStroke(Color c, List<Vector3> points) {
        BrushStroke b = Instantiate(brushPrefab);
        b.points = points;
        //b.brushMode = (BrushStroke.BrushMode)brushMode;
        if (brushMat[(int)brushMode]) b.mat = brushMat[(int)brushMode];
        b.brushSize = brushSize;
        b.brushColor = c;
        if (useEndColor) {
            b.brushEndColor = endColor;
        } else {
            b.brushEndColor = c;
        }
        b.transform.SetParent(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].transform);
        layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Add(b);
    }

	void instantiateFrame() {
        BrushFrame f = Instantiate(framePrefab);
        f.transform.SetParent(layerList[currentLayer].transform);
		layerList[currentLayer].frameList.Add(f);
		longestLayer = getLongestLayer();
	}

	void instantiateLayer() {
        BrushLayer l = Instantiate(layerPrefab);
        l.transform.SetParent(transform);
		layerList.Add(l);
		Debug.Log ("layerList has " + layerList.Count + " layers.");
	}

	public IEnumerator readBrushStrokes() {
		Debug.Log ("*** Begin reading...");
		isReadingFile = true;

		for (int h = 0; h < layerList.Count; h++) {
			for (int i = 0; i < layerList [h].frameList.Count; i++) {
				Destroy (layerList [h].frameList [i].gameObject);
			}
			Destroy(layerList[h].gameObject);
		}
		layerList = new List<BrushLayer>();

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

		jsonNode = JSON.Parse (www.text);

		for (int f = 0; f < jsonNode["grease_pencil"][0]["layers"].Count; f++) {
			instantiateLayer();
			currentLayer = f;
			layerList[currentLayer].name = jsonNode["grease_pencil"][0]["layers"][f]["name"];
			for (int h = 0; h < jsonNode["grease_pencil"][0]["layers"][f]["frames"].Count; h++) {
				Debug.Log ("Starting frame " + (layerList[currentLayer].currentFrame + 1) + ".");
				instantiateFrame();
				layerList[currentLayer].currentFrame = h;

				for (int i = 0; i < jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"].Count; i++) {
					float r = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][0].AsFloat;
					float g = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][1].AsFloat;
					float b = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][2].AsFloat;
					Color c = new Color (r, g, b);

					instantiateStroke (c);
					for (int j = 0; j < jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"].Count; j++) {
						//float x = 0f;
						//float y = 0f;
						//float z = 0f;

						/*
                        if (useScaleAndOffsetRead) {
							x = (jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][0].AsFloat * globalScale.x) + globalOffset.x;
							y = (jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][1].AsFloat * globalScale.y) + globalOffset.y;
							z = (jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][2].AsFloat * globalScale.z) + globalOffset.z; 						
						} else {
                        */
						float x = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][0].AsFloat;
						float y = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][1].AsFloat;
						float z = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][2].AsFloat; 				
						//}

						Vector3 p = applyTransformMatrix(new Vector3 (x, y, z));
                        //if (useScaleAndOffsetRead) p = applyTransformMatrix(p);

						layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].points.Add (p);	
					}

					Debug.Log ("Adding frame " + (layerList[currentLayer].currentFrame + 1) + ": stroke " + (i + 1) + " of " + layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count + ".");
				}
				if (textMesh != null) textMesh.text = "READING " + (layerList[currentLayer].currentFrame + 1) + " / " + jsonNode["grease_pencil"][0]["layers"][0]["frames"].Count;
				Debug.Log ("Ending frame " + (layerList[currentLayer].currentFrame + 1) + ".");
				yield return new WaitForSeconds (consoleUpdateInterval);
			}
		

			for (int h = 0; h < layerList[currentLayer].frameList.Count; h++) {
				layerList[currentLayer].currentFrame = h;
				layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].isDirty = true;
				if (checkEmptyFrame (layerList[currentLayer].currentFrame)) {
					if (fillEmptyMethod == FillEmptyMethod.WRITE) {
						copyFramePointsForward (layerList[currentLayer].currentFrame);
					}
				}
			}

			layerList[currentLayer].currentFrame = 0;

			for (int h = 0; h < layerList[currentLayer].frameList.Count; h++) {
				if (h != layerList[currentLayer].currentFrame) {
					layerList[currentLayer].frameList[h].showFrame (false);
				} else {
					layerList[currentLayer].frameList[h].showFrame (true);
				}
			}
		}

		if (newLayerOnRead) {
			instantiateLayer ();
			currentLayer = layerList.Count - 1;
			instantiateFrame ();
		}

		Debug.Log("*** Read " + url);
		isReadingFile = false;
		if (playOnStart) isPlaying = true;
	}

	public IEnumerator writeBrushStrokes() {
		Debug.Log ("*** Begin writing...");
		isWritingFile = true;

		ArrayList FINAL_LAYER_LIST = new ArrayList ();

		for (int fllA = 0; fllA < layerList.Count; fllA++) {
			currentLayer = fllA;

			ArrayList sb = new ArrayList ();
			ArrayList sbHeaderL = new ArrayList ();
			string sbHeader = "                    \"frames\":[" + "\n";
			sbHeaderL.Add (sbHeader);
			sb.Add (sbHeaderL);

			for (int h = 0; h < layerList[currentLayer].frameList.Count; h++) {
				Debug.Log ("Starting frame " + (layerList[currentLayer].currentFrame + 1) + ".");
				layerList[currentLayer].currentFrame = h;

				ArrayList sbbHeaderL = new ArrayList ();
				string sbbHeader = "                        {" + "\n";
				sbbHeader += "                            \"strokes\":[" + "\n";
				sbbHeaderL.Add (sbbHeader);
				sb.Add (sbbHeaderL);
				for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
					ArrayList sbbL = new ArrayList ();
					string sbb = "                                {" + "\n";
					float r = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.r;
					float g = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.g;
					float b = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.b;
					sbb += "                                    \"color\":[" + r + ", " + g + ", " + b + "]," + "\n";

					if (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count > 0) {
						sbb += "                                    \"points\":[" + "\n";
						for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
							//float x = 0f;
							//float y = 0f;
							//float z = 0f;

							/*
                            if (useScaleAndOffsetWrite) {
								x = (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j].x * globalScale.x) + globalOffset.x;
								y = (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j].y * globalScale.y) + globalOffset.y;
								z = (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j].z * globalScale.z) + globalOffset.z;
							} else {
                            */
							float x = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j].x;
							float y = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j].y;
							float z = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j].z;
							//}
                            //if (useScaleAndOffsetWrite) {
                                //Vector3 p = applyTransformMatrix(new Vector3(x, y, z));
                                //x = p.x;
                                //y = p.y;
                                //z = p.z;
                            //}

							if (j == layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count - 1) {
								sbb += "                                        {\"co\":[" + x + ", " + y + ", " + z + "], \"pressure\":1, \"strength\":1}" + "\n";
								sbb += "                                    ]" + "\n";
							} else {
								sbb += "                                        {\"co\":[" + x + ", " + y + ", " + z + "], \"pressure\":1, \"strength\":1}," + "\n";
							}
						}
					} else {
						sbb += "                                    \"points\":[]" + "\n";
					}

					if (i == layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1) {
						sbb += "                                }" + "\n";
					} else {
						sbb += "                                }," + "\n";
					}

					Debug.Log ("Adding frame " + (layerList[currentLayer].currentFrame + 1) + ": stroke " + (i + 1) + " of " + layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count + ".");

					sbbL.Add (sbb);
					sb.Add (sbbL);
				}

				if (textMesh != null) textMesh.text = "WRITING " + (layerList[currentLayer].currentFrame + 1) + " / " + layerList[currentLayer].frameList.Count;
				Debug.Log ("Ending frame " + (layerList[currentLayer].currentFrame + 1) + ".");
				yield return new WaitForSeconds (consoleUpdateInterval);

				ArrayList sbFooterL = new ArrayList ();
				string sbFooter = "";
				if (h == layerList[currentLayer].frameList.Count - 1) {
					sbFooter += "                            ]" + "\n";
					sbFooter += "                        }" + "\n";
				} else {
					sbFooter += "                            ]" + "\n";
					sbFooter += "                        }," + "\n";
				} 
				sbFooterL.Add (sbFooter);
				sb.Add (sbFooterL);
			}

			FINAL_LAYER_LIST.Add(sb);
		}

		yield return new WaitForSeconds(consoleUpdateInterval);
		Debug.Log("+++ Parsing finished. Begin file writing.");
		yield return new WaitForSeconds(consoleUpdateInterval);

		string s = "{" + "\n";
		s += "    \"creator\": \"unity\"," + "\n";
		s += "    \"grease_pencil\":[" + "\n";
		s += "        {" + "\n";
		s += "            \"layers\":[" + "\n";

		for (int fllB = 0; fllB < layerList.Count; fllB++) {
			s += "                {" + "\n";
			if (layerList[fllB].name != null && layerList[fllB].name != "") {
				s += "                    \"name\": \"" + layerList[fllB].name + "\"," + "\n";
			} else {
				s += "                    \"name\": \"UnityLayer " + (fllB + 1) + "\"," + "\n";
			}
			ArrayList sb_y = (ArrayList) FINAL_LAYER_LIST[fllB];
			for (int zx = 0; zx < sb_y.Count; zx++) {
				ArrayList x = (ArrayList) sb_y[zx];
				for (int zzx = 0; zzx < x.Count; zzx++) {
					s += x[zzx];
				}
			}
			s += "                    ]" + "\n";
			if (fllB < layerList.Count - 1) {
				s += "                }," + "\n";
			} else {
				s += "                }" + "\n";
			}
		}
		s += "            ]" + "\n"; // end layers
		s += "        }" + "\n";
		s += "    ]" + "\n";
		s += "}" + "\n";

		string url = "";
		if (useTimestamp) {
			string ext = ".json";
			string tempName = writeFileName.Replace(ext, "");
			int timestamp = (int) (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
			tempName += "_" + timestamp + ext;
			url = Path.Combine(Application.dataPath, tempName);

			#if UNITY_ANDROID
			url = "/sdcard/Movies/" + tempName;
			#endif

			#if UNITY_IOS
			url = Path.Combine(Application.persistentDataPath, tempName);
			#endif
		} else {
			url = Path.Combine(Application.dataPath, writeFileName);

			#if UNITY_ANDROID
			url = "/sdcard/Movies/" + writeFileName;
			#endif

			#if UNITY_IOS
			url = Path.Combine(Application.persistentDataPath, writeFileName);
			#endif
		}

		File.WriteAllText(url, s); 
		Debug.Log("*** Wrote " + url);
		isWritingFile = false;

		yield return null;
	}

	void doErase() {
		isPlaying = false;

		int strokeToDelete = -1;

		for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
			bool foundStroke = false;
			for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
				if (!foundStroke && Vector3.Distance(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], target.position) < eraseRange) {
					strokeToDelete = i;
					foundStroke = true;
				}
			}
		}

		if (strokeToDelete > -1) {
			Destroy(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[strokeToDelete].gameObject);
			layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.RemoveAt(strokeToDelete);
		}
	}

	void eraseLastStroke() {
		Destroy(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].gameObject);
		layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.RemoveAt(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1); 
	}

	void eraseFirstStroke() {
		Destroy(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[0].gameObject);
		layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.RemoveAt(0); 
	}

	void doPush() {
		isPlaying = false;

		for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
			for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
				if (Vector3.Distance (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], target.position) < pushRange) {
					layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j] = Vector3.Lerp (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], target.position, pushSpeed);
					layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].isDirty = true;
				}
			}
		}
	}

	void doColorPick() {
		isPlaying = false;

		for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
			for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
				if (Vector3.Distance (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], target.position) < colorPickRange) {
					mainColor = new Color(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.r, layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.g, layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.b);
				}
			}
		}
	}

	Vector3 tween3D(Vector3 v1, Vector3 v2, float e) {
		v1 += (v2 - v1) / e;
		return v1;
	}

	bool checkEmptyFrame(int _index) {
		if (_index > 0 && layerList[currentLayer].frameList[_index].brushStrokeList.Count == 0) {
			layerList[currentLayer].frameList[_index].isDuplicate = true;
			Debug.Log("Empty frame " + _index);
		} else {
			layerList[currentLayer].frameList[_index].isDuplicate = false;
		}
		return layerList[currentLayer].frameList[_index].isDuplicate;
	}

	void copyFramePointsForward(int index) {
		Debug.Log("Before copy (" + index + "): " + layerList[currentLayer].frameList[index - 1].brushStrokeList.Count + " " + layerList[currentLayer].frameList[index].brushStrokeList.Count);
		for (int i = 0; i < layerList[currentLayer].frameList[index - 1].brushStrokeList.Count; i++) {
			instantiateStroke(mainColor);
			layerList[currentLayer].frameList[index].brushStrokeList[i].setPoints(layerList[currentLayer].frameList[index - 1].brushStrokeList[i].points);
			layerList[currentLayer].frameList[index].brushStrokeList[i].setBrushColor(layerList[currentLayer].frameList[index - 1].brushStrokeList[i].brushColor);
			layerList[currentLayer].frameList[index].brushStrokeList[i].isDirty = true;
		}
		Debug.Log("After copy (" + index + "): " + layerList[currentLayer].frameList[index - 1].brushStrokeList.Count + " " + layerList[currentLayer].frameList[index].brushStrokeList.Count);
	}

	// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 

	public void inputNewFrame() {
		if (textMeshRen != null) textMeshRen.enabled = true;
		instantiateFrame();
		layerList[currentLayer].currentFrame = layerList[currentLayer].frameList.Count - 1;
		showAllFrames(showOnionSkin);
	}

	public void inputNewFrameAndCopy() {
		inputNewFrame();

		copyFramePointsForward(layerList[currentLayer].currentFrame);
	}

	void refreshFrame() {
		changeFrame(0);
	}

	public void inputPlay() {
		isPlaying = !isPlaying;

		showOnionSkin = false;
		inputHideFrames();

		for (int h = 0; h < layerList.Count; h++) {
			for (int i = 0; i < layerList[h].frameList.Count; i++) {
				layerList[h].frameList[i].showFrame(false);
			}
		}

		if (isPlaying) {
			doAudioPlay();
		} else {
			for (int h = 0; h < layerList.Count; h++) {
				layerList[h].frameList[layerList[h].currentFrame].showFrame(true);
			}
			doAudioStop();
		}

		if (textMeshRen != null) textMeshRen.enabled = !isPlaying;

		if (animatorRen.Length != 0 && animatorRen[0] != null) {
			for (int i = 0; i < animatorRen.Length; i++) {
				animatorRen[i].enabled = !isPlaying;
			}
		}

		Debug.Log ("isPlaying: " + isPlaying);
	}

	public void inputFrameBack() {
		if (textMeshRen != null) textMeshRen.enabled = true;
		changeFrame(-1);	
		doAudioPlay();
		if (showOnionSkin) inputShowFrames();
	}

	public void inputFrameForward() {
		if (textMeshRen != null) textMeshRen.enabled = true;
		changeFrame(1);	
		doAudioPlay();
		if (showOnionSkin) inputShowFrames();
	}

	public void inputShowFrames() {
		if (textMeshRen != null) textMeshRen.enabled = true;
		if (floorRen != null) floorRen.enabled = true;
		showAllFrames(true);	
	}

	public void inputHideFrames() {
		if (textMeshRen != null) textMeshRen.enabled = true;
		if (floorRen != null) floorRen.enabled = false;
		showAllFrames(false);	
	}

	public void inputErase() {
		doErase();
	}

	public void inputEraseLastStroke() {
		eraseLastStroke();
	}

	public void inputEraseFirstStroke() {
		eraseFirstStroke();
	}

	public void inputPush() {
		doPush();
	}

	public void inputColorPick() {
		doColorPick();
	}

	public void inputRefreshFrame() {
		refreshFrame();
	}

	public void inputOnionSkin() {
		showOnionSkin = !showOnionSkin;
		if (showOnionSkin) {
			inputShowFrames();
		} else {
			inputHideFrames();
		}
	}

	public void inputNextLayer() {
		showOnionSkin = false;
		inputHideFrames();
		currentLayer++;
		if (currentLayer > layerList.Count - 1) currentLayer = 0;
	}

	public void inputNewLayer() {
		showOnionSkin = false;
		inputHideFrames();
		instantiateLayer();
		currentLayer = layerList.Count - 1;
		if (createFrameWithLayer && layerList[currentLayer].frameList.Count < 1) instantiateFrame();
	}

    public void brushSizeInc() {
        brushSize += brushSizeDelta;
    }

    public void brushSizeDec() {
        brushSize -= brushSizeDelta;
        if (brushSize < brushSizeDelta) {
            brushSize = brushSizeDelta;
        }
    }

	void doAudioPlay() {
		if (audio != null) {
			markTime = Time.realtimeSinceStartup;
			if (layerList[currentLayer].currentFrame * frameInterval <= audio.clip.length) {
				audio.time = layerList[currentLayer].currentFrame * frameInterval;
				audio.Stop();
				audio.Play();			
			}
		}
	}

	void doAudioStop() {
		if (audio != null) {
			audio.Stop();
		}
	}

}
