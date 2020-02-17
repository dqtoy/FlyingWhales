using Inner_Maps;
using UnityEngine.Assertions;

/// <summary>
/// Base class for anything in the settlement map that can be damaged and has a physical object to be shown.
/// </summary>
public abstract class MapObject<T> where T: IDamageable {
    public BaseCollisionTrigger<T> collisionTrigger => mapVisual.collisionTrigger;
    public virtual MapObjectVisual<T> mapVisual { get; protected set; } ///this is set in each inheritors implementation of <see cref="CreateAreaMapGameObject"/>
    public MAP_OBJECT_STATE mapObjectState { get; private set; }

    #region Initialization
    protected abstract void CreateAreaMapGameObject();

    public void InitializeMapObject(T obj) {
        CreateAreaMapGameObject();
        mapVisual.Initialize(obj);
        InitializeCollisionTrigger(obj);
    }
    #endregion

    #region Placement
    protected void PlaceMapObjectAt(LocationGridTile tile) {
        mapVisual.PlaceObjectAt(tile);
        collisionTrigger.gameObject.SetActive(true);
    }
    #endregion

    #region Visuals
    protected void DisableGameObject() {
        mapVisual.SetActiveState(false);
    }
    protected void EnableGameObject() {
        mapVisual.SetActiveState(true);
    }
    protected void DestroyMapVisualGameObject() {
        Assert.IsNotNull(mapVisual, $"Trying to destroy map visual of {this.ToString()} but map visual is null!");
        ObjectPoolManager.Instance.DestroyObject(mapVisual);
        mapVisual = null;
    }
    #endregion

    #region Collision
    private void InitializeCollisionTrigger(T obj) {
        collisionTrigger.Initialize(obj);
        mapVisual.UpdateCollidersState(obj);
        
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
