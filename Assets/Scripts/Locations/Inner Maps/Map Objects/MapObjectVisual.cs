using Inner_Maps;
using UnityEngine;

[RequireComponent(typeof(HoverHandler))]
public abstract class MapObjectVisual<T> : BaseMapObjectVisual where T : IDamageable {
    public BaseCollisionTrigger<T> collisionTrigger { get; protected set; }

    public virtual void Initialize(T obj) {
        _hoverHandler.SetOnHoverAction(() => OnPointerEnter(obj));
        _hoverHandler.SetOnHoverOutAction(() => OnPointerExit(obj));
        onLeftClickAction = () => OnPointerLeftClick(obj);
        onRightClickAction = () => OnPointerRightClick(obj);
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
    #endregion

    #region Visuals
    public abstract void UpdateTileObjectVisual(T obj);
    #endregion

    #region Furniture Spots
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
    #endregion

    #region Collisions
    public abstract void UpdateCollidersState(T obj);
    protected void DisableColliders() {
        collisionTrigger.SetCollidersState(false);
    }
    protected void EnableColliders() {
        collisionTrigger.SetCollidersState(true);
    }
    #endregion
}