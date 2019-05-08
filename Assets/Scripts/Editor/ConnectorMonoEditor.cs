#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConnectorMono))]
public class ConnectorMonoEditor : Editor {

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    static void DrawGizmos(ConnectorMono connector, GizmoType gizmoType) {
        Vector3 position = connector.transform.localPosition;
        Gizmos.color = Color.yellow;

        if (connector.allowedStructureType == STRUCTURE_TYPE.DWELLING) {
            Handles.color = Color.white;
            Vector3 center = Vector3.zero;
            Vector3 origin = connector.transform.localPosition;

            Vector2 dwellingSize = new Vector2(4f, 4f);
            switch (connector.connectionDirection) {
                case Cardinal_Direction.North:
                    origin.y += 3.5f;
                    origin.x -= 0.5f;
                    break;
                case Cardinal_Direction.South:
                    origin.y -= 3.5f;
                    origin.x -= 0.5f;
                    break;
                case Cardinal_Direction.East:
                    origin.x += 3.5f;
                    origin.y += 0.5f;
                    break;
                case Cardinal_Direction.West:
                    origin.x -= 3.5f;
                    origin.y += 0.5f;
                    break;
                default:
                    break;
            }
            Gizmos.DrawWireCube(origin, dwellingSize);
        }
    }
}
#endif

