using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CustomDropZone : MonoBehaviour {
    public CustomDropEvent onDropItem;

    public bool isEnabled = true;

    public GameObject droppedItem { get; private set; }

    public void OnDrop(GameObject go) {
        if (!isEnabled) {
            return;
        }
        //Debug.Log(eventData.pointerDrag.name + " dropped on " + gameObject.name);
        //Transform trans = eventData.pointerDrag.GetComponent<Transform>();
        if (go != null) {
            droppedItem = go;
            if (onDropItem != null) {
                onDropItem.Invoke(go);
            }
        }
    }

    public void SetEnabledState(bool state) {
        isEnabled = state;
    }
}


[System.Serializable]
public class CustomDropEvent : UnityEvent<GameObject> {
}
