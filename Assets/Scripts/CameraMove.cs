using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance = null;

	float minFov = 60f;
	float maxFov = 163f;
	float sensitivity = 20f;

	public float dampTime = 0.2f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;

	void Awake(){
		Instance = this;
	}

	void Update () {
		float xAxisValue = Input.GetAxis("Horizontal");
		float zAxisValue = Input.GetAxis("Vertical");
		if(Camera.current != null){
			Camera.main.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
		}

		float fov = Camera.main.fieldOfView;
		fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity;
		fov = Mathf.Clamp(fov, minFov, maxFov);
		Camera.main.fieldOfView = fov;

		if (target) {
			Vector3 point = Camera.main.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
			Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
			if (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y)) {
				target = null;
			}
		}

	}

	public void CenterCameraOn(GameObject GO){
		Camera.main.fieldOfView = 70;
		target = GO.transform;
//		Vector3 diff = Camera.main.ScreenToWorldPoint(GO.transform.position);
//		Camera.main.transform.Translate(new Vector3(diff.x, diff.y, 0.0f));
	}
}
