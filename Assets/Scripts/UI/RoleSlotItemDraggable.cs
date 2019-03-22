using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoleSlotItemDraggable : DraggableItem {

    [SerializeField] private RoleSlotItem roleSlotItem;

    #region getters/setters
    public override bool isDraggable {
        get { return !PlayerManager.Instance.player.roleSlots[roleSlotItem.slotJob].isSlotLocked; }
    }
    #endregion

    private void Awake() {
        //roleSlotItem = gameObject.GetComponent<RoleSlotItem>();
        SetDraggable(isDraggable);
    }

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        //base.OnBeginDrag(eventData);
        //_characterItem = null;
        if (!isDraggable) {
            return;
        }
        GameManager.Instance.SetCursorToItemDragClicked();
        //_characterItem = gameObject.GetComponent<PlayerCharacterItem>();
        CharacterPortrait portrait = roleSlotItem.portrait;
        GameObject clone = (GameObject)Instantiate(portrait.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();
        _draggingObject.gameObject.AddComponent<DragObject>().parentItem = roleSlotItem;

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = portrait.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    public override void OnEndDrag(PointerEventData eventData) {
        _isDragging = false;
        GameManager.Instance.SetCursorToItemDragHover();
        if (roleSlotItem != null && _draggingObject != null) {
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
                GameManager.Instance.SetCursorToDefault();
                customDropzone.OnDrop(_draggingObject.gameObject);
                Destroy(_draggingObject.gameObject);
            } else {
                GameManager.Instance.SetCursorToDefault();
                PlayerManager.Instance.player.UnassignCharacterFromJob(roleSlotItem.slotJob);
                CancelDrag();
            }
        }
    }
    public override void CancelDrag() {
        base.CancelDrag();
        //_characterItem = null;
    }
    public override void SetDraggable(bool state) {
        if (isDraggable != state) {
            base.SetDraggable(state);
            //if (state) {
            //    _characterItem.portrait.SwitchBGToDraggable();
            //} else {
            //    _characterItem.portrait.SwitchBGToLocked();
            //}
        }
    }
    #endregion

    public override void OnPointerEnter(PointerEventData eventData) {
        if (roleSlotItem.associatedObj == null) {
            return;
        }
        base.OnPointerEnter(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData) {
        if (roleSlotItem.associatedObj == null) {
            return;
        }
        base.OnPointerExit(eventData);
    }
}
