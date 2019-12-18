﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    protected bool isHovering;

    [SerializeField] private UIHoverPosition tooltipPos;
    [SerializeField] protected string tooltipHeader;
    [SerializeField] protected bool ignoreInteractable = false;

    [SerializeField] protected UnityEvent onHoverOverAction;
    [SerializeField] protected UnityEvent onHoverExitAction;

    protected Selectable selectable;


    private void OnEnable() {
        selectable = this.GetComponent<Selectable>();
    }

    private void OnDisable() {
        isHovering = false;
        onHoverExitAction?.Invoke();
    }
    private void OnDestroy() {
        isHovering = false;
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (!ignoreInteractable && selectable != null) {
            if (!selectable.IsInteractable()) {
                return;
            }
        }
        isHovering = true;
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        if (!ignoreInteractable && selectable != null) {
            if (!selectable.IsInteractable()) {
                return;
            }
        }
        isHovering = false;
        onHoverExitAction?.Invoke();
    }


    public void SetOnHoverAction(UnityAction e) {
        onHoverOverAction.RemoveAllListeners();
        onHoverOverAction.AddListener(e);
    }
    public void SetOnHoverOutAction(UnityAction e) {
        onHoverExitAction.RemoveAllListeners();
        onHoverExitAction.AddListener(e);
    }
    public void AddOnHoverAction(UnityAction e) {
        onHoverOverAction.AddListener(e);
    }
    public void AddOnHoverOutAction(UnityAction e) {
        onHoverExitAction.AddListener(e);
    }

    void Update() {
        if (isHovering) {
            onHoverOverAction?.Invoke();
        }
    }

    public void ShowSmallInfoString(string message) {
        UIManager.Instance.ShowSmallInfo(message, tooltipHeader);
    }
    public void HideSmallInfoString() {
        UIManager.Instance.HideSmallInfo();
    }

    public void ShowSmallInfoInSpecificPosition(string message) {
        if (tooltipPos != null) {
            UIManager.Instance.ShowSmallInfo(message, tooltipPos, tooltipHeader);
        } else {
            UIManager.Instance.ShowSmallInfo(message, tooltipHeader);
        }
    }
}