using UnityEngine;
using System.Collections;

public class LAMoveWasd : MonoBehaviour {
	
	public float walkSpeed = 2.0f;
	public float runSpeed = 20.0f;
	public float accel = 0.002f;
	//public float turnSpeed = 200.0f;
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
		p.y = 0.0f;
		p.z = Input.GetAxis("Vertical") * Time.deltaTime * currentSpeed;

		//float mX = Input.GetAxis("Mouse X") * Time.deltaTime * turnSpeed;
		//float mY = Input.GetAxis("Mouse Y") * Time.deltaTime * turnSpeed;

		transform.Translate(p.x, p.y, p.z);

		//transform.Rotate(mY, mX, 0.0f);
		//Debug.Log(mX + " " + mY);	

		if (Input.GetKeyDown(KeyCode.Home)) {
			transform.position = homePoint;
		}
	}

}
