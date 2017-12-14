using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ECS {
    public class ItemCreator : EditorWindow {

        // Add menu item to the Window menu
        [MenuItem("Window/Item Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(ItemCreator));
        }

        private void OnGUI() {

        }
    }
}