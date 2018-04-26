using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkSkeleton : LatkDrawing {

    public LatkBone[] brushBones;
    public int numStrokes = 10;
    public float spread = 0.25f;
    public float fps = 12f;
    public float smear = 1f;

    void Awake() {
		for (int i=0; i<brushBones.Length; i++) {
            brushBones[i].brushMat = new Material[brushMat.Length];
            for (int j=0; j<brushMat.Length; j++) {
                brushBones[i].brushMat[j] = brushMat[j];
            }

            if (!brushBones[i].overrideBrush) {
                brushBones[i].fps = fps;
                brushBones[i].brushSize = brushSize;
                brushBones[i].spread = spread;
                brushBones[i].color = color;
                brushBones[i].numStrokes = numStrokes;
                brushBones[i].brushMode = brushMode;
                brushBones[i].smear = smear;
            }
        }
    }
	
}
