using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterDraggable : DraggableItem {

    private CharacterTokenItem parentItem;

    private void Awake() {
        parentItem = gameObject.GetComponent<CharacterTokenItem>();
        SetDraggable(_isDraggable);
    }

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        if (!_isDraggable) {
            return;
        }
        CharacterPortrait portrait = parentItem.characterPortrait;
        GameObject clone = (GameObject)Instantiate(portrait.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();

        _draggingObject.gameObject.AddComponent<DragObject>().parentItem = parentItem;

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = portrait.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    public override void SetDraggable(bool state) {
        if (_isDraggable != state) {
            base.SetDraggable(state);
            if (state) {
                //parentItem.characterPortrait.SwitchBGToDraggable();
            } else {
                //parentItem.characterPortrait.SwitchBGToLocked();
            }
        }
    }
    #endregion
}
