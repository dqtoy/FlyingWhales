using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ActionOptionHoverHandler : UIHoverHandler {
    public override void OnPointerEnter(PointerEventData eventData) {
        //if (selectable != null) {
        //    if (!selectable.IsInteractable()) {
        //        return;
        //    }
        //}
        if(selectable != null) {
            isHovering = true;
        }
    }
    public override void OnPointerExit(PointerEventData eventData) {
        if (selectable != null) {
            isHovering = false;
            if (onHoverExitAction != null) {
                onHoverExitAction.Invoke();
            }
        }
        //isHovering = false;
        //if (onHoverExitAction != null) {
        //    onHoverExitAction.Invoke();
        //}
    }
}
