using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class LatkSpriteToBrush : MonoBehaviour {

	public bool hideOriginal = true;
	public bool spriteIsPacked = false;
	public SpriteRenderer sourceSprite;
	//public Renderer[] destMesh;
	public LightningArtist latk;

	private bool firstRun = true;

	void Start() {
		sourceSprite = GetComponent<SpriteRenderer>();
		if (hideOriginal) sourceSprite.enabled = false;
	}
	
	void Update() {
		/*
		if (firstRun) {
			firstRun = false;
		} else {
			for (int i = 0; i < latk.frameList[latk.currentFrame].brushStrokeList.Count; i++) { 
				if (latk.frameList[latk.currentFrame].brushStrokeList.Count > 0 && latk.frameList[latk.currentFrame].brushStrokeList[i].lineRenderer != null) {
					if (spriteIsPacked) {
						latk.frameList[latk.currentFrame].brushStrokeList[i].lineRenderer.sharedMaterial.mainTexture = grabPackedSprite (sourceSprite.sprite);
					} else {
						latk.frameList[latk.currentFrame].brushStrokeList[i].lineRenderer.sharedMaterial.mainTexture = sourceSprite.sprite.texture;
					}
				}
			}
		}
		*/
	}

	Texture2D grabPackedSprite(Sprite _sprite) {
		// Make sure read/write is enabled in Advanced import settings
		Texture2D tex = new Texture2D((int) _sprite.rect.width, (int) _sprite.rect.height);
		Color[] pixels = _sprite.texture.GetPixels((int) _sprite.textureRect.x, (int) _sprite.textureRect.y, (int) _sprite.textureRect.width, (int) _sprite.textureRect.height);
		tex.SetPixels(pixels);
		tex.Apply();
		return tex;
	}

}
