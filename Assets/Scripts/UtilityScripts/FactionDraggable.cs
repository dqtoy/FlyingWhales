using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FactionDraggable : DraggableItem {

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        if (!_isDraggable) {
            return;
        }

        FactionEmblem emblem = gameObject.GetComponent<FactionIntelItem>().factionEmblem;
        GameObject clone = (GameObject)Instantiate(emblem.gameObject);
        //GameObject clone = ObjectPoolManager.Instance.InstantiateObjectFromPool(emblem.name, Vector3.zero, Quaternion.identity);
        _draggingObject = clone.GetComponent<RectTransform>();
        _draggingObject.gameObject.AddComponent<DragObject>().parentItem = gameObject.GetComponent<FactionIntelItem>();

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = emblem.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    #endregion
}
