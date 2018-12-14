using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpecialTokenDraggable : DraggableItem {

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        if (!_isDraggable) {
            return;
        }

        GameObject visual = gameObject.GetComponent<SpecialTokenItem>().specialTokenVisual;
        GameObject clone = (GameObject)Instantiate(visual.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();
        _draggingObject.gameObject.AddComponent<DragObject>().parentItem = gameObject.GetComponent<SpecialTokenItem>();

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = visual.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    #endregion
}
