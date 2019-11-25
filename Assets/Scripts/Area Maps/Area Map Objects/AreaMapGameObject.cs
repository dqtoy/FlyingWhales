using EZObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AreaMapGameObject<T> : PooledObject 
    where T : IPointOfInterest{

    [SerializeField] protected SpriteRenderer objectVisual;
    public POICollisionTrigger collisionTrigger;

    #region getters
    public Sprite usedSprite {
        get { return objectVisual.sprite; }
    }
    #endregion

    public abstract void Initialize(T poi);

    #region Placement
    public void PlaceObjectAt(LocationGridTile tile) {
        if (tile.structure.structureObj != null) {
            tile.structure.structureObj.ReceiveMapObject(this);
        } else {
            //TODO: Make it so that work area and wilderness can also have a structure object to prevent this checking.
            this.transform.SetParent(tile.parentAreaMap.structureParent);
        }
        Vector3 worldPos = tile.centeredWorldLocation;
        this.transform.position = worldPos;
    }
    public void SetRotation(float rotation) {
        this.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }
    #endregion

    #region Visuals
    public void OverrideVisual(Sprite sprite) {
        objectVisual.sprite = sprite;
    }
    public abstract void UpdateTileObjectVisual(T obj);
    public void SetColor(Color color) {
        objectVisual.color = color;
    }
    public void SetActiveState(bool state) {
        this.gameObject.SetActive(state);
    }
    #endregion

}
