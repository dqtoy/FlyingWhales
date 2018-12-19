using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    protected bool isHovering;

    [SerializeField] protected RectTransform tooltipPos;
    [SerializeField] protected string tooltipHeader;

    [SerializeField] protected UnityEvent onHoverOverAction;
    [SerializeField] protected UnityEvent onHoverExitAction;

    protected Selectable selectable;

    private void OnEnable() {
        selectable = this.GetComponent<Selectable>();
    }

    private void OnDisable() {
        isHovering = false;
        if (onHoverExitAction != null) {
            onHoverExitAction.Invoke();
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (selectable != null) {
            if (!selectable.IsInteractable()) {
                return;
            }
        }
        isHovering = true;
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        if (selectable != null) {
            if (!selectable.IsInteractable()) {
                return;
            }
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
