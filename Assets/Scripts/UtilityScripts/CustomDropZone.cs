using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CustomDropZone : MonoBehaviour, IDropHandler {
    public DropEvent onDropItem;

    public void OnDrop(PointerEventData eventData) {
        Debug.Log(eventData.pointerDrag.name + " dropped on " + gameObject.name);

        Transform trans = eventData.pointerDrag.GetComponent<Transform>();
        if (trans != null) {
            if (onDropItem != null) {
                onDropItem.Invoke(trans);
            }
        }
    }
}
