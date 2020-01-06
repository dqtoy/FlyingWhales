using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance = null;

	[SerializeField] private float _minFov;
	[SerializeField] private float _maxFov;
	[SerializeField] private float sensitivity;
    [SerializeField] private float _zoomSpeed = 5f;
    public Camera nameplateCamera;
    public Camera areaMapsCamera;
    [SerializeField] private Camera _uiCamera;

    private float dampTime = 0.2f;
	private Vector3 velocity = Vector3.zero;
	[SerializeField] private Transform target;

	const float MIN_Z = -10f;
	const float MAX_Z = -10f;
    [SerializeField] internal float MIN_X;
    [SerializeField] internal float MAX_X;
    [SerializeField] internal float MIN_Y;
    [SerializeField] internal float MAX_Y;

    [SerializeField] private bool allowZoom = true;

    [Header("Dragging")]
    private float dragThreshold = 0.13f;
    [SerializeField] private float currDragTime;
    [SerializeField] private Vector3 dragOrigin;
    public bool isDragging = false;

    [Header("Edging")]
    [SerializeField] private int edgeBoundary = 30;
    [SerializeField] private float edgingSpeed = 30f;
    private bool allowEdgePanning = false;

    private float previousCameraFOV;

    private bool cameraControlEnabled = true;

    int defaultMask;

    #region getters/setters
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
        if (!cameraControlEnabled) {
            return;
        }

#if WORLD_CREATION_TOOL
        ArrowKeysMovement();
        Zooming();
        Targetting();
        ConstrainCameraBounds();
#else
        ArrowKeysMovement();
        Dragging();
        Edging();
        Zooming();
        Targetting();
        ConstrainCameraBounds();
#endif

    }
    private void OnDestroy() {
        RemoveListeners();
    }

    public void Initialize() {
        Messenger.AddListener(Signals.GAME_LOADED, SetInitialCameraPosition);
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
    }

    private void RemoveListeners() {
        Messenger.RemoveListener(Signals.GAME_LOADED, SetInitialCameraPosition);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.RemoveListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
    }

    #region Utilities
    public void ToggleMainCameraLayer(string layerName) {
        Camera.main.cullingMask ^= 1 << LayerMask.NameToLayer(layerName);
        defaultMask = Camera.main.cullingMask;
    }
    public void ZoomToTarget(float targetZoom) {
        StartCoroutine(lerpFieldOfView(Camera.main, targetZoom, 0.1f));
        StartCoroutine(lerpFieldOfView(nameplateCamera, targetZoom, 0.1f));
    }
    private IEnumerator lerpFieldOfView(Camera targetCamera, float toFOV, float duration) {
        float counter = 0;

        float fromFOV = targetCamera.orthographicSize;

        while (counter < duration) {
            counter += Time.deltaTime;

            float fOVTime = counter / duration;
            //Debug.Log(fOVTime);

            //Change FOV
            targetCamera.orthographicSize = Mathf.Lerp(fromFOV, toFOV, fOVTime);
            //Wait for a frame
            yield return null;
        }
    }
    #endregion

    #region Positioning
    private void SetInitialCameraPosition() {
        //Vector3 initialPos = Vector3.zero;
        //initialPos.z = -10;
        Vector3 initialPos = new Vector3(-2.35f, -1.02f, -10f);
        this.transform.position = initialPos;
    }
    public void MoveMainCamera(Vector2 newPos) {
        Camera.main.transform.position = newPos;
        CameraMove.Instance.ConstrainCameraBounds();
    }
    public void CenterCameraOn(GameObject GO) {
        target = GO.transform;
    }
    private void ArrowKeysMovement() {
#if WORLD_CREATION_TOOL
        if (!worldcreator.WorldCreatorUI.Instance.IsUserOnUI()) {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
                float zAxisValue = Input.GetAxis("Vertical");
                iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("y", Camera.main.transform.position.y + zAxisValue, "time", 0.1f));
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
                float xAxisValue = Input.GetAxis("Horizontal");
                iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("x", Camera.main.transform.position.x + xAxisValue, "time", 0.1f));
            }
        }
#else
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
            if (!UIManager.Instance.IsConsoleShowing() && (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() == null)) {  //UIManager.Instance.regionInfoUI.fingersUI.createNewFactionGO.activeSelf
                float zAxisValue = Input.GetAxis("Vertical");
                iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("y", Camera.main.transform.position.y + zAxisValue, "time", 0.1f));
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
            if (!UIManager.Instance.IsConsoleShowing() && (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() == null)) {
                float xAxisValue = Input.GetAxis("Horizontal");
                iTween.MoveUpdate(Camera.main.gameObject, iTween.Hash("x", Camera.main.transform.position.x + xAxisValue, "time", 0.1f));
            }
        }
