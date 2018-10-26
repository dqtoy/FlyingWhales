using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CustomDropZone : MonoBehaviour, IDropHandler {
    public DropEvent onDropItem;

    public Transform droppedItem { get; private set; }

    public void OnDrop(PointerEventData eventData) {
        Debug.Log(eventData.pointerDrag.name + " dropped on " + gameObject.name);
        Transform trans = eventData.pointerDrag.GetComponent<Transform>();
        if (trans != null) {
            droppedItem = trans;
            if (onDropItem != null) {
                onDropItem.Invoke(trans);
            }
        }
    }

    public void ReturnDropped() {

    }
}
