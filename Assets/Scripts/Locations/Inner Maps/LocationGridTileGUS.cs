using EZObjectPools;
using Pathfinding;
using UnityEngine;

namespace Inner_Maps {
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
            this.transform.localPosition = Vector3.zero;
            Apply();
        }

        public void Destroy() {
            gus.setWalkability = true;
            Apply();
            ObjectPoolManager.Instance.DestroyObject(this);
        }
    }
}
