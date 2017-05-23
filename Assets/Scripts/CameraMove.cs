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
//		if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.W)){
//			this.direction = DIRECTION.UP;
//			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, new Vector3 (Camera.main.transform.position.x + xAxisValue, Camera.main.transform.position.y + zAxisValue, Camera.main.transform.position.z), Time.smoothDeltaTime * this.moveSpeed);
////			Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, new Vector3 (Camera.main.transform.position.x + xAxisValue, Camera.main.transform.position.y + zAxisValue, Camera.main.transform.position.z), ref velocity, dampTime);
////			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, new Vector3 (Camera.main.transform.position.x + xAxisValue, Camera.main.transform.position.y + zAxisValue, Camera.main.transform.position.z), Time.deltaTime * this.moveSpeed);
//		}
//		if (Input.GetKey (KeyCode.DownArrow) || Input.GetKey (KeyCode.S)){
//			this.direction = DIRECTION.DOWN;
//			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, new Vector3 (Camera.main.transform.position.x + xAxisValue, Camera.main.transform.position.y + zAxisValue, Camera.main.transform.position.z), Time.smoothDeltaTime * this.moveSpeed);
//		}
//		if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey (KeyCode.A)){
//			this.direction = DIRECTION.LEFT;
//			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, new Vector3 (Camera.main.transform.position.x + xAxisValue, Camera.main.transform.position.y + zAxisValue, Camera.main.transform.position.z), Time.smoothDeltaTime * this.moveSpeed);
//
//		}
//		if (Input.GetKey (KeyCode.RightArrow) || Input.GetKey (KeyCode.D)){
//			this.direction = DIRECTION.RIGHT;
//			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, new Vector3 (Camera.main.transform.position.x + xAxisValue, Camera.main.transform.position.y + zAxisValue, Camera.main.transform.position.z), Time.smoothDeltaTime * this.moveSpeed);
//		}
		if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.DownArrow) || Input.GetKey (KeyCode.LeftArrow) || Input.GetKey (KeyCode.RightArrow) ||
			Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.D)) {
			iTween.MoveUpdate (Camera.main.gameObject, iTween.Hash("x", Camera.main.transform.position.x + xAxisValue, "y", Camera.main.transform.position.y + zAxisValue, "time", 0.1f));
		}
//		if(Camera.current != null){
//			Camera.main.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
//		}

		if (!UIManager.Instance.IsMouseOnUI ()) {
			float fov = Camera.main.orthographicSize;
			fov += Input.GetAxis ("Mouse ScrollWheel") * (sensitivity * -1f);
			fov = Mathf.Clamp (fov, minFov, maxFov);
			Camera.main.orthographicSize = fov;
			eventIconCamera.orthographicSize = fov;
			resourceIconCamera.orthographicSize = fov;
			generalCamera.orthographicSize = fov;
		}

//		transform.position = new Vector3(
//			Mathf.Clamp(transform.position.x, MIN_X, MAX_X),
//			Mathf.Clamp(transform.position.y, MIN_Y, MAX_Y),
//			Mathf.Clamp(transform.position.z, -10, -10));

		if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow) ||
		   Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D)) {
			target = null;
		}

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
//		Camera.main.orthographicSize = minFov;
//		eventIconCamera.orthographicSize = minFov;
//		resourceIconCamera.orthographicSize = minFov;
//		generalCamera.orthographicSize = minFov;

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
