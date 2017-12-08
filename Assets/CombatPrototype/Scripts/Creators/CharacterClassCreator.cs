using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ECS {
    public class CharacterClassCreator : EditorWindow {

        private string className;

        // Add menu item to the Window menu
        [MenuItem("Window/Character Class Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(CharacterClassCreator));
        }

        void OnGUI() {
            GUILayout.Label("Class Creator ", EditorStyles.boldLabel);
            className = EditorGUILayout.TextField("Class Name: ", className);

            //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            //myBool = EditorGUILayout.Toggle("Toggle", myBool);
            //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            //EditorGUILayout.EndToggleGroup();
        }
    }
}

