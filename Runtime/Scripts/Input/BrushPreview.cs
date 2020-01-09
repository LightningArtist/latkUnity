using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushPreview : MonoBehaviour {

    public LightningArtist latk;
    //public Transform target;

    private Renderer ren;
    private TrailRenderer tRen;
    private Shader shader1, shader2;
    private LightningArtist.BrushMode lastBrushMode;

    private void Awake() {
        ren = GetComponent<Renderer>();
        tRen = GetComponent<TrailRenderer>();

        shader1 = Shader.Find("Unlit/Color");
        shader2 = Shader.Find("Standard");
    }

	private void Update() {
        if (latk.brushMode != lastBrushMode) {
            if (latk.brushMode == LightningArtist.BrushMode.ADD) {
                ren.material.shader = shader1;
                tRen.material.shader = shader1;
            } else {
                ren.material.shader = shader2;
                tRen.material.shader = shader2;
            }
            lastBrushMode = latk.brushMode;
        }

        ren.sharedMaterial.SetColor("_Color", latk.mainColor);
        tRen.sharedMaterial.SetColor("_Color", latk.mainColor);

        tRen.widthMultiplier = latk.brushSize;

        transform.localScale = Vector3.one * latk.brushSize;
        if (latk.useCollisions) {
            if (latk.thisHit != Vector3.zero) {
                transform.position = latk.thisHit;
                ren.enabled = true;
                tRen.enabled = true;
            } else {
                transform.localPosition = Vector3.zero;
                ren.enabled = false;
                tRen.enabled = false;
            }
        } else {
            transform.localPosition = Vector3.zero;
            ren.enabled = true;
            tRen.enabled = true;
        }
	}

}
