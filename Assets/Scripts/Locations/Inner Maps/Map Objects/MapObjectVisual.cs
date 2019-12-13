using EZObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Base class to be used for the visuals of any objects that are Area Map Objects.
/// </summary>
[RequireComponent(typeof(HoverHandler))]
public abstract class MapObjectVisual<T> : PooledObject, IMapObjectVisual, IPointerClickHandler where T : IDamageable {

    [SerializeField] private SpriteRenderer objectVisual;
    [SerializeField] private SpriteRenderer hoverObject;
    public BaseCollisionTrigger<T> collisionTrigger { get; protected set; }
    public GameObject gameObjectVisual => this.gameObject;
    private bool isHoverObjectStateLocked;
    private System.Action onLeftClickAction;
    private System.Action onRightClickAction;
    private HoverHandler _hoverHandler;

    #region getters
    public Sprite usedSprite {
        get { return objectVisual.sprite; }
    }
    #endregion

    private void Awake() {
        _hoverHandler = GetComponent<HoverHandler>();
    }
    public virtual void Initialize(T poi) {
        _hoverHandler.SetOnHoverAction(() => OnPointerEnter(poi));
        _hoverHandler.SetOnHoverOutAction(() => OnPointerExit(poi));
        onLeftClickAction = () => OnPointerLeftClick(poi);
        onRightClickAction = () => OnPointerRightClick(poi);
    }

    #region Placement
    public virtual void PlaceObjectAt(LocationGridTile tile) {
        if (tile.structure.structureObj != null) {
            tile.structure.structureObj.ReceiveMapObject(this);
        } else {
            //TODO: Make it so that work area and wilderness can also have a structure object to prevent this checking.
            this.transform.SetParent(tile.parentMap.structureParent);
        }
        Vector3 worldPos = tile.centeredWorldLocation;
        this.transform.position = worldPos;
    }
    public void SetRotation(float rotation) {
        this.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }
    #endregion

    #region Visuals
    public void SetVisual(Sprite sprite) {
        objectVisual.sprite = sprite;
        hoverObject.sprite = sprite;
    }
    public abstract void UpdateTileObjectVisual(T obj);
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
        hoverObject.gameObject.SetActive(state);
    }
    public void LockHoverObject() {
        isHoverObjectStateLocked = true;
    }
    public void UnlockHoverObject() {
            isHoverObjectStateLocked = false;
        }
    #endregion

    #region Furniture Spots
    public abstract void ApplyFurnitureSettings(FurnitureSetting furnitureSetting);
    #endregion

    #region Pointer Functions
    protected virtual void OnPointerEnter(T poi) {
        SetHoverObjectState(true);
    }
    protected virtual void OnPointerExit(T poi) {
        SetHoverObjectState(false);
    }
    protected virtual void OnPointerLeftClick(T poi) { }
    protected virtual void OnPointerRightClick(T poi) { }
    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            onLeftClickAction?.Invoke();    
        }else if (eventData.button == PointerEventData.InputButton.Right) {
            onRightClickAction?.Invoke();
        }
    }
    #endregion
}
