using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour {

    public void OnPointerClickWithBaseData(BaseEventData data) {
        PointerEventData ped = (PointerEventData)data;
        CameraMove.Instance.MoveMainCamera(GetLocalCursorPoint(ped));
    }

    private Vector2 GetLocalCursorPoint(PointerEventData ped) {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), ped.position, ped.pressEventCamera, out localCursor))
            return Vector2.zero;
        return localCursor;
    }

    void DebugPoint(PointerEventData ped) {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), ped.position, ped.pressEventCamera, out localCursor))
            return;

        Debug.Log("LocalCursor:" + localCursor);
    }
}
