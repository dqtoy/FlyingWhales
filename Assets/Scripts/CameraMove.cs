using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance = null;

	[SerializeField] private float _minFov;
	[SerializeField] private float _maxFov;
	[SerializeField] private float sensitivity;
    [SerializeField] private Camera nameplateCamera;
    [SerializeField] private Camera _wholeMapCamera;
    [SerializeField] private Camera _uiCamera;

    private float dampTime = 0.2f;
	private Vector3 velocity = Vector3.zero;
	[SerializeField] private Transform target;

    public RenderTexture minimapTexture;

	const float MIN_Z = -10f;
	const float MAX_Z = -10f;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    [SerializeField] internal float MIN_X;
    [SerializeField] internal float MAX_X;
    [SerializeField] internal float MIN_Y;
    [SerializeField] internal float MAX_Y;

    //[SerializeField] private float minXUIAdjustment;
    //[SerializeField] private float maxXUIAdjustment;
    //[SerializeField] private float minYUIAdjustment;
    //[SerializeField] private float maxYUIAdjustment;

    private float previousCameraFOV;

    #region getters/setters
//public MinimapCamera minimap {
//    get { return _minimap; }
//}
    public Camera wholeMapCamera {
        get { return _wholeMapCamera; }
    }
    public Camera uiCamera {
        get { return _uiCamera; }
    }
    public float currentFOV {
        get { return Camera.main.orthographicSize; }
    }
    public float maxFOV {
        get { return _maxFov; }
    }
#endregion

    private void Awake(){
		Instance = this;
	}
    private void Update() {
        float xAxisValue = Input.GetAxis("Horizontal");
        float zAxisValue = Input.GetAxis("Vertical");
#if WORLD_CREATION_TOOL
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
            iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("y", Camera.main.transform.position.y + zAxisValue, "time", 0.1f));
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
            iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("x", Camera.main.transform.position.x + xAxisValue, "time", 0.1f));
        }
#else
        if (!UIManager.Instance.IsConsoleShowing()) {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
                iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("y", Camera.main.transform.position.y + zAxisValue, "time", 0.1f));
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
                iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("x", Camera.main.transform.position.x + xAxisValue, "time", 0.1f));
            }
        }
#endif

        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
#if WORLD_CREATION_TOOL
        if (!worldcreator.WorldCreatorUI.Instance.IsMouseOnUI() && screenRect.Contains(Input.mousePosition)) {
#else
        if (!UIManager.Instance.IsMouseOnUI() && screenRect.Contains(Input.mousePosition)) {
#endif
            //camera scrolling code
            float fov = Camera.main.orthographicSize;
            float adjustment = Input.GetAxis("Mouse ScrollWheel") * (sensitivity * -1f);
            fov += adjustment;
            fov = Mathf.Clamp(fov, _minFov, _maxFov);

            if (!Mathf.Approximately(previousCameraFOV, fov)) {
                previousCameraFOV = fov;
                Camera.main.orthographicSize = fov;
                nameplateCamera.orthographicSize = fov;
#if !WORLD_CREATION_TOOL
                if (GameManager.Instance.gameHasStarted) {
                    CalculateCameraBounds();
                }
#else
                _uiCamera.orthographicSize = fov;
                CalculateCameraBounds();
#endif
            }
        }

#if WORLD_CREATION_TOOL
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)) {
#else
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Minimap.Instance.isDragging) {
#endif
            //reset target when player pushes a button to pan the camera
            target = null;
        }

        if (target) { //smooth camera center
            Vector3 point = Camera.main.WorldToViewportPoint(target.position);
            Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta;
            //float xCoord = Mathf.Clamp(destination.x, MIN_X, MAX_X);
            //float yCoord = Mathf.Clamp(destination.y, MIN_Y, MAX_Y);
            //destination = new Vector3(xCoord, yCoord, destination.z);
            //if (!allowHorizontalMovement) {
            //    xCoord = transform.position.x;
            //}
            //if (!allowVerticalMovement) {
            //    yCoord = transform.position.y;
            //}
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
            if (HasReachedBounds() || (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
                target = null;
            }
        }

        //if (!allowVerticalMovement || !allowHorizontalMovement) {
        //    Vector3 pos = Vector3.zero;
        //    if (!allowVerticalMovement) {
        //        pos.y = 0f + (maxYUIAdjustment/2f);
        //    }
        //    if (!allowHorizontalMovement) {
        //        pos.x = 0f + (maxXUIAdjustment/2f);
        //    }
        //    this.transform.position = pos;
        //}
        ConstrainCameraBounds();
    }

    public void Initialize() {
        Messenger.AddListener(Signals.GAME_LOADED, SetInitialCameraPosition);
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_ADDED, UpdateMinimapTexture);
    }
    private void SetInitialCameraPosition() {
        Vector3 initialPos = Vector3.zero;
        //initialPos.x += maxXUIAdjustment/2f;
        //initialPos.y += maxYUIAdjustment/2f;
        initialPos.z = -10;
        this.transform.position = initialPos;
    }

    public void SetWholemapCameraValues() {
        HexTile topTile = GridMap.Instance.map[0, (int)GridMap.Instance.height - 1];
        float newSize = topTile.transform.position.y + 3;
        wholeMapCamera.orthographicSize = newSize;

        HexTile topCornerHexTile = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height - 1];
        float xSize = Mathf.FloorToInt(topCornerHexTile.transform.localPosition.x + 4f) * 3;
        float zSize = Mathf.FloorToInt(topCornerHexTile.transform.localPosition.y + 4f) * 3;

        int width = (int)Mathf.Min(xSize, Minimap.Instance.maxWidth);
        int height = (int)Mathf.Min(zSize, Minimap.Instance.maxHeight);

        minimapTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
        minimapTexture.Create();
        wholeMapCamera.targetTexture = minimapTexture;
    }
    public void UpdateMinimapTexture() {
        wholeMapCamera.Render();
    }
    public void UpdateMinimapTexture(Area affectedArea, HexTile tile) {
        wholeMapCamera.Render();
    }

    public void MoveMainCamera(Vector2 newPos) {
        Camera.main.transform.position = newPos;
        CameraMove.Instance.ConstrainCameraBounds();
    }

    public void CalculateCameraBounds() {
        //Bounds bounds = new Bounds()
#if WORLD_CREATION_TOOL
        if (worldcreator.WorldCreatorManager.Instance.map == null) {
            return;
        }
        Vector2 topRightCornerCoordinates = worldcreator.WorldCreatorManager.Instance.map[worldcreator.WorldCreatorManager.Instance.width - 1, worldcreator.WorldCreatorManager.Instance.height - 1].transform.localPosition;
        HexTile leftMostTile = worldcreator.WorldCreatorManager.Instance.map[0, worldcreator.WorldCreatorManager.Instance.height / 2];
        HexTile rightMostTile = worldcreator.WorldCreatorManager.Instance.map[worldcreator.WorldCreatorManager.Instance.width - 1, worldcreator.WorldCreatorManager.Instance.height / 2];
        HexTile topMostTile = worldcreator.WorldCreatorManager.Instance.map[worldcreator.WorldCreatorManager.Instance.width/2, worldcreator.WorldCreatorManager.Instance.height - 1];
        HexTile botMostTile = worldcreator.WorldCreatorManager.Instance.map[worldcreator.WorldCreatorManager.Instance.width/2, 0];
#else
        Vector2 topRightCornerCoordinates = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height - 1].transform.localPosition;
        HexTile leftMostTile = GridMap.Instance.map[0, (int)GridMap.Instance.height / 2];
        HexTile rightMostTile = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height / 2];
        HexTile topMostTile = GridMap.Instance.map[(int)GridMap.Instance.width/2, (int)GridMap.Instance.height - 1];
        HexTile botMostTile = GridMap.Instance.map[(int)GridMap.Instance.width/2, 0];
