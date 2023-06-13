﻿/*
LIGHTNING ARTIST TOOLKIT v1.1.0

The Lightning Artist Toolkit was developed with support from:
   Canada Council on the Arts
   Eyebeam Art + Technology Center
   Ontario Arts Council
   Toronto Arts Council
   
Copyright (c) 2019 Nick Fox-Gieg
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
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using ICSharpCode.SharpZipLibUnityPort.Core;
using ICSharpCode.SharpZipLibUnityPort.Zip;

public class LightningArtist : MonoBehaviour {

    [Header("~ Objects ~")]
    public Transform target;
    public LatkLayer layerPrefab;
    public LatkFrame framePrefab;
    public LatkStroke brushPrefab;
    public TextMesh textMesh;
    public AudioSource sound;
    public Animator animator;
    public Renderer floorRen;
    public Renderer[] animatorRen;

    public enum BrushMode { ADD, SURFACE, UNLIT };
    [Header("~ Brush Options ~")]
    public BrushMode brushMode = BrushMode.ADD;
    public Material[] brushMat;
    public Color mainColor = new Color(0.5f, 0.5f, 0.5f);
    public Color endColor = new Color(0.5f, 0.5f, 0.5f);
    public bool useEndColor = false;
    public bool fillMeshStrokes = false;
    public bool refineStrokes = false;
    public bool drawWhilePlaying = false;
    public float strokeLife = 5f;
    public bool killStrokes = false;
    public bool useCollisions = false;

    [Header("~ UI Options ~")]
    public float minDistance = -1f; //0.0001f;
    public float brushSize = 0.008f;
    [HideInInspector] public float pushSpeed = 0.01f;
    [HideInInspector] public float pushRange = 0.05f;
    [HideInInspector] public float eraseRange = 0.05f;
    [HideInInspector] public float colorPickRange = 0.05f;

    [Header("~ File Options ~")]
    public string readFileName = "LatkStrokes-saved.json";
    public bool readOnStart = false;
    public bool playOnStart = false;
    public string writeFileName = "LatkStrokes-saved.json";
    public bool useTimestamp = false;
    public bool writePressure = false;
    public bool createFrameWithLayer = false;
    public bool newLayerOnRead = false;

    // NONE plays empty frames as empty. 
    // WRITE copies last good frame into empty frame (will save out). 
    // DISPLAY holds last good frame but doesn't copy (won't save out).
    public enum FillEmptyMethod { NONE, WRITE, DISPLAY };
    [Header("~ Display Options ~")]
    public FillEmptyMethod fillEmptyMethod = FillEmptyMethod.DISPLAY;
    public float frameInterval = 12f;
    public int onionSkinRange = 5;
    public int drawTrailLength = 4;
    public float frameBrightNormal = 0.5f;
    public float frameBrightDim = 0.05f;

    [HideInInspector] public List<LatkLayer> layerList;
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

    [HideInInspector] public Vector3 lastHit = Vector3.zero;
    [HideInInspector] public Vector3 thisHit = Vector3.zero;
    [HideInInspector] public string url;

    [HideInInspector] public string statusText = "";

    //private bool firstRun = true;
    private float lastFrameTime = 0f;
    private Renderer textMeshRen;
    //private int rememberFrame = 0;
    private float markTime = 0f;
    private Vector2 brushSizeRange = new Vector2(0f, 1f);

    private float normalizedFrameInterval = 0f;
    private string clipName = "Take 001";
    private int clipLayer = 0;
    private float animVal = 0f;
    private Vector3 lastTargetPos = Vector3.zero;

    private float consoleUpdateInterval = 0f;
    //private int debugTextCurrentFrame = 0;
    //private int debugTextLastFrame = 0;
    private int longestLayer = 0;
    private float brushSizeDelta = 0f;

    private Matrix4x4 transformMatrix;
    private Matrix4x4 cameraTransformMatrix;

    public void updateTransformMatrix() {
        transformMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
    }

    public void updateCameraTransformMatrix() {
        cameraTransformMatrix = Matrix4x4.TRS(Camera.main.transform.position, Camera.main.transform.rotation, Camera.main.transform.localScale);
    }

    public Vector3 applyTransformMatrix(Vector3 p) {
        return transformMatrix.MultiplyPoint3x4(p);
    }

    public Vector3 applyCameraTransformMatrix(Vector3 p) {
        return cameraTransformMatrix.MultiplyPoint3x4(p);
    }

    void Awake() {
        updateTransformMatrix();
        updateCameraTransformMatrix();
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

        brushSizeDelta = brushSize / 30f;
    }

    void Update() {
    	pushSpeed = pushRange = eraseRange = colorPickRange = brushSize;
    	
        if (armReadFile) {
            statusText = "READING...";
            StartCoroutine(readLatkStrokes());
            armReadFile = false;
        } else if (armWriteFile) {
            statusText = "WRITING...";
            StartCoroutine(writeLatkStrokes());
            armWriteFile = false;
        } else if (!isReadingFile && !isWritingFile) {
            if (useCollisions) updateCollision();

            for (int i = 0; i < layerList.Count; i++) {
                if (layerList[i].frameList.Count > 0 && !layerList[i].frameList[layerList[i].currentFrame].isDuplicate) {
                    layerList[i].previousFrame = layerList[i].currentFrame;
                }
            }

            if (isPlaying) {
                float t = 0f;
                if (sound != null) {
                    t = markTime + sound.time;
                } else {
                    t = Time.realtimeSinceStartup;
                }

                if ((sound != null && Time.realtimeSinceStartup > markTime + sound.clip.length) || (layerList[longestLayer].frameList.Count > 1 && t > lastFrameTime + frameInterval)) {
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
                        layerList[i].frameList[layerList[i].currentFrame].showFrame(false);
                        layerList[i].currentFrame++;

                        if (layerList[i].currentFrame > layerList[i].frameList.Count - 1) {
                            layerList[i].currentFrame = 0;

                            if (i == longestLayer) {
                                animVal = 0f;
                                markTime = Time.realtimeSinceStartup;
                                if (sound != null) {
                                    sound.time = 0f;
                                    sound.Stop();
                                    sound.Play();
                                }
                            }
                        }

                        if (fillEmptyMethod == FillEmptyMethod.DISPLAY && layerList[i].frameList[layerList[i].currentFrame].isDuplicate) {
                            layerList[i].frameList[layerList[i].previousFrame].showFrame(true);
                        } else {
                            layerList[i].frameList[layerList[i].previousFrame].showFrame(false);
                            layerList[i].frameList[layerList[i].currentFrame].showFrame(true);
                        }
                    }
                }
            } else {
                if (sound != null && sound.isPlaying && Time.realtimeSinceStartup > markTime + frameInterval) sound.Stop();
            }

            if (animator != null) animator.Play(clipName, clipLayer, animVal);

            // ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~

            //try {
            if (isDrawing) {
                if (minDistance > 0f) {
                    if (isDrawing && Vector3.Distance(lastTargetPos, target.position) > minDistance) buildStroke();
                } else {
                    buildStroke();
                }
            }
            //} catch (System.Exception e) {
                //Debug.Log(e.Data);
            //}

            if (clicked && !isDrawing) {
                beginStroke();
                if (!useCollisions && drawWhilePlaying && isPlaying && layerList[currentLayer].frameList.Count > 1 && layerList[currentLayer].frameList[layerList[currentLayer].previousFrame].brushStrokeList.Count > 0) {
                    LatkStroke lastStroke = layerList[currentLayer].frameList[layerList[currentLayer].previousFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].previousFrame].brushStrokeList.Count - 1];
                    for (int pts = lastStroke.points.Count / drawTrailLength; pts < lastStroke.points.Count - 1; pts++) {
                        layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].points.Add(lastStroke.points[pts]);
                    }
                }
            }

            if (!clicked && isDrawing) {
                endStroke();
            }

            statusText = "frame " + (layerList[currentLayer].currentFrame + 1) + " / " + layerList[currentLayer].frameList.Count;

            if (layerList.Count > 1) statusText = "" + (currentLayer + 1) + ". " + statusText;

            if (layerList[currentLayer].frameList.Count < layerList[longestLayer].frameList.Count) statusText += " (" + layerList[longestLayer].frameList.Count + ")";
        }

        lastTargetPos = target.position;

        if (textMesh != null) textMesh.text = statusText;
    }

    int getLongestLayer() {
        int returns = 0;
        for (int i = 0; i < layerList.Count; i++) {
            if (layerList[i].frameList.Count > layerList[returns].frameList.Count) returns = i;
        }
        return returns;
    }

    void beginStroke() {
        if (useCollisions) {
            lastHit = Vector3.zero; 
        }
        if (!drawWhilePlaying) isPlaying = false;
        isDrawing = true;
        instantiateStroke(mainColor);
        layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].setBrushColor(mainColor);
    }

    void buildStroke() {
        if (useCollisions) {
            Vector3 p = getCollision();
            if (blockCollisions(p)) return;
            layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].points.Add(p);
        } else {
            layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].points.Add(target.position);
        }
        layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].isDirty = true;
    }

    void endStroke() {
        if (!isPlaying) {
            if (fillMeshStrokes) layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].fillMesh();
            if (refineStrokes) layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].refine();
        }
        isDrawing = false;
    }

    bool blockCollisions(Vector3 p) {
        bool returns = false;
        if (p == Vector3.zero) {
            returns = true;
            clicked = false;
            isDrawing = false;
        }
        return returns;
    }

    public void testRandomStrokes() {
        int numStrokes = 10;
        int numSegments = 4;
        float range = 5f;

        for (int i = 0; i < numStrokes; i++) {
            instantiateStroke(mainColor);
            randomStroke(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1], numSegments, range);
        }
    }

    void randomStroke(LatkStroke _b, int _num, float _range) {
        for (int i = 0; i < _num; i++) {
            Vector3 v = new Vector3(Random.Range(-_range, _range), Random.Range(-_range, _range), Random.Range(-_range, _range));
            _b.points.Add(v);
        }
    }

    public void resetAll() {
        for (int i = 0; i < layerList[currentLayer].frameList.Count; i++) {
            layerList[currentLayer].frameList[i].reset();
            Destroy(layerList[currentLayer].frameList[i].gameObject);
        }

        layerList[currentLayer].frameList = new List<LatkFrame>();
        instantiateFrame();
        refreshFrame();
    }

    void changeFrame(int index) {
        isPlaying = false;
        frameMotor(index);
    }

    void frameMotor(int index) {
        for (int i = 0; i < layerList.Count; i++) {
            Debug.Log("Layer " + i + ", frame " + layerList[i].currentFrame);
            if (layerList[i].frameList.Count > 0) {
                layerList[i].frameList[layerList[i].currentFrame].showFrame(false);

                layerList[i].currentFrame += index;
                if (layerList[i].currentFrame < 0) {
                    layerList[i].currentFrame = layerList[i].frameList.Count - 1;
                } else if (layerList[i].currentFrame > layerList[i].frameList.Count - 1) {
                    layerList[i].currentFrame = 0;
                }
                layerList[i].frameList[layerList[i].currentFrame].showFrame(true);
            }

            if (i == longestLayer) animVal = normalizedFrameInterval * layerList[longestLayer].currentFrame; // TODO should this be longest layer?
        }
    }

    void jumpToFrame(int index) {
        int diff = 0;

        for (int i = 0; i < layerList.Count; i++) {

            if (index > layerList[i].currentFrame) {
                diff = index - layerList[i].currentFrame;
            } else if (index < layerList[i].currentFrame) {
                diff = layerList[i].currentFrame - index;
            }

            Debug.Log("currentFrame: " + layerList[i].currentFrame + "   index: " + index + "   diff: " + diff);
            changeFrame(diff);
        }
    }

    void showAllFrames(bool _b) {
        for (int i = 0; i < layerList[currentLayer].frameList.Count; i++) {
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
        LatkStroke b = Instantiate(brushPrefab);
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
        LatkStroke b = Instantiate(brushPrefab);
        b.points = points;
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

	public void inputInstantiateStrokeHead(Color c, List<Vector3> points) {
		LatkStroke b = Instantiate(brushPrefab);
		b.points = points;
		if (brushMat[(int)brushMode]) b.mat = brushMat[(int)brushMode];
		b.brushSize = brushSize;
		b.brushColor = c;
		if (useEndColor) {
			b.brushEndColor = endColor;
		} else {
			b.brushEndColor = c;
		}
		b.transform.SetParent(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].transform);
		layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Insert(0, b);
	}

    void instantiateFrame() {
        LatkFrame f = Instantiate(framePrefab);
        f.transform.SetParent(layerList[currentLayer].transform);
        layerList[currentLayer].frameList.Add(f);
        longestLayer = getLongestLayer();
    }

    void instantiateLayer() {
        LatkLayer l = Instantiate(layerPrefab);
        l.transform.SetParent(transform);
        layerList.Add(l);
        Debug.Log("layerList has " + layerList.Count + " layers.");
    }

    void getBrushSizeRange() {
        List<float> allBrushSizes = new List<float>();

        for (int i=0; i<layerList.Count; i++) {
            for (int j=0; j<layerList[i].frameList.Count; j++) {
                for (int k=0; k<layerList[i].frameList[j].brushStrokeList.Count; k++) {
                    allBrushSizes.Add(layerList[i].frameList[j].brushStrokeList[k].brushSize);
                }
            }
        }

        allBrushSizes.Sort();
        brushSizeRange = new Vector2(allBrushSizes[0], allBrushSizes[allBrushSizes.Count-1]);
    }

    float getNormalizedBrushSize(float s) {
        return map(s, brushSizeRange.x, brushSizeRange.y, 0.1f, 1f);
    }

    float map(float s, float a1, float a2, float b1, float b2) {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public IEnumerator readLatkStrokes() {
        Debug.Log("*** Begin reading...");
        isReadingFile = true;

        string ext = Path.GetExtension(readFileName).ToLower();
        Debug.Log("Found extension " + ext);
        bool useZip = (ext == ".latk" || ext == ".zip");

        for (int h = 0; h < layerList.Count; h++) {
            for (int i = 0; i < layerList[h].frameList.Count; i++) {
                Destroy(layerList[h].frameList[i].gameObject);
            }
            Destroy(layerList[h].gameObject);
        }
        layerList = new List<LatkLayer>();

        url = "";

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

        #if UNITY_WSA
		url = Path.Combine("file://" + Application.dataPath, readFileName);		
        #endif

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        Debug.Log("+++ File reading finished. Begin parsing...");
        yield return new WaitForSeconds(consoleUpdateInterval);

        if (useZip) {
            jsonNode = getJsonFromZip(www.downloadHandler.data);
        } else {
            jsonNode = JSON.Parse(www.downloadHandler.text);
        }

        for (int f = 0; f < jsonNode["grease_pencil"][0]["layers"].Count; f++) {
            instantiateLayer();
            currentLayer = f;
            layerList[currentLayer].info = jsonNode["grease_pencil"][0]["layers"][f]["name"];
            for (int h = 0; h < jsonNode["grease_pencil"][0]["layers"][f]["frames"].Count; h++) {
                Debug.Log("Starting frame " + (layerList[currentLayer].currentFrame + 1) + ".");
                instantiateFrame();
                layerList[currentLayer].currentFrame = h;

                try {
                    float px = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["parent_location"][0].AsFloat / 10f;
                    float py = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["parent_location"][2].AsFloat / 10f;
                    float pz = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["parent_location"][1].AsFloat / 10f;
                    layerList[currentLayer].frameList[h].parentPos = new Vector3(px, py, pz);
                } catch (UnityException e) {
                    Debug.Log(e.Message);
                }

                for (int i = 0; i < jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"].Count; i++) {
                    float r = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][0].AsFloat;
                    float g = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][1].AsFloat;
                    float b = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["color"][2].AsFloat;
                    Color c = new Color(r, g, b);

                    instantiateStroke(c);
                    for (int j = 0; j < jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"].Count; j++) {
                        float x = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][0].AsFloat;
                        float y = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][1].AsFloat;
                        float z = jsonNode["grease_pencil"][0]["layers"][f]["frames"][h]["strokes"][i]["points"][j]["co"][2].AsFloat;

                        Vector3 p = applyTransformMatrix(new Vector3(x, y, z));

                        layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1].points.Add(p);
                    }

                    Debug.Log("Adding frame " + (layerList[currentLayer].currentFrame + 1) + ": stroke " + (i + 1) + " of " + layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count + ".");
                }
                statusText = "READING " + (layerList[currentLayer].currentFrame + 1) + " / " + jsonNode["grease_pencil"][0]["layers"][0]["frames"].Count;
                Debug.Log("Ending frame " + (layerList[currentLayer].currentFrame + 1) + ".");
                yield return new WaitForSeconds(consoleUpdateInterval);
            }


            for (int h = 0; h < layerList[currentLayer].frameList.Count; h++) {
                layerList[currentLayer].currentFrame = h;
                layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].isDirty = true;
                if (checkEmptyFrame(layerList[currentLayer].currentFrame)) {
                    if (fillEmptyMethod == FillEmptyMethod.WRITE) {
                        copyFramePointsForward(layerList[currentLayer].currentFrame);
                    }
                }
            }

            layerList[currentLayer].currentFrame = 0;

            for (int h = 0; h < layerList[currentLayer].frameList.Count; h++) {
                if (h != layerList[currentLayer].currentFrame) {
                    layerList[currentLayer].frameList[h].showFrame(false);
                } else {
                    layerList[currentLayer].frameList[h].showFrame(true);
                }
            }
        }

        if (newLayerOnRead) {
            instantiateLayer();
            currentLayer = layerList.Count - 1;
            instantiateFrame();
        }

        Debug.Log("*** Read " + url);
        isReadingFile = false;
        if (playOnStart) isPlaying = true;
    }

    public IEnumerator writeLatkStrokes() {
        Debug.Log("*** Begin writing...");
        isWritingFile = true;

        string ext = Path.GetExtension(writeFileName).ToLower();
        Debug.Log("Found extension " + ext);
        bool useZip = (ext == ".latk" || ext == ".zip");

        List<string> FINAL_LAYER_LIST = new List<string>();

        if (writePressure) getBrushSizeRange();

        for (int hh = 0; hh < layerList.Count; hh++) {
            currentLayer = hh;

            List<string> sb = new List<string>();
            List<string> sbHeader = new List<string>();
            sbHeader.Add("\t\t\t\t\t\"frames\":[");
            sb.Add(string.Join("\n", sbHeader.ToArray()));

            for (int h = 0; h < layerList[currentLayer].frameList.Count; h++) {
                Debug.Log("Starting frame " + (layerList[currentLayer].currentFrame + 1) + ".");
                layerList[currentLayer].currentFrame = h;

                List<string> sbbHeader = new List<string>();
                sbbHeader.Add("\t\t\t\t\t\t{");
                sbbHeader.Add("\t\t\t\t\t\t\t\"strokes\":[");
                sb.Add(string.Join("\n", sbbHeader.ToArray()));
                for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
                    List<string> sbb = new List<string>();
                    sbb.Add("\t\t\t\t\t\t\t\t{");
                    float r = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.r;
                    float g = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.g;
                    float b = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushColor.b;
                    sbb.Add("\t\t\t\t\t\t\t\t\t\"color\":[" + r + ", " + g + ", " + b + "],");

                    if (layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count > 0) {
                        sbb.Add("\t\t\t\t\t\t\t\t\t\"points\":[");
                        for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
                            Vector3 pt = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].transform.TransformPoint(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j]);
                            float x = pt.x;
                            float y = pt.y;
                            float z = pt.z;

                            string pressureVal = "1";
                            if (writePressure) pressureVal = "" + getNormalizedBrushSize(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].brushSize);

                            if (j == layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count - 1) {
                                sbb.Add("\t\t\t\t\t\t\t\t\t\t{\"co\":[" + x + ", " + y + ", " + z + "], \"pressure\":" + pressureVal + ", \"strength\":1}");
                                sbb.Add("\t\t\t\t\t\t\t\t\t]");
                            } else {
                                sbb.Add("\t\t\t\t\t\t\t\t\t\t{\"co\":[" + x + ", " + y + ", " + z + "], \"pressure\":" + pressureVal + ", \"strength\":1},");
                            }
                        }
                    } else {
                        sbb.Add("\t\t\t\t\t\t\t\t\t\"points\":[]");
                    }

                    if (i == layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count - 1) {
                        sbb.Add("\t\t\t\t\t\t\t\t}");
                    } else {
                        sbb.Add("\t\t\t\t\t\t\t\t},");
                    }

                    Debug.Log("Adding frame " + (layerList[currentLayer].currentFrame + 1) + ": stroke " + (i + 1) + " of " + layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count + ".");

                    sb.Add(string.Join("\n", sbb.ToArray()));
                }

                statusText = "WRITING " + (layerList[currentLayer].currentFrame + 1) + " / " + layerList[currentLayer].frameList.Count;
                Debug.Log("Ending frame " + (layerList[currentLayer].currentFrame + 1) + ".");
                yield return new WaitForSeconds(consoleUpdateInterval);

                List<string> sbFooter = new List<string>();
                if (h == layerList[currentLayer].frameList.Count - 1) {
                    sbFooter.Add("\t\t\t\t\t\t\t]");
                    sbFooter.Add("\t\t\t\t\t\t}");
                } else {
                    sbFooter.Add("\t\t\t\t\t\t\t]");
                    sbFooter.Add("\t\t\t\t\t\t},");
                }
                sb.Add(string.Join("\n", sbFooter.ToArray()));
            }

            FINAL_LAYER_LIST.Add(string.Join("\n", sb.ToArray()));
        }

        yield return new WaitForSeconds(consoleUpdateInterval);
        Debug.Log("+++ Parsing finished. Begin file writing.");
        yield return new WaitForSeconds(consoleUpdateInterval);

        List<string> s = new List<string>();
        s.Add("{");
        s.Add("\t\"creator\": \"unity\",");
        s.Add("\t\"grease_pencil\":[");
        s.Add("\t\t{");
        s.Add("\t\t\t\"layers\":[");

        for (int i = 0; i < layerList.Count; i++) {
            currentLayer = i;

            s.Add("\t\t\t\t{");
            if (layerList[currentLayer].info != null && layerList[currentLayer].info != "") {
                s.Add("\t\t\t\t\t\"name\": \"" + layerList[currentLayer].info + "\",");
            } else {
                s.Add("\t\t\t\t\t\"name\": \"UnityLayer " + (currentLayer + 1) + "\",");
            }

            s.Add(FINAL_LAYER_LIST[currentLayer]);

            s.Add("\t\t\t\t\t]");
            if (currentLayer < layerList.Count - 1) {
                s.Add("\t\t\t\t},");
            } else {
                s.Add("\t\t\t\t}");
            }
        }
        s.Add("            ]"); // end layers
        s.Add("        }");
        s.Add("    ]");
        s.Add("}");

        url = "";
        string tempName = "";
        if (useTimestamp) {
            string extO = "";
            if (useZip) {
                extO = ".latk";
            } else {
                extO = ".json";
            }
            tempName = writeFileName.Replace(extO, "");
            int timestamp = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            tempName += "_" + timestamp + extO;
        }

        url = Path.Combine(Application.dataPath, tempName);

#if UNITY_ANDROID
        //url = "/sdcard/Movies/" + tempName;
        url = Path.Combine(Application.persistentDataPath, tempName);
#endif

#if UNITY_IOS
		url = Path.Combine(Application.persistentDataPath, tempName);
#endif

        if (useZip) {
            saveJsonAsZip(url, tempName, string.Join("\n", s.ToArray()));
        } else {
            File.WriteAllText(url, string.Join("\n", s.ToArray()));
        }

        Debug.Log("*** Wrote " + url);
        isWritingFile = false;

        yield return null;
    }

    void doErase() {
        int strokeToDelete = -1;

        for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
            bool foundStroke = false;
            Vector3 t = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].transform.InverseTransformPoint(target.position);

            for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
                if (!foundStroke && Vector3.Distance(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], t) < eraseRange) {
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
        for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
            Vector3 t = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].transform.InverseTransformPoint(target.position);

            for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
                if (Vector3.Distance(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], t) < pushRange) {
                    layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j] = Vector3.Lerp(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], t, pushSpeed);
                    layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].isDirty = true;
                }
            }
        }
    }

    void doColorPick() {
        for (int i = 0; i < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList.Count; i++) {
            Vector3 t = layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].transform.InverseTransformPoint(target.position);

            for (int j = 0; j < layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points.Count; j++) {
                if (Vector3.Distance(layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].brushStrokeList[i].points[j], t) < colorPickRange) {
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
            List<Vector3> newPoints = new List<Vector3>();
            for (int j=0; j< layerList[currentLayer].frameList[index - 1].brushStrokeList[i].points.Count; j++) {
                newPoints.Add(layerList[currentLayer].frameList[index - 1].brushStrokeList[i].transform.TransformPoint(layerList[currentLayer].frameList[index - 1].brushStrokeList[i].points[j]));
            }
            layerList[currentLayer].frameList[index].brushStrokeList[i].setPoints(newPoints);
            layerList[currentLayer].frameList[index].brushStrokeList[i].setBrushColor(layerList[currentLayer].frameList[index - 1].brushStrokeList[i].brushColor);
            layerList[currentLayer].frameList[index].brushStrokeList[i].isDirty = true;
        }
        Debug.Log("After copy (" + index + "): " + layerList[currentLayer].frameList[index - 1].brushStrokeList.Count + " " + layerList[currentLayer].frameList[index].brushStrokeList.Count);
    }

    // ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 

    public void inputNewFrame() {
        //if (textMeshRen != null) textMeshRen.enabled = true;
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

        //if (textMeshRen != null) textMeshRen.enabled = !isPlaying;

        if (animatorRen.Length != 0 && animatorRen[0] != null) {
            for (int i = 0; i < animatorRen.Length; i++) {
                animatorRen[i].enabled = !isPlaying;
            }
        }

        Debug.Log("isPlaying: " + isPlaying);
    }

    public void inputFrameBack() {
        //if (textMeshRen != null) textMeshRen.enabled = true;
        changeFrame(-1);
        doAudioPlay();
        if (showOnionSkin) inputShowFrames();
    }

    public void inputFrameForward() {
        //if (textMeshRen != null) textMeshRen.enabled = true;
        changeFrame(1);
        doAudioPlay();
        if (showOnionSkin) inputShowFrames();
    }

    public void inputFirstFrame() {
        jumpToFrame(0);
        changeFrame(0);
    }

    public void inputLastFrame() {
        int lastFrame = layerList[currentLayer].frameList.Count - 1;
        jumpToFrame(lastFrame);
        changeFrame(0);
    }

    public void inputShowFrames() {
        //if (textMeshRen != null) textMeshRen.enabled = true;
        if (floorRen != null) floorRen.enabled = true;
        showAllFrames(true);
    }

    public void inputHideFrames() {
        //if (textMeshRen != null) textMeshRen.enabled = true;
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

    public void inputDeleteFrame() {
        isPlaying = false;
        if (layerList[currentLayer].frameList.Count > 1) { // more than one frame exists on this layer, so OK to delete this frame
            layerList[currentLayer].deleteFrame();
        } else {
            if (layerList.Count > 1) { // more than one layer exists, so OK to delete this layer
                try {
                    Destroy(layerList[currentLayer].gameObject);
                } catch (UnityException e) {
                    Debug.Log(e.Message);
                }
                layerList.RemoveAt(currentLayer);
                currentLayer--;
                if (currentLayer < 0) currentLayer = 0;
            } else { // we're down to one layer and one frame, so just clear the frame's strokes without deleting it
                layerList[currentLayer].frameList[layerList[currentLayer].currentFrame].reset();
            }
        }
        jumpToFrame(layerList[currentLayer].currentFrame);
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

    public void inputPreviousLayer() {
        showOnionSkin = false;
        inputHideFrames();
        currentLayer--;
        if (currentLayer < 0) currentLayer = layerList.Count - 1;
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
        if (sound != null) {
            markTime = Time.realtimeSinceStartup;
            if (layerList[currentLayer].currentFrame * frameInterval <= sound.clip.length) {
                sound.time = layerList[currentLayer].currentFrame * frameInterval;
                sound.Stop();
                sound.Play();
            }
        }
    }

    void doAudioStop() {
        if (sound != null) {
            sound.Stop();
        }
    }

    public void updateCollision() {
        RaycastHit hit;
        Ray ray;

        ray = new Ray(target.position, target.forward);

        if (Physics.Raycast(ray, out hit)) {
            thisHit = hit.point;
        } else {
            thisHit = Vector3.zero;
        }
    }

    public Vector3 getCollision() {
        if (thisHit != Vector3.zero) {
            if (lastHit == Vector3.zero || Vector3.Distance(thisHit, lastHit) < 10f) {// brushSize) {
                lastHit = Vector3.Lerp(thisHit, Camera.main.transform.position, 0.02f);
                return lastHit;
            } else {
                return Vector3.zero;
            }
        } else {
            return Vector3.zero;
        }
    }

    /*
    public Vector3 getCollision() {
        RaycastHit hit;
        Ray ray;

        ray = new Ray(target.position, target.forward);

        if (Physics.Raycast(ray, out hit)) {
            if (lastHit == Vector3.zero || Vector3.Distance(hit.point, lastHit) < (10f * scaleRef.localScale.z)) {// brushSize) {
                //Vector3 cam = Camera.main.ScreenToWorldPoint(hit.point); //applyCameraTransformMatrix(hit.point);
                lastHit = Vector3.Lerp(hit.point, Camera.main.transform.position, 0.02f);
                return lastHit;
            } else {
                return Vector3.zero;
            }
        } else {
            return Vector3.zero;
        }
    }
    */

    public LatkFrame getLastFrame() {
        LatkLayer layer = layerList[currentLayer];
        return layer.frameList[layer.currentFrame];
    }

    public LatkStroke getLastStroke() {
        LatkFrame frame = getLastFrame();
        LatkStroke stroke = frame.brushStrokeList[frame.brushStrokeList.Count - 1];
        return stroke;
    }

    public Vector3 getLastPoint() {
        LatkStroke stroke = getLastStroke();
        return stroke.points[stroke.points.Count - 1];
    }

    JSONNode getJsonFromZip(byte[] bytes) {
        // https://gist.github.com/r2d2rigo/2bd3a1cafcee8995374f

        MemoryStream fileStream = new MemoryStream(bytes, 0, bytes.Length);
        ZipFile zipFile = new ZipFile(fileStream);

        foreach (ZipEntry entry in zipFile) {
            if (Path.GetExtension(entry.Name).ToLower() == ".json") {
                Stream zippedStream = zipFile.GetInputStream(entry);
                StreamReader read = new StreamReader(zippedStream, true);
                string json = read.ReadToEnd();
                Debug.Log(json);
                return JSON.Parse(json);
            }
        }

        return null;
    }

    void saveJsonAsZip(string url, string fileName, string s) {
        // https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
        // https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples
        // https://stackoverflow.com/questions/8624071/save-and-load-memorystream-to-from-a-file

        MemoryStream memStreamIn = new MemoryStream();
        StreamWriter writer = new StreamWriter(memStreamIn);
        writer.Write(s);
        writer.Flush();
        memStreamIn.Position = 0;

        MemoryStream outputMemStream = new MemoryStream();
        ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

        zipStream.SetLevel(4); //0-9, 9 being the highest level of compression

        string fileNameMinusExtension = "";
        string[] nameTemp = fileName.Split('.');
        for (int i = 0; i < nameTemp.Length - 1; i++) {
            fileNameMinusExtension += nameTemp[i];
        }

        ZipEntry newEntry = new ZipEntry(fileNameMinusExtension + ".json");
        newEntry.DateTime = System.DateTime.Now;

        zipStream.PutNextEntry(newEntry);

        StreamUtils.Copy(memStreamIn, zipStream, new byte[4096]);
        zipStream.CloseEntry();

        zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
        zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.

        outputMemStream.Position = 0;

        using (FileStream file = new FileStream(url, FileMode.Create, System.IO.FileAccess.Write)) {
            byte[] bytes = new byte[outputMemStream.Length];
            outputMemStream.Read(bytes, 0, (int)outputMemStream.Length);
            file.Write(bytes, 0, bytes.Length);
            outputMemStream.Close();
        }

        /*
        // Alternative outputs:
        // ToArray is the cleaner and easiest to use correctly with the penalty of duplicating allocated memory.
        byte[] byteArrayOut = outputMemStream.ToArray();

        // GetBuffer returns a raw buffer raw and so you need to account for the true length yourself.
        byte[] byteArrayOut = outputMemStream.GetBuffer();
        long len = outputMemStream.Length;
        */
    }

}
