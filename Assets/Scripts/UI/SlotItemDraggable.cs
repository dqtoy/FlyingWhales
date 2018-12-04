using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotItemDraggable : DraggableItem {

    [SerializeField] private SlotItem slot;

    private void Awake() {
        SetDraggable(_isDraggable);
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        if (slot.placedObject == null || !_isDraggable) {
            return; //do not allow drag if the slot does not have an item on it
        }
        base.OnBeginDrag(eventData);
        GameObject original = null;
        GameObject clone = null;
        if (slot.placedObject is LocationIntel) {
            AreaEmblem emblem = slot.areaEmblem;
            original = emblem.gameObject;
            clone = (GameObject)Instantiate(emblem.gameObject);
        } else if (slot.placedObject is FactionIntel) {
            FactionEmblem emblem = slot.factionEmblem;
            original = emblem.gameObject;
            clone = (GameObject)Instantiate(emblem.gameObject);
        } else if (slot.placedObject is Minion || slot.placedObject is Character) {
            CharacterPortrait portrait = slot.portrait;
            original = portrait.gameObject;
            clone = (GameObject)Instantiate(portrait.gameObject);
        }
        _draggingObject = clone.GetComponent<RectTransform>();
        _draggingObject.sizeDelta = original.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
        //slot.HideVisuals();
    }

    public override void OnDrag(PointerEventData eventData) {
        if (slot.placedObject == null || !_isDraggable) {
            return; //do not allow drag if the slot does not have an item on it
        }
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData) {
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
            Destroy(_draggingObject.gameObject);
            if (customDropzone == null) {
                slot.OnItemDroppedOut();
            }
        }
    }
    public override void SetDraggable(bool state) {
        if (_isDraggable != state) {
            base.SetDraggable(state);
            if (state) {
                slot.portrait.SwitchBGToDraggable();
            } else {
                slot.portrait.SwitchBGToLocked();
            }
        }
    }
}
