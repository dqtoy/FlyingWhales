using System.Collections;
using EZObjectPools;
using Pathfinding;
using UnityEngine;

namespace Inner_Maps {
    public class LocationGridTileGUS : PooledObject {
        [SerializeField] private GraphUpdateScene gus;
        [SerializeField] private BoxCollider2D boxCollider;

        [ContextMenu("Apply")]
        public void Apply() {
            // gus.Apply();
            StartCoroutine(ApplyCoroutine());
        }
        private IEnumerator ApplyCoroutine() {
            yield return null;
            gus.Apply();
        }
        public void Initialize(Vector2 offset, Vector2 size, IPointOfInterest poi) {
            name = poi.name;
            boxCollider.offset = offset;
            boxCollider.size = size;
            gus.setWalkability = false;
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
            Apply();
        }

        public void Destroy() {
            gus.setWalkability = true;
            Apply();
            ObjectPoolManager.Instance.DestroyObject(this);
        }
    }
}
