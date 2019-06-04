#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConnectorMono))]
public class ConnectorMonoEditor : Editor {

    private static Dictionary<STRUCTURE_TYPE, Vector2> structureSizes = new Dictionary<STRUCTURE_TYPE, Vector2>() {
        { STRUCTURE_TYPE.DWELLING, new Vector2(4, 4) },
        { STRUCTURE_TYPE.INN, new Vector2(12, 8) },
        { STRUCTURE_TYPE.WAREHOUSE, new Vector2(7, 6) },
    };
    private static Dictionary<STRUCTURE_TYPE, Color> structureColor = new Dictionary<STRUCTURE_TYPE, Color>() {
        { STRUCTURE_TYPE.DWELLING, Color.white },
        { STRUCTURE_TYPE.INN, Color.blue },
        { STRUCTURE_TYPE.WAREHOUSE, Color.red },
    };

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    static void DrawGizmos(ConnectorMono connector, GizmoType gizmoType) {
        Vector3 position = connector.transform.localPosition;
        Gizmos.color = Color.yellow;

        if (structureSizes.ContainsKey(connector.allowedStructureType)) {
            Handles.color = structureColor[connector.allowedStructureType];
            Vector3 origin = connector.transform.localPosition;

            Vector2 size = structureSizes[connector.allowedStructureType];
            switch (connector.connectionDirection) {
                case Cardinal_Direction.North:
                    origin.y += (size.y - 0.5f);
                    origin.x -= 0.5f;
                    break;
                case Cardinal_Direction.South:
                    origin.y -= (size.y - 0.5f);
                    origin.x -= 0.5f;
                    break;
                case Cardinal_Direction.East:
                    origin.x += (size.y - 0.5f);
                    origin.y += 0.5f;
                    break;
                case Cardinal_Direction.West:
                    origin.x -= (size.y - 0.5f);
                    origin.y += 0.5f;
                    break;
                default:
                    break;
            }
            Gizmos.DrawWireCube(origin, size);
        }

        //if (connector.allowedStructureType == STRUCTURE_TYPE.DWELLING) {
        //    Handles.color = Color.white;
        //    Vector3 center = Vector3.zero;
        //    Vector3 origin = connector.transform.localPosition;

        //    Vector2 dwellingSize = new Vector2(4f, 4f);
        //    switch (connector.connectionDirection) {
        //        case Cardinal_Direction.North:
        //            origin.y += 3.5f;
        //            origin.x -= 0.5f;
        //            break;
        //        case Cardinal_Direction.South:
        //            origin.y -= 3.5f;
        //            origin.x -= 0.5f;
        //            break;
        //        case Cardinal_Direction.East:
        //            origin.x += 3.5f;
        //            origin.y += 0.5f;
        //            break;
        //        case Cardinal_Direction.West:
        //            origin.x -= 3.5f;
        //            origin.y += 0.5f;
        //            break;
        //        default:
        //            break;
        //    }
        //    Gizmos.DrawWireCube(origin, dwellingSize);
        //}
    }
}
#endif

