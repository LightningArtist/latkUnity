using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class SpriteToBrush : MonoBehaviour {

	public bool hideOriginal = true;
	public bool spriteIsPacked = false;
	public SpriteRenderer sourceSprite;
	//public Renderer[] destMesh;
	public LightningArtist lightningArtist;

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
			for (int i = 0; i < lightningArtist.frameList[lightningArtist.currentFrame].brushStrokeList.Count; i++) { 
				if (lightningArtist.frameList[lightningArtist.currentFrame].brushStrokeList.Count > 0 && lightningArtist.frameList[lightningArtist.currentFrame].brushStrokeList[i].lineRenderer != null) {
					if (spriteIsPacked) {
						lightningArtist.frameList[lightningArtist.currentFrame].brushStrokeList[i].lineRenderer.sharedMaterial.mainTexture = grabPackedSprite (sourceSprite.sprite);
					} else {
						lightningArtist.frameList[lightningArtist.currentFrame].brushStrokeList[i].lineRenderer.sharedMaterial.mainTexture = sourceSprite.sprite.texture;
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
