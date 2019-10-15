using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPickerItem<T> : MonoBehaviour {

    [SerializeField] protected TextMeshProUGUI mainLbl;
    [SerializeField] protected TextMeshProUGUI subLbl;
    [SerializeField] protected Button mainBtn;
    [SerializeField] protected NewMinionDraggable draggable;

    public System.Action<T> onHoverEnterAction;
    public System.Action<T> onHoverExitAction;

    public virtual T obj { get; }

    public virtual void SetButtonState(bool state) {
        mainBtn.interactable = state;
    }
    public virtual void SetDraggableState(bool state) {
        draggable.SetDraggable(state);
    }

    public void OnHoverEnter() {
        if (onHoverEnterAction != null) {
            onHoverEnterAction.Invoke(obj);
        }
    }
    public virtual void OnHoverExit() {
        if (onHoverExitAction != null) {
            onHoverExitAction.Invoke(obj);
        }
    }
}
