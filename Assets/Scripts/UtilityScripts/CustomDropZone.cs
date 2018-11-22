using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomDropZone : MonoBehaviour {
    public CustomDropEvent onDropItem;

    public bool isEnabled = true;

    [Space(10)]
    [Header("On Enable/Disable")]
    [SerializeField] private UnityEvent onEnableSlotAction;
    [SerializeField] private UnityEvent onDisableSlotAction;

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
        if (isEnabled) {
            if (onEnableSlotAction != null) {
                onEnableSlotAction.Invoke();
            }
        } else {
            if (onDisableSlotAction != null) {
                onDisableSlotAction.Invoke();
            }
        }
    }
}


[System.Serializable]
public class CustomDropEvent : UnityEvent<GameObject> { }
