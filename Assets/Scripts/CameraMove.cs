using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

	float minFov = 60f;
	float maxFov = 163f;
	float sensitivity = 20f;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		float xAxisValue = Input.GetAxis("Horizontal");
		float zAxisValue = Input.GetAxis("Vertical");
		if(Camera.current != null)
		{
			Camera.main.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
		}

		float fov = Camera.main.fieldOfView;
		fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity;
		fov = Mathf.Clamp(fov, minFov, maxFov);
		Camera.main.fieldOfView = fov;
	}
}
