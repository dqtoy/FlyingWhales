using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Pathfinding.RVO {
    using Pathfinding.Util;
    public class FleeingRVOController : RVOController {
        public override float radius {
            get {
                return radiusBackingField;
            }
            set {
                radiusBackingField = value;
            }
        }

        void OnDrawGizmos() {
            tr = transform;
            // The AI script will draw similar gizmos
            if (ai == null) {
                var color = AIBase.ShapeGizmoColor * (locked ? 0.5f : 1.0f);
                var pos = transform.position;

                var scale = tr.localScale;
                if (movementPlane == MovementPlane.XY) {
                    Draw.Gizmos.Cylinder(pos, Vector3.forward, 0, radius * scale.x, color);
                } else {
                    Draw.Gizmos.Cylinder(pos + To3D(Vector2.zero, center - height * 0.5f) * scale.y, To3D(Vector2.zero, 1), height * scale.y, radius * scale.x, color);
                }
            } else {
                var pos = transform.position;
                var color = AIBase.ShapeGizmoColor * (locked ? 0.5f : 1.0f);
                Draw.Gizmos.Cylinder(pos, Vector3.forward, 0, radius * tr.localScale.x, color);

                //if (!float.IsPositiveInfinity(ai.destination.x) && Application.isPlaying) Draw.Gizmos.CircleXZ(ai.destination, 0.2f, Color.blue);
            }

        }
    }
}

