using UnityEngine;
using System.Collections;

public class LatkMoveWasd : MonoBehaviour {
	
	public float walkSpeed = 2.0f;
	public float runSpeed = 20.0f;
	public float accel = 0.002f;
    public bool useYAxis = false;
    public string yAxisName = "Vertical2";
    public Vector3 homePoint = new Vector3(0,0,0);

	private float currentSpeed;
	private Vector3 p = new Vector3(0,0,0);
	private bool run = false;

	void Start() {
		currentSpeed = walkSpeed;
	}
	
	void Update() {
		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			run = true;
		} else if (Input.GetKeyUp(KeyCode.LeftShift)) {
			run = false;
		}

		if (run && currentSpeed < runSpeed) {
			currentSpeed += accel;
			if (currentSpeed > runSpeed) currentSpeed = runSpeed;
		} else if (!run && currentSpeed > walkSpeed) {
			currentSpeed -= accel;
			if (currentSpeed < walkSpeed) currentSpeed = walkSpeed;
		}

		p.x = Input.GetAxis("Horizontal") * Time.deltaTime * currentSpeed;
        if (useYAxis) {
            p.y = Input.GetAxis(yAxisName) * Time.deltaTime * currentSpeed;
        } else {
            p.y = 0f;
        }
        p.z = Input.GetAxis("Vertical") * Time.deltaTime * currentSpeed;

		transform.Translate(p.x, p.y, p.z);

		if (Input.GetKeyDown(KeyCode.Home)) {
			transform.position = homePoint;
		}
	}

}
