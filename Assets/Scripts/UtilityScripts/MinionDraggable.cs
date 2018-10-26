using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinionDraggable : DraggableItem {

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        if (!_isDraggable) {
            return;
        }
        CharacterPortrait portrait = gameObject.GetComponent<MinionItem>().portrait;
        GameObject clone = (GameObject)Instantiate(portrait.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = portrait.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    #endregion

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
