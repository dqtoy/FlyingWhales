﻿using Inner_Maps;
using UnityEngine;

/// <summary>
/// Base Class for map objects that can move dynamically on their own.
/// </summary>
/// <typeparam name="T">The type of object that uses this.</typeparam>
public abstract class MovingMapObjectVisual<T> : MapObjectVisual<T> where T : IDamageable{
    
    public LocationGridTile gridTileLocation => GetLocationGridTileByXy(Mathf.FloorToInt(_pos.x), Mathf.FloorToInt(_pos.y));
    private Vector3 _pos;
    protected ILocation _mapLocation;
    
    public override void Initialize(T obj) {
        base.Initialize(obj);
        _mapLocation = obj.gridTileLocation.parentMap.location;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        _pos = transform.localPosition;
    }
    public override void Reset() {
        base.Reset();
        _mapLocation = null;
    }
    private LocationGridTile GetLocationGridTileByXy(int x, int y) {
        return _mapLocation.innerMap.map[x, y];
    }
    protected virtual void Update() {
        _pos = transform.localPosition;
    }
    
}