#endif
    }
    private void Zooming() {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
#if WORLD_CREATION_TOOL
        if (worldcreator.WorldCreatorManager.Instance.isDoneLoadingWorld 
            && screenRect.Contains(Input.mousePosition)) {
#else
        if (allowZoom && screenRect.Contains(Input.mousePosition)) {
#endif
            //camera scrolling code
            float fov = Camera.main.orthographicSize;
            float adjustment = Input.GetAxis("Mouse ScrollWheel") * (sensitivity);
#if WORLD_CREATION_TOOL
            if (adjustment != 0f && !worldcreator.WorldCreatorUI.Instance.IsMouseOnUI()
            && !worldcreator.WorldCreatorUI.Instance.IsUserOnUI()) {
#else
            if (adjustment != 0f && !UIManager.Instance.IsMouseOnUI()) {
#endif
                //Debug.Log(adjustment);
                fov -= adjustment;
                //fov = Mathf.Round(fov * 100f) / 100f;
                fov = Mathf.Clamp(fov, _minFov, _maxFov);

                if (!Mathf.Approximately(previousCameraFOV, fov)) {
                    previousCameraFOV = fov;
                    Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
                    nameplateCamera.orthographicSize = Mathf.Lerp(nameplateCamera.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
#if WORLD_CREATION_TOOL
                _uiCamera.orthographicSize = fov;
#endif
                } else {
                    Camera.main.orthographicSize = fov;
                    nameplateCamera.orthographicSize = fov;
                }
                CalculateCameraBounds();
            }
        }
    }
    private void Targetting() {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || isDragging || Input.GetMouseButtonDown(0)) {
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
    }
    private bool startedOnUI = false;
    private bool hasReachedThreshold = false;
    private Vector3 originMousePos;
    private void Dragging() {
        if (startedOnUI) {
            if (!Input.GetMouseButton(2)) {
                ResetDragValues();
            }
            return;
        }
        if (!isDragging) {
            if (Input.GetMouseButtonDown(2)) {
                if (UIManager.Instance.IsMouseOnUI()) { //if the dragging started on UI, a tileobject or a character, do not allow drag
                    startedOnUI = true;
                    return;
                }
                //dragOrigin = Input.mousePosition; //on first press of mouse
            } else if (Input.GetMouseButton(2)) {
                currDragTime += Time.deltaTime; //while the left mouse button is pressed
                if (currDragTime >= dragThreshold) {
                    if (!hasReachedThreshold) {
                        dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        originMousePos = Input.mousePosition;
                        hasReachedThreshold = true;
                    }
                    if (originMousePos != Input.mousePosition) { //check if the mouse has moved position from the origin, only then will it be considered dragging
                        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Drag_Clicked);
                        isDragging = true;
                    }
                }
            }

        }


        if (isDragging) {
            Vector3 difference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            Camera.main.transform.position = dragOrigin - difference;
            if (Input.GetMouseButtonUp(2)) {
                ResetDragValues();
                CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
            }
        } else {
            if (!Input.GetMouseButton(2)) {
                currDragTime = 0f;
                hasReachedThreshold = false;
            }
        }
    }
    private void ResetDragValues() {
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        currDragTime = 0f;
        isDragging = false;
        startedOnUI = false;
        hasReachedThreshold = false;
    }
    private void Edging() {
        if (!allowEdgePanning || isDragging) {
            return;
        }
        bool isEdging = false;
        Vector3 newPos = transform.position;
        if (Input.mousePosition.x > Screen.width - edgeBoundary) {
            newPos.x += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.x < 0 + edgeBoundary) {
            newPos.x -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }

        if (Input.mousePosition.y > Screen.height - edgeBoundary) {
            newPos.y += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.y < 0 + edgeBoundary) {
            newPos.y -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (isEdging) {
            target = null; //reset target
        }
        transform.position = newPos;
    }
    public void AllowEdgePanning(bool state) {
        allowEdgePanning = state;
    }
    #endregion

    #region Bounds
    public void CalculateCameraBounds() {
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
        if (GridMap.Instance.map == null) {
            return;
        }
        Vector2 topRightCornerCoordinates = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height - 1].transform.localPosition;
        //HexTile leftMostTile = GridMap.Instance.map[0, (int)GridMap.Instance.height / 2];
        HexTile rightMostTile = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height / 2];
        HexTile topMostTile = GridMap.Instance.map[(int)GridMap.Instance.width/2, (int)GridMap.Instance.height - 1];
        //HexTile botMostTile = GridMap.Instance.map[(int)GridMap.Instance.width/2, 0];
#endif
        //float mapX = Mathf.Floor(topRightCornerCoordinates.x);
        //float mapY = Mathf.Floor(topRightCornerCoordinates.y);

        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        Bounds newBounds = new Bounds();
#if WORLD_CREATION_TOOL
        newBounds.extents = new Vector3(Mathf.Abs(rightMostTile.transform.position.x) + (1.28f * (float)(worldcreator.WorldCreatorManager.Instance._borderThickness + 2f)), Mathf.Abs(topMostTile.transform.position.y) + (1.28f * 4f), 0f);
#else
        newBounds.extents = new Vector3(Mathf.Abs(rightMostTile.transform.position.x), Mathf.Abs(topMostTile.transform.position.y), 0f);
#endif
        SetCameraBounds(newBounds, horzExtent, vertExtent);

        //if (Utilities.IsVisibleFrom(leftMostTile.gameObject.GetComponentInChildren<Renderer>(), Camera.main)
        //    && Utilities.IsVisibleFrom(rightMostTile.gameObject.GetComponentInChildren<Renderer>(), Camera.main)) {
        //    allowHorizontalMovement = false;
        //} else {
        //    allowHorizontalMovement = true;
        //}

        //if (Utilities.IsVisibleFrom(topMostTile.gameObject.GetComponentInChildren<Renderer>(), Camera.main)
        //    && Utilities.IsVisibleFrom(botMostTile.gameObject.GetComponentInChildren<Renderer>(), Camera.main)) {
        //    allowVerticalMovement = false;
        //} else {
        //    allowVerticalMovement = true;
        //}
    }
    public void ConstrainCameraBounds() {
        float xLowerBound = MIN_X;
        float xUpperBound = MAX_X;
        float yLowerBound = MIN_Y;
        float yUpperBound = MAX_Y;
        if (MAX_X < MIN_X) {
            xLowerBound = MAX_X;
            xUpperBound = MIN_X;
        }
        if (MAX_Y < MIN_Y) {
            yLowerBound = MAX_Y;
            yUpperBound = MIN_Y;
        }
        float xCoord = Mathf.Clamp(transform.position.x, xLowerBound, xUpperBound);
        float yCoord = Mathf.Clamp(transform.position.y, yLowerBound, yUpperBound);
        float zCoord = Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z);
        //if (!allowHorizontalMovement) {
        //    xCoord = 0f;
        //}
        //if (!allowVerticalMovement) {
        //    yCoord = 0f;
        //}
        //if (!allowVerticalMovement && !allowHorizontalMovement) {
        //    xCoord = 0f;
        //    yCoord = 0f;
        //}
        Camera.main.transform.position = new Vector3(
            xCoord,
            yCoord,
            zCoord);
    }
    private bool HasReachedBounds() {
        if ((Mathf.Approximately(transform.position.x, MAX_X) || Mathf.Approximately(transform.position.x, MIN_X)) &&
                (Mathf.Approximately(transform.position.y, MAX_Y) || Mathf.Approximately(transform.position.y, MIN_Y))) {
            return true;
        }
        return false;
    }
    private bool IsWithinBounds(float value, float lowerBound, float upperBound) {
        if (value >= lowerBound && value <= upperBound) {
            return true;
        }
        return false;
    }
    private void SetCameraBounds(Bounds bounds, float horzExtent, float vertExtent) {
        //boundDrawer.bounds = bounds;
        float halfOfHexagon = (256f) / 100f; //1.28
#if WORLD_CREATION_TOOL
        int borderCount = worldcreator.WorldCreatorManager.Instance._borderThickness;
#else
        int borderCount = GridMap.Instance._borderThickness;
#endif
        MIN_X = bounds.min.x + horzExtent - (halfOfHexagon * ((float)borderCount));
        MAX_X = bounds.max.x - horzExtent + (halfOfHexagon * (borderCount)); //removed -1 because of UI
        MIN_Y = bounds.min.y + vertExtent - (halfOfHexagon * ((float)borderCount - 2));
        MAX_Y = bounds.max.y - vertExtent + (halfOfHexagon * (borderCount - 2));
    }
    private Vector2[] GetCameraWorldCorners(Camera camera) {
        Vector2[] corners = new Vector2[4]; //4 corners

        // Screens coordinate corner location
        var upperLeftScreen = new Vector2(0, Screen.height);
        var upperRightScreen = new Vector2(Screen.width, Screen.height);
        var lowerLeftScreen = new Vector2(0, 0);
        var lowerRightScreen = new Vector2(Screen.width, 0);

        //Corner locations in world coordinates
        var upperLeft = camera.ScreenToWorldPoint(upperLeftScreen);
        var upperRight = camera.ScreenToWorldPoint(upperRightScreen);
        var lowerRight = camera.ScreenToWorldPoint(lowerRightScreen);
        var lowerLeft = camera.ScreenToWorldPoint(lowerLeftScreen);

        corners[0] = upperLeft;
        corners[1] = upperRight;
        corners[2] = lowerRight;
        corners[3] = lowerLeft;

        return corners;
    }
    #endregion

    #region Listeners
    private float lastZoomAmount = 0f;
    private void OnMenuOpened(UIMenu openedMenu) {
        if (openedMenu is LandmarkInfoUI) {
            allowZoom = false;
            if (!Mathf.Approximately(Camera.main.orthographicSize, 6.5f)) {
                lastZoomAmount = Camera.main.orthographicSize;
                ZoomToTarget(6.5f);
            }
        }
    }
    private void OnMenuClosed(UIMenu openedMenu) {
        if (openedMenu is LandmarkInfoUI) {
            allowZoom = true;
            ZoomToTarget(lastZoomAmount);
        }
    }
    private void OnAreaMapOpened(Area area) {
        Camera.main.cullingMask = 0;
        SetCameraControlState(false);
    }
    private void OnAreaMapClosed(Area area) {
        Camera.main.cullingMask = defaultMask;
        SetCameraControlState(true);
    }
    #endregion

    #region Camera Control
    public void SetCameraControlState(bool state) {
        cameraControlEnabled = state;
    }
    #endregion
}
