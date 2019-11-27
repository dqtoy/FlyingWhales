using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AreaMapObject<T> where T : IPointOfInterest {

    public POICollisionTrigger collisionTrigger { get; private set; }
    public virtual AreaMapGameObject<T> areaMapGameObject { get; protected set; } ///this is set in each inheritors implementation of <see cref="CreateAreaMapGameObject"/>
            
    #region Main
    protected abstract void CreateAreaMapGameObject();
    protected void InitializeMapObject(T obj) {
        CreateAreaMapGameObject();
        areaMapGameObject.Initialize(obj);
        InitializeCollisionTrigger(obj);
    }
    public void PlaceMapObjectAt(LocationGridTile tile) {
        areaMapGameObject.PlaceObjectAt(tile);
        //collisionTrigger.transform.SetParent(tile.parentAreaMap.objectsParent);
        //(collisionTrigger.transform as RectTransform).anchoredPosition = tile.centeredLocalLocation;
        collisionTrigger.gameObject.SetActive(true);
        collisionTrigger.SetLocation(tile);
    }
    public void DisableGameObject() {
        areaMapGameObject.SetActiveState(false);
    }
    public void EnableGameObject() {
        areaMapGameObject.SetActiveState(true);
    }
    #endregion

    #region Collision
    public void InitializeCollisionTrigger(T obj) {
        SetCollisionTrigger(areaMapGameObject.collisionTrigger);
        collisionTrigger.Initialize(obj);
    }
    public void SetCollisionTrigger(POICollisionTrigger trigger) {
        collisionTrigger = trigger;
    }
    #endregion
}
