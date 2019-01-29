using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour {

    public static Minimap Instance = null;

    public bool isDragging = false;

    [SerializeField] private RawImage minimapTexture;
    //[SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimapTransform;
    [SerializeField] private RectTransform cameraBorders;

    public float maxWidth;
    public float maxHeight;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    private float xMagicNum;
    private float yMagicNum;

    private void Awake() {
        Instance = this;
    }

    internal void Initialize() {
        //Compute the magic numbers
        float minimapMaxXBounds = minimapTexture.rectTransform.rect.width / 2f;
        float minimapMaxYBounds = minimapTexture.rectTransform.rect.height / 2f;

        HexTile topRightHexTile = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height - 1];
        xMagicNum = minimapMaxXBounds / topRightHexTile.transform.position.x;
        yMagicNum = minimapMaxYBounds / topRightHexTile.transform.position.y;

        //minimapTexture.texture = CameraMove.Instance.minimapTexture;
        //minimapTexture.SetNativeSize();
    }

    //public void OnClickMinimap() {
    //    Vector2 localPoint;
    //    //This returns a screen point relative to the size of the image, with 0,0 at the center of the image and half of the width and height as the left, right, top and bottom bounds
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapTransform, Input.mousePosition, CameraMove.Instance.uiCamera, out localPoint);
    //    //Debug.Log(localPoint);
    //    Vector3 targetPos = new Vector3(localPoint.x / xMagicNum, localPoint.y / yMagicNum, -10f);
    //    CameraMove.Instance.CenterCameraOn(null);
    //    CameraMove.Instance.MoveMainCamera(targetPos);
    //}

    public void OnPointerClickWithBaseData(BaseEventData data) {
        isDragging = true;
        PointerEventData ped = (PointerEventData)data;
        Vector2 localPoint = GetLocalCursorPoint(ped);
        //Debug.Log(localPoint);
        CameraMove.Instance.MoveMainCamera(new Vector3(localPoint.x / xMagicNum, localPoint.y / yMagicNum, -10f));
        //OnClickMinimap();
    }

    //void OnDrag() {
    //    isDragging = true;
    //    OnClickMinimap();
    //}

    //void OnDragEnd() {
    //    isDragging = false;
    //}

    public void OnDragFinish(BaseEventData data) {
        isDragging = false;
    }

    private Vector2 GetLocalCursorPoint(PointerEventData ped) {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapTransform, ped.position, ped.pressEventCamera, out localCursor))
            return Vector2.zero;
        return localCursor;
    }

    private void Update() {
        ComputeBounds();
        //Vector3 mainCameraPosInWorld = RectTransformUtility.WorldToScreenPoint(CameraMove.Instance.wholeMapCamera, Camera.main.transform.position);
        //Vector2 newPos;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapTransform, mainCameraPosInWorld, CameraMove.Instance.uiCamera, out newPos);
        Vector3 newPos = Camera.main.transform.position;
        newPos.x *= xMagicNum;
        newPos.y *= yMagicNum;

        cameraBorders.transform.localPosition = newPos;

        ConstrainBounds();
    }

    public void UpdateCameraBorderScale() {
        float newScale = CameraMove.Instance.currentFOV / CameraMove.Instance.maxFOV;
        cameraBorders.transform.localScale = new Vector3(newScale, newScale, 1f);
        ComputeBounds();
    }
    

    private void ComputeBounds() {
        float minimapTextureWidth = minimapTransform.rect.width;
        float minimapTextureHeight = minimapTransform.rect.height;

        float cameraBordersWidth = cameraBorders.sizeDelta.x * cameraBorders.transform.localScale.x;
        float cameraBordersHeight = cameraBorders.sizeDelta.y * cameraBorders.transform.localScale.y;

        maxX = (minimapTextureWidth - cameraBordersWidth) / 2f;
        minX = maxX * -1f;
        maxY = (minimapTextureHeight - cameraBordersHeight) / 2f;
        minY = maxY * -1f;
    }

    private void ConstrainBounds() {
        cameraBorders.transform.localPosition = new Vector3(
            Mathf.Clamp(cameraBorders.transform.localPosition.x, minX, maxX),
            Mathf.Clamp(cameraBorders.transform.localPosition.y, minY, maxY),
            cameraBorders.transform.localPosition.z);
    }
}
