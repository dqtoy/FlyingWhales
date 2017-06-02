using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance = null;

	//	float minFov = 60f;
	//	float maxFov = 150f;
	//	float sensitivity = 20f;

	[SerializeField] private float minFov;
	[SerializeField] private float maxFov;
	[SerializeField] private float sensitivity;
	[SerializeField] private Camera eventIconCamera;
	[SerializeField] private Camera resourceIconCamera;
	[SerializeField] private Camera generalCamera;
	[SerializeField] private Camera traderCamera;

	private float dampTime = 0.2f;
	private Vector3 velocity = Vector3.zero;
	private Transform target;

	//default camera bounds when fov is at minimum
	const float minMIN_X = 12.5f;
	const float minMAX_X = 189.5f;
	const float minMIN_Y = 6.5f;
	const float minMAX_Y = 143.5f;

	//default camera bounds when fov is at maximum
	const float maxMIN_X = 44f;
	const float maxMAX_X = 158f;
	const float maxMIN_Y = 24.5f;
	const float maxMAX_Y = 126f;

	const float MIN_Z = -10f;
	const float MAX_Z = -10f;

	private float MIN_X;
	private float MAX_X;
	private float MIN_Y;
	private float MAX_Y;

	void Awake(){
		Instance = this;
		MIN_X = minMIN_X;
		MAX_X = minMAX_X;
		MIN_Y = minMIN_Y;
		MAX_Y = minMAX_Y;
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

		Rect screenRect = new Rect(0,0, Screen.width, Screen.height);
		if (!UIManager.Instance.IsMouseOnUI () && screenRect.Contains(Input.mousePosition)) {
			//camera scrolling code
			float fov = Camera.main.orthographicSize;
			float adjustment = Input.GetAxis ("Mouse ScrollWheel") * (sensitivity * -1f);
			fov += adjustment;
			fov = Mathf.Clamp (fov, minFov, maxFov);
			Camera.main.orthographicSize = fov;
			eventIconCamera.orthographicSize = fov;
			resourceIconCamera.orthographicSize = fov;
			generalCamera.orthographicSize = fov;

			//adjust camera movement clamps
			if (adjustment > 0f) {
				MAX_X -= .9f;
				MAX_X = Mathf.Clamp (MAX_X, maxMAX_X, minMAX_X);

				MIN_X += .9f;
				MIN_X = Mathf.Clamp (MIN_X, minMIN_X, maxMIN_X);

				MAX_Y -= .5f;
				MAX_Y = Mathf.Clamp (MAX_Y, maxMAX_Y, minMAX_Y);

				MIN_Y += .5f;
				MIN_Y = Mathf.Clamp (MIN_Y, minMIN_Y, maxMIN_Y);
			} else if (adjustment < 0f) {
				MAX_X += .9f;
				MAX_X = Mathf.Clamp (MAX_X, maxMAX_X, minMAX_X);

				MIN_X -= .9f;
				MIN_X = Mathf.Clamp (MIN_X, minMIN_X, maxMIN_X);

				MAX_Y += .5f;
				MAX_Y = Mathf.Clamp (MAX_Y, maxMAX_Y, minMAX_Y);

				MIN_Y -= .5f;
				MIN_Y = Mathf.Clamp (MIN_Y, minMIN_Y, maxMIN_Y);
			}

		}

//		float xAxisValue = Input.GetAxis("Horizontal");
//		float zAxisValue = Input.GetAxis("Vertical");
//		if(Camera.current != null){
//			//camera movement code
//			Camera.main.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
//			Camera.main.transform.position = new Vector3(
//				Mathf.Clamp(transform.position.x, MIN_X, MAX_X),
//				Mathf.Clamp(transform.position.y, MIN_Y, MAX_Y),
//				Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z));
//		}

		if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow) ||
			Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D)) {
			//reset target when player pushes a button to pan the camera
			target = null;
		}

		if (target) { //smooth camera center
			Vector3 point = Camera.main.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
			Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
			if (Mathf.Approximately(transform.position.x, MAX_X) || Mathf.Approximately(transform.position.x, MIN_X) || 
				Mathf.Approximately(transform.position.y, MAX_Y) || Mathf.Approximately(transform.position.y, MIN_Y) ||
				(Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
				target = null;
			}
		}

		Camera.main.transform.position = new Vector3(
			Mathf.Clamp(transform.position.x, MIN_X, MAX_X),
			Mathf.Clamp(transform.position.y, MIN_Y, MAX_Y),
			Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z));

	}

	public void CenterCameraOn(GameObject GO){
		target = GO.transform;
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
