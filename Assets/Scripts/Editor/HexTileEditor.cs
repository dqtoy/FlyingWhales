#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexTile))]
public class HexTileEditor : Editor {

    //[DrawGizmo(GizmoType.NonSelected)]
    //static void DrawGizmos(HexTile tile, GizmoType gizmoType) {
    //    string summary = tile.xCoordinate.ToString() + ", " + tile.yCoordinate;
    //    summary += "\n" + tile.elevationType.ToString();
    //    summary += "\n" + tile.landmarkOnTile?.ToString();
    //    Handles.Label(tile.transform.position, summary);
    //}
}
#endif
