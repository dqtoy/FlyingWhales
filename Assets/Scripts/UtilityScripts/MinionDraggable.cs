using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinionDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    private bool _isDraggable;
    private bool _isDragging;
    private Vector2 _draggingObjectOriginalSize;

    private RectTransform _draggingObject;

    void Start() {
        SetDraggable(true);
    }

    #region IBeginDragHandler Members
    public void OnBeginDrag(PointerEventData eventData) {
        if (!_isDraggable) {
            return;
        }
        CharacterPortrait portrait = gameObject.GetComponent<MinionItem>().portrait;
        GameObject clone = (GameObject) Instantiate(portrait.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = portrait.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    #endregion

    #region IDragHandler Members
    public void OnDrag(PointerEventData eventData) {
        if (!_isDragging) {
            return;
        }
        //Set dragging object on cursor
        var canvas = _draggingObject.GetComponentInParent<Canvas>();
        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position,
            canvas.worldCamera, out worldPoint);
        _draggingObject.position = worldPoint;
    }
    #endregion

    #region IEndDragHandler Members
    public void OnEndDrag(PointerEventData eventData) {
        _isDragging = false;

        if (_draggingObject != null) {
            List<RaycastResult> newRaycastResults = new List<RaycastResult>();
            CustomDropZone customDropzone = null;
            EventSystem.current.RaycastAll(eventData, newRaycastResults);
            for (int i = 0; i < newRaycastResults.Count; i++) {
                customDropzone = newRaycastResults[i].gameObject.GetComponent<CustomDropZone>();
                if (customDropzone != null) {
                    break;
                }
            }

            if (customDropzone != null) {
                customDropzone.OnDrop(eventData);
                Destroy(_draggingObject.gameObject);
            } else {
                CancelDrag();
            }
        }
    }
    #endregion

    public void SetDraggable(bool state) {
        _isDraggable = state;
    }

    private void CancelDrag() {
        _isDragging = false;
        Destroy(_draggingObject.gameObject);
    }

    //private void RefreshSizes() {
    //    Vector2 size = _draggingObjectOriginalSize;

    //    if (_currentReorderableListRaycasted != null && _currentReorderableListRaycasted.IsDropable && _currentReorderableListRaycasted.Content.childCount > 0) {
    //        var firstChild = _currentReorderableListRaycasted.Content.GetChild(0);
    //        if (firstChild != null) {
    //            size = firstChild.GetComponent<RectTransform>().rect.size;
    //        }
    //    }

    //    _draggingObject.sizeDelta = size;
    //    _fakeElementLE.preferredHeight = _draggingObjectLE.preferredHeight = size.y;
    //    _fakeElementLE.preferredWidth = _draggingObjectLE.preferredWidth = size.x;
    //}
}
