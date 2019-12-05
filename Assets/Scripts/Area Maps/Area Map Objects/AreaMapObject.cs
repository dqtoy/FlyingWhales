using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for anything in the area map that can be damaged and has a physical object to be shown.
/// </summary>
public abstract class AreaMapObject<T> where T: IDamageable {
    public IDamageable damageable { get; private set; }
    public BaseCollisionTrigger<T> collisionTrigger { get { return areaMapVisual.collisionTrigger; } }
    public virtual AreaMapObjectVisual<T> areaMapVisual { get; protected set; } ///this is set in each inheritors implementation of <see cref="CreateAreaMapGameObject"/>
    public MAP_OBJECT_STATE mapObjectState { get; private set; }

    #region Initialization
    protected abstract void CreateAreaMapGameObject();
    protected void InitializeMapObject(T obj) {
        damageable = obj;
        CreateAreaMapGameObject();
        areaMapVisual.Initialize(obj);
        InitializeCollisionTrigger(obj);
    }
    #endregion

    #region Placement
    public void PlaceMapObjectAt(LocationGridTile tile) {
        areaMapVisual.PlaceObjectAt(tile);
        collisionTrigger.gameObject.SetActive(true);
    }
    #endregion

    #region Visuals
    public void DisableGameObject() {
        areaMapVisual.SetActiveState(false);
    }
    public void EnableGameObject() {
        areaMapVisual.SetActiveState(true);
    }
    public void DestroyGameObject() {
        ObjectPoolManager.Instance.DestroyObject(areaMapVisual.gameObject);
        areaMapVisual = null;
    }
    #endregion

    #region Collision
    public void InitializeCollisionTrigger(T obj) {
        collisionTrigger.Initialize(obj);
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