#endif
        float mapX = Mathf.Floor(topRightCornerCoordinates.x);
        float mapY = Mathf.Floor(topRightCornerCoordinates.y);


        float vertExtent = Camera.main.orthographicSize;    
        float horzExtent = vertExtent * Screen.width / Screen.height;
 
         // Calculations assume map is position at the origin
        minX = mapX / 2.0f - horzExtent;
        maxX = horzExtent - mapX / 2.0f;
        minY = mapY / 2.0f - vertExtent;
        maxY = vertExtent - mapY / 2.0f;

        float halfOfHexagon = (256f / 2f) / 100f; //1.28

        //#endif
        if (Utilities.IsPositive(minX)) {
            MIN_X = minX + halfOfHexagon;
        } else {
            MIN_X = minX - halfOfHexagon;
        }

        if (Utilities.IsPositive(maxX)) {
            MAX_X = maxX + halfOfHexagon;
        } else {
            MAX_X = maxX - halfOfHexagon;
        }

        if (Utilities.IsPositive(minY)) {
            MIN_Y = minY + halfOfHexagon;
        } else {
            MIN_Y = minY - halfOfHexagon;
        }

        if (Utilities.IsPositive(maxY)) {
            MAX_Y = maxY + halfOfHexagon;
        } else {
            MAX_Y = maxY - halfOfHexagon;
        }
    }
    public void ConstrainCameraBounds() {
        float xLowerBound = MIN_X;
        float xUpperBound = MAX_X;
        float yLowerBound = MIN_Y;
        float yUpperBound = MAX_Y;
        if (MAX_X < MIN_X) {
            //switch
            xLowerBound = MAX_X;
            xUpperBound = MIN_X;
        }
        if (MAX_Y < MIN_Y) {
            //switch
            yLowerBound = MAX_Y;
            yUpperBound = MIN_Y;
        }
        if (IsWithinBounds(transform.position.x, xLowerBound, xUpperBound) && IsWithinBounds(transform.position.y, yLowerBound, yUpperBound)) {
            return; //already within bounds
        }
        float xCoord = Mathf.Clamp(transform.position.x, xLowerBound, xUpperBound);
        float yCoord = Mathf.Clamp(transform.position.y, yLowerBound, yUpperBound);
        float zCoord = Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z);
        Camera.main.transform.position = new Vector3(
            xCoord,
            yCoord,
            zCoord);
    }
    private Vector3 ConstrainPosition(Vector3 pos) {
        float xCoord = Mathf.Clamp(pos.x, MIN_X, MAX_X);
        float yCoord = Mathf.Clamp(pos.y, MIN_Y, MAX_Y);
        float zCoord = Mathf.Clamp(pos.z, MIN_Z, MAX_Z);
        //if (!allowHorizontalMovement) {
        //    xCoord = pos.x;
        //}
        //if (!allowVerticalMovement) {
        //    yCoord = pos.y;
        //}
        return new Vector3(xCoord, yCoord, zCoord);
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

    private bool IsWithinBounds(float value, float lowerBound, float upperBound) {
        if (value >= lowerBound && value <= upperBound) {
            return true;
        }
        return false;
    }
}
