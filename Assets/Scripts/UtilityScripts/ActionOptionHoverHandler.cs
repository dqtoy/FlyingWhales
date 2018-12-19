using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ActionOptionHoverHandler : UIHoverHandler {

    private void OnEnable() {
        selectable = this.GetComponent<Selectable>();
    }

    private void OnDisable() {
        isHovering = false;
        if (onHoverExitAction != null) {
            onHoverExitAction.Invoke();
        }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        if (selectable != null) {
            return;
        }
        isHovering = true;
    }
    public override void OnPointerExit(PointerEventData eventData) {
        if (selectable != null) {
            return;
        }
        isHovering = false;
        if (onHoverExitAction != null) {
            onHoverExitAction.Invoke();
        }
    }
    void Update() {
        if (isHovering) {
            if (onHoverOverAction != null) {
                onHoverOverAction.Invoke();
            }
        }
    }
}
