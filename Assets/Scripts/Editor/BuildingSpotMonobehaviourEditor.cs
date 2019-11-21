#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingSpotMonobehaviour))]
public class BuildingSpotMonobehaviourEditor : Editor {

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    static void DrawGizmos(BuildingSpotMonobehaviour connector, GizmoType gizmoType) {
        Vector3 position = connector.transform.position;
        if (connector.isOpen) {
            Gizmos.color = Color.white;
        } else {
            Gizmos.color = Color.red;
        }
        
        Gizmos.DrawWireCube(position, InteriorMapManager.Building_Spot_Size);
    }

}
#endif

