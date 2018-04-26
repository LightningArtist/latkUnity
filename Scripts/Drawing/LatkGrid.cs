using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatkGrid : LatkDrawing {

    public int gridRows = 10;
    public int gridColumns = 10;
    public float gridCell = 0.1f;

    private void Start() {
        if (createOnStart) makeGrid(transform.position.z, gridCell);
    }

    public List<LatkStroke> makeGrid(float zPos, float cell) {
        float xMax = (float) gridRows * cell;
        float yMax = (float) gridColumns * cell;
        float xHalf = xMax / 2f;
        float yHalf = yMax / 2f;

        for (int x = 0; x <= gridRows; x++) {
            float xPos = (float) x * cell;
            strokes.Add(makeLine(new Vector3(-xHalf, xPos - xHalf, zPos), new Vector3(xHalf, xPos - xHalf, zPos)));
        }

        for (int y = 0; y <= gridColumns; y++) {
            float yPos = (float) y * cell;
            strokes.Add(makeLine(new Vector3(yPos - yHalf, -yHalf, zPos), new Vector3(yPos - yHalf, yHalf, zPos)));
        }

        return strokes;
    }

}