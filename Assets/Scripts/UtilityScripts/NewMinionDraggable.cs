using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewMinionDraggable : DraggableItem {

    private AttackPickerItem _characterItem;

    private void Awake() {
        _characterItem = gameObject.GetComponent<AttackPickerItem>();
        SetDraggable(_isDraggable);
    }

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        //base.OnBeginDrag(eventData);
        //_characterItem = null;
        if (!_isDraggable) {
            return;
        }
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Drag_Clicked);
        //_characterItem = gameObject.GetComponent<PlayerCharacterItem>();
        CharacterPortrait portrait = _characterItem.portrait;
        GameObject clone = (GameObject)Instantiate(portrait.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();
        _draggingObject.gameObject.AddComponent<DragObject>().parentItem = _characterItem;

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = portrait.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    public override void OnEndDrag(PointerEventData eventData) {
        _isDragging = false;
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Drag_Hover);
        if (_characterItem != null && _draggingObject != null) {
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
                CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
                customDropzone.OnDrop(_draggingObject.gameObject);
                Destroy(_draggingObject.gameObject);
            } else {
                CancelDrag();
            }
        }
    }
    public override void CancelDrag() {
        base.CancelDrag();
        //_characterItem = null;
    }
    public override void SetDraggable(bool state) {
        if (_isDraggable != state) {
            base.SetDraggable(state);
            //if (state) {
            //    _characterItem.portrait.SwitchBGToDraggable();
            //} else {
            //    _characterItem.portrait.SwitchBGToLocked();
            //}
        }
    }
    #endregion
}
