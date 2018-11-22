using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {

    [SerializeField] protected bool _isDraggable;
    [SerializeField] protected bool _isDragging;
    protected Vector2 _draggingObjectOriginalSize;

    protected RectTransform _draggingObject;

    protected object associatedObj;

    private void Awake() {
        SetDraggable(true);
    }

    public virtual void SetAssociatedObject(object obj) {
        associatedObj = obj;
    }

    #region IBeginDragHandler Members
    public virtual void OnBeginDrag(PointerEventData eventData) {
        
    }
    #endregion

    #region IDragHandler Members
    public virtual void OnDrag(PointerEventData eventData) {
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
    public virtual void OnEndDrag(PointerEventData eventData) {
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
                customDropzone.OnDrop(_draggingObject.gameObject);
                Destroy(_draggingObject.gameObject);
            } else {
                CancelDrag();
            }
        }
    }
    public virtual void CancelDrag() {
        _isDragging = false;
        Destroy(_draggingObject.gameObject);
    }
    #endregion

    public void SetDraggable(bool state) {
        _isDraggable = state;
    }

    
}

public class DragObject : MonoBehaviour {

    public IDragParentItem parentItem;
}
