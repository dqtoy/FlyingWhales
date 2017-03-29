using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance = null;

	float minFov = 60f;
	float maxFov = 163f;
	float sensitivity = 20f;

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

	}

	public void CenterCameraOn(GameObject GO){
		Vector3 diff = Camera.main.ScreenToWorldPoint(GO.transform.position);
		Camera.main.transform.Translate(new Vector3(diff.x, diff.y, 0.0f));
	}
}
