using Inner_Maps;
using UnityEngine;

[RequireComponent(typeof(HoverHandler))]
public abstract class MapObjectVisual<T> : BaseMapObjectVisual where T : IDamageable {
    public BaseCollisionTrigger<T> collisionTrigger { get; protected set; }

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
}