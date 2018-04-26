using UnityEngine;
using System.Collections;

public class ParticleTrigger : MonoBehaviour {

	public LightningArtist latk;
	public ParticleSystem ps;
    public float startSize = 0.005f;
    public Transform scaleRef;

    [HideInInspector] public ParticleSystem.MainModule psMain;
    [HideInInspector] public ParticleSystem.EmissionModule psEm;

    void Awake() {
		if (ps == null)	ps = GetComponent<ParticleSystem> ();
		psEm = ps.emission;
        psMain = ps.main;
	}

	void Start() {
		psEm.enabled = false;
	}
	
	void Update() {
        psMain.startSize = startSize * scaleRef.localScale.x;
		psEm.enabled = latk.clicked;
	}

}
