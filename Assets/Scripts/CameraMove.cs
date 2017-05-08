using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance = null;

//	float minFov = 60f;
//	float maxFov = 150f;
//	float sensitivity = 20f;

	float minFov = 7f;
	float maxFov = 54f;
	float sensitivity = 5f;


	public float dampTime = 0.2f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;

	public Camera eventIconCamera;
	public Camera resourceIconCamera;
	public Camera generalCamera;
    public Camera traderCamera;

    private float MIN_X = 66f;
	private float MAX_X = 126f;
	private float MIN_Y = 36f;
	private float MAX_Y = 92f;

	void Awake(){
		Instance = this;
	}

	void Update () {
		float xAxisValue = Input.GetAxis("Horizontal");
		float zAxisValue = Input.GetAxis("Vertical");
		if(Camera.current != null){
			Camera.main.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
		}

		float fov = Camera.main.orthographicSize;
		fov += Input.GetAxis("Mouse ScrollWheel") * (sensitivity * -1f);
		fov = Mathf.Clamp(fov, minFov, maxFov);
		Camera.main.orthographicSize = fov;
		eventIconCamera.orthographicSize = fov;
		resourceIconCamera.orthographicSize = fov;
		generalCamera.orthographicSize = fov;

//		transform.position = new Vector3(
//			Mathf.Clamp(transform.position.x, MIN_X, MAX_X),
//			Mathf.Clamp(transform.position.y, MIN_Y, MAX_Y),
//			Mathf.Clamp(transform.position.z, -10, -10));

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

	public void ShowWholeMap(){
		CenterCameraOn (GridMap.Instance.map [25, 25].gameObject);
		Camera.main.orthographicSize = maxFov;
		eventIconCamera.orthographicSize = maxFov;
		resourceIconCamera.orthographicSize = maxFov;
		generalCamera.orthographicSize = maxFov;

	}

	public void CenterCameraOn(GameObject GO){
		Camera.main.orthographicSize = minFov;
		eventIconCamera.orthographicSize = minFov;
		resourceIconCamera.orthographicSize = minFov;
		generalCamera.orthographicSize = minFov;

		target = GO.transform;
//		Vector3 diff = Camera.main.ScreenToWorldPoint(GO.transform.position);
//		Camera.main.transform.Translate(new Vector3(diff.x, diff.y, 0.0f));
	}

	public void ToggleResourceIcons(){
		resourceIconCamera.gameObject.SetActive(!resourceIconCamera.gameObject.activeSelf);
	}

    public void ToggleGeneralCamera(){
        generalCamera.gameObject.SetActive(!generalCamera.gameObject.activeSelf);
    }

    public void ToggleTraderCamera() {
        traderCamera.gameObject.SetActive(!traderCamera.gameObject.activeSelf);
    }
}
