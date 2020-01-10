using EZObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Base class to be used for the visuals of any objects that are Settlement Map Objects.
/// </summary>
public abstract class BaseMapObjectVisual : PooledObject, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] protected SpriteRenderer objectVisual;
    [SerializeField] protected SpriteRenderer hoverObject;
    private bool isHoverObjectStateLocked;
    protected System.Action onHoverOverAction;
    protected System.Action onHoverExitAction;
    protected System.Action onLeftClickAction;
    protected System.Action onRightClickAction;
    public GameObject gameObjectVisual => this.gameObject;
    public Sprite usedSprite => objectVisual.sprite;
    
    #region Visuals
    public void SetRotation(float rotation) {
        this.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }
    public void SetVisual(Sprite sprite) {
        objectVisual.sprite = sprite;
        hoverObject.sprite = sprite;
    }
    public void SetColor(Color color) {
        objectVisual.color = color;
    }
    public void SetActiveState(bool state) {
        this.gameObject.SetActive(state);
    }
    public void SetVisualAlpha(float alpha) {
        Color color = objectVisual.color;
        color.a = alpha;
        SetColor(color);
    }
    public void SetHoverObjectState(bool state) {
        if (isHoverObjectStateLocked) {
            return; //ignore change because hover state is locked
        }
        if (hoverObject.gameObject.activeSelf == state) {
            return; //ignore change
        }
        hoverObject.gameObject.SetActive(state);
    }
    public void LockHoverObject() {
        isHoverObjectStateLocked = true;
    }
    public void UnlockHoverObject() {
        isHoverObjectStateLocked = false;
    }
    public abstract void ApplyFurnitureSettings(FurnitureSetting furnitureSetting);
    #endregion

    #region Inquiry
    /// <summary>
    /// Is this objects menu (CharacterInfoUI, TileObjectInfoUI) currently showing this objects info?
    /// </summary>
    /// <returns>True or false</returns>
    public abstract bool IsMapObjectMenuVisible();
    public bool IsInvisible() {
        if (objectVisual != null) {
            return Mathf.Approximately(objectVisual.color.a, 0f);    
        }
        return false;
    }
    #endregion

    #region Pointer Functions
    public void ExecuteClickAction(PointerEventData.InputButton button) {
        if (button == PointerEventData.InputButton.Left) {
            onLeftClickAction?.Invoke();    
        }else if (button == PointerEventData.InputButton.Right) {
            onRightClickAction?.Invoke();
        }
    }
    public void OnPointerEnter(PointerEventData eventData) {
        ExecuteHoverEnterAction();
    }
    public void OnPointerExit(PointerEventData eventData) {
        ExecuteHoverExitAction();
    }
    public void ExecuteHoverEnterAction() {
        onHoverOverAction?.Invoke();
    }
    public void ExecuteHoverExitAction() {
        onHoverExitAction?.Invoke();
    }
    #endregion

    
}