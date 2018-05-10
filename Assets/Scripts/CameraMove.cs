using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance = null;

	//	float minFov = 60f;
	//	float maxFov = 150f;
	//	float sensitivity = 20f;

	[SerializeField] private float _minFov;
	[SerializeField] private float _maxFov;
	[SerializeField] private float sensitivity;
	[SerializeField] private Camera resourceIconCamera;
    [SerializeField] private Camera nameplateCamera;
    [SerializeField] private Camera _wholeMapCamera;
    //[SerializeField] private MinimapCamera _minimap;

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

	[SerializeField] internal float MIN_X;
    [SerializeField] internal float MAX_X;
    [SerializeField] internal float MIN_Y;
    [SerializeField] internal float MAX_Y;

    private float previousCameraFOV;

    #region getters/setters
    //public MinimapCamera minimap {
    //    get { return _minimap; }
    //}
    public Camera wholeMapCamera {
        get { return _wholeMapCamera; }
    }
    public float currentFOV {
        get { return Camera.main.orthographicSize; }
    }
    public float maxFOV {
        get { return _maxFov; }
    }
    #endregion

    void Awake(){
		Instance = this;
		MIN_X = minMIN_X;
		MAX_X = minMAX_X;
		MIN_Y = minMIN_Y;
		MAX_Y = minMAX_Y;
	}

    public void SetWholemapCameraValues() {
        HexTile topTile = GridMap.Instance.map[0, (int)GridMap.Instance.height - 1];
        float newSize = topTile.transform.position.y + 3;
        wholeMapCamera.orthographicSize = newSize;
        //HexTile centerTile = GridMap.Instance.map[(int)(GridMap.Instance.width / 2), (int)(GridMap.Instance.height / 2)];
        //wholeMapCamera.transform.localPosition = new Vector3(centerTile.transform.localPosition.x, wholeMapCamera.transform.localPosition.y, -186);
    }

    public void MoveMainCamera(Vector2 newPos) {
        Camera.main.transform.position = newPos;
        CameraMove.Instance.ConstrainCameraBounds();
        //UpdateMinimapTexture();
    }

    public void UpdateMinimapTexture() {
        wholeMapCamera.Render();
    }

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    public void CalculateCameraBounds() {
        Vector2 topRightCornerCoordinates = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height - 1].transform.localPosition;
        float mapX = Mathf.Floor(topRightCornerCoordinates.x);
        float mapY = Mathf.Floor(topRightCornerCoordinates.y);

         float vertExtent = Camera.main.orthographicSize;    
         float horzExtent = vertExtent * Screen.width / Screen.height;
 
         // Calculations assume map is position at the origin
         minX = horzExtent - mapX / 2.0f;
         maxX = mapX / 2.0f - horzExtent;
         minY = vertExtent - mapY / 2.0f;
         maxY = mapY / 2.0f - vertExtent;

        //MIN_X = minX - 2.5f;
        //MAX_X = maxX + 15f;
        //MIN_Y = minY - 4.5f;
        //MAX_Y = maxY + 3f;

        float halfOfHexagon = (256f / 2f) / 100f;

        MIN_X = minX - (halfOfHexagon * 1.8f);
        MAX_X = maxX + (halfOfHexagon * 9.5f);
        MIN_Y = minY - (halfOfHexagon * 2f);
        MAX_Y = maxY + halfOfHexagon;

        //MIN_X = minX;
        //MAX_X = maxX;
        //MIN_Y = minY;
        //MAX_Y = maxY;

        //MIN_X = minX - 0.6f;
        //MAX_X = maxX - 0.6f;
        //MIN_Y = minY - 3.5f;
        //MAX_Y = maxY - 0.5f;
    }

	void Update () {
		float xAxisValue = Input.GetAxis("Horizontal");
		float zAxisValue = Input.GetAxis("Vertical");
        if (!UIManager.Instance.IsConsoleShowing()) {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) {
                iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("x", Camera.main.transform.position.x + xAxisValue, "y", Camera.main.transform.position.y + zAxisValue, "time", 0.1f));
            }
        }
		Rect screenRect = new Rect(0,0, Screen.width, Screen.height);
		if (!UIManager.Instance.IsMouseOnUI () && screenRect.Contains(Input.mousePosition)) {
			//camera scrolling code
			float fov = Camera.main.orthographicSize;
			float adjustment = Input.GetAxis ("Mouse ScrollWheel") * (sensitivity * -1f);
			fov += adjustment;
			fov = Mathf.Clamp (fov, _minFov, _maxFov);

            if(!Mathf.Approximately(previousCameraFOV, fov)) {
                //if(fov < (_maxFov / 2f)) {
                //    SetBiomeDetailsState(true);
                //} else {
                //    SetBiomeDetailsState(false);
                //}
                previousCameraFOV = fov;
                Camera.main.orthographicSize = fov;
                resourceIconCamera.orthographicSize = fov;
                nameplateCamera.orthographicSize = fov;
                Minimap.Instance.UpdateCameraBorderScale();
                CalculateCameraBounds();
            }
		}

		if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow) ||
			Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D) || Minimap.Instance.isDragging) {
			//reset target when player pushes a button to pan the camera
			target = null;
		}

		if (target) { //smooth camera center
			Vector3 point = Camera.main.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
			Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
			if (HasReachedBounds() || (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
				target = null;
			}
		}

        ConstrainCameraBounds();
    }

    public void ConstrainCameraBounds() {
        Camera.main.transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, MIN_X, MAX_X),
            Mathf.Clamp(transform.position.y, MIN_Y, MAX_Y),
            Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z));
    }

    private bool HasReachedBounds() {
        if ((Mathf.Approximately(transform.position.x, MAX_X) || Mathf.Approximately(transform.position.x, MIN_X)) &&
                (Mathf.Approximately(transform.position.y, MAX_Y) || Mathf.Approximately(transform.position.y, MIN_Y))) {
            return true;
        }
        return false;
    }

	public void CenterCameraOn(GameObject GO){
        if(GO == null) {
            target = null;
        } else {
            target = GO.transform;
        }
	}

    public void ToggleMainCameraLayer(string layerName) {
        Camera.main.cullingMask ^= 1 << LayerMask.NameToLayer(layerName);
    }

    private void SetBiomeDetailsState(bool state) {
        for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
            HexTile currHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
            currHexTile.SetBiomeDetailState(state);
        }
    }
}
