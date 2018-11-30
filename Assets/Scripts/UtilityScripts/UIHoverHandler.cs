using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private bool isHovering;

    [SerializeField] private RectTransform tooltipPos;
    [SerializeField] private string tooltipHeader;

    [SerializeField] private UnityEvent onHoverOverAction;
    [SerializeField] private UnityEvent onHoverExitAction;

    private void OnDisable() {
        isHovering = false;
        if (onHoverExitAction != null) {
            onHoverExitAction.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        if (onHoverExitAction != null) {
            onHoverExitAction.Invoke();
        }
    }

    private void Update() {
        if (isHovering) {
            if (onHoverOverAction != null) {
                onHoverOverAction.Invoke();
            }
        }
    }

    public void ShowSmallInfoString(string message) {
        UIManager.Instance.ShowSmallInfo(message);
    }
    public void HideSmallInfoString() {
        UIManager.Instance.HideSmallInfo();
    }

    public void ShowSmallInfoInSpecificPosition(string message) {
        UIManager.Instance.ShowSmallInfo(message, tooltipHeader, tooltipPos);
    }
}
