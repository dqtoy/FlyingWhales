using EZObjectPools;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationGridTileGUS : PooledObject {
    [SerializeField] private GraphUpdateScene gus;
    [SerializeField] private BoxCollider2D boxCollider;

    [ContextMenu("Apply")]
    public void Apply() {
        gus.Apply();
    }
    public void Initialize(Vector2 offset, Vector2 size, IPointOfInterest poi) {
        this.name = poi.name;
        boxCollider.offset = offset;
        boxCollider.size = size;
        gus.setWalkability = false;
        this.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
        Apply();
    }

    public void Destroy() {
        gus.setWalkability = true;
        Apply();
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
}
