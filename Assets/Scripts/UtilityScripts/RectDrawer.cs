using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class RectDrawer : MonoBehaviour {

    public Rect rect;
    public Color color;

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    private void OnGUI() {
        GUIDrawRect(rect, color);
    }

    // Note that this function is only meant to be called from OnGUI() functions.
    public static void GUIDrawRect(Rect position, Color color) {
        if (_staticRectTexture == null) {
            _staticRectTexture = new Texture2D(1, 1);
        }

        if (_staticRectStyle == null) {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(position, GUIContent.none, _staticRectStyle);


    }
}
