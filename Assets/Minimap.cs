using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour {

    public static Minimap Instance = null;

    public bool isDragging = false;

    [SerializeField] private RectTransform minimapTransform;
    [SerializeField] private RectTransform cameraBordersTransform;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    private void Awake() {
        Instance = this;
    }

    public void OnPointerClickWithBaseData(BaseEventData data) {
        isDragging = true;
        PointerEventData ped = (PointerEventData)data;
        CameraMove.Instance.MoveMainCamera(GetLocalCursorPoint(ped));
    }

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
        cameraBordersTransform.localPosition = Camera.main.transform.position;
        ConstrainBounds();
    }

    public void UpdateCameraBorderScale() {
        float newScale = CameraMove.Instance.currentFOV / CameraMove.Instance.maxFOV;
        cameraBordersTransform.localScale = new Vector3(newScale, newScale, 1f);
        ComputeBounds();
    }
    

    private void ComputeBounds() {
        float minimapTextureWidth = minimapTransform.rect.width;
        float minimapTextureHeight = minimapTransform.rect.height;

        float cameraBordersWidth = cameraBordersTransform.rect.width * cameraBordersTransform.localScale.x;
        float cameraBordersHeight = cameraBordersTransform.rect.height * cameraBordersTransform.localScale.y;

        maxX = (minimapTextureWidth - cameraBordersWidth) / 2f;
        minX = -maxX;
        maxY = (minimapTextureHeight - cameraBordersHeight) / 2f;
        minY = -maxY;
    }

    private void ConstrainBounds() {
        cameraBordersTransform.localPosition = new Vector3(
            Mathf.Clamp(cameraBordersTransform.localPosition.x, minX, maxX),
            Mathf.Clamp(cameraBordersTransform.localPosition.y, minY, maxY),
            cameraBordersTransform.localPosition.z);
    }
}
