
using Inner_Maps;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingSpotDataMonobehaviour))]
public class BuildingSpotDataMonobehaviourEditor : Editor {

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    static void DrawGizmos(BuildingSpotDataMonobehaviour connector, GizmoType gizmoType) {
        Vector3 position = connector.transform.position;
        if (connector.isOpen) {
            Gizmos.color = Color.white;
        } else {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireCube(position, new Vector3(InnerMapManager.BuildingSpotSize.x, InnerMapManager.BuildingSpotSize.y, 0));
    }
}
#endif

