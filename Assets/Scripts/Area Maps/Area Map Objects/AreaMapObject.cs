using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AreaMapObject<T> where T : IPointOfInterest {

    public POICollisionTrigger collisionTrigger { get; private set; }
    public virtual AreaMapObjectVisual<T> areaMapGameObject { get; protected set; } ///this is set in each inheritors implementation of <see cref="CreateAreaMapGameObject"/>
    public MAP_OBJECT_STATE mapObjectState { get; private set; }

    #region Initialization
    protected abstract void CreateAreaMapGameObject();
    protected void InitializeMapObject(T obj) {
        CreateAreaMapGameObject();
        areaMapGameObject.Initialize(obj);
        InitializeCollisionTrigger(obj);
    }
    #endregion

    #region Placement
    public void PlaceMapObjectAt(LocationGridTile tile) {
        areaMapGameObject.PlaceObjectAt(tile);
        collisionTrigger.gameObject.SetActive(true);
        collisionTrigger.SetLocation(tile);
    }
    #endregion

    #region Visuals
    public void DisableGameObject() {
        areaMapGameObject.SetActiveState(false);
    }
    public void EnableGameObject() {
        areaMapGameObject.SetActiveState(true);
    }
    public void DestroyGameObject() {
        ObjectPoolManager.Instance.DestroyObject(areaMapGameObject.gameObject);
        areaMapGameObject = null;
        SetCollisionTrigger(null);
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

    #region Object State
    public void SetMapObjectState(MAP_OBJECT_STATE state) {
        if (mapObjectState == state) {
            return; //ignore change
        }
        mapObjectState = state;
        OnMapObjectStateChanged();
    }
    protected abstract void OnMapObjectStateChanged();
    #endregion
}
