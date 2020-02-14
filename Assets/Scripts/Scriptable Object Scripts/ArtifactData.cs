using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact Data", menuName = "Artifact Data")]
public class ArtifactData : ScriptableObject {
    [SerializeField] private ARTIFACT_TYPE _type;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private Sprite _portrait;
    [SerializeField] private ArtifactUnlockable[] _unlocks;
    
    #region getters
    public ARTIFACT_TYPE type => _type;
    public Sprite sprite => _sprite;
    public ArtifactUnlockable[] unlocks => _unlocks;
    public Sprite portrait => _portrait;
    #endregion
}

[CustomPropertyDrawer(typeof(ArtifactUnlockable))]
public class ArtifactUnlockableDrawer : PropertyDrawer {
    private ARTIFACT_UNLOCKABLE_TYPE _unlockableType;
    private int _selectedIndex;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Element"));

        EditorGUI.indentLevel = 0;
        
        var unlockableTypeRect = new Rect(position.x, position.y, 150, position.height);
        var unlockableStrRect = new Rect(position.x + 150, position.y, 150, position.height);
        
        SerializedProperty unlockableTypeProperty = property.FindPropertyRelative("unlockableType");
        SerializedProperty unlockableIdentifierProperty = property.FindPropertyRelative("identifier");
        
        _unlockableType = (ARTIFACT_UNLOCKABLE_TYPE) unlockableTypeProperty.enumValueIndex;
        _unlockableType = (ARTIFACT_UNLOCKABLE_TYPE)EditorGUI.EnumPopup(unlockableTypeRect, _unlockableType);
        
        string[] unlockables = PlayerDB.GetChoicesForUnlockableType(_unlockableType);

        if (string.IsNullOrEmpty(unlockableIdentifierProperty.stringValue) == false) {
            _selectedIndex = Array.IndexOf(unlockables, unlockableIdentifierProperty.stringValue);
            if (_selectedIndex == -1) {
                _selectedIndex = 0;
            }
        }
        _selectedIndex = EditorGUI.Popup(unlockableStrRect, _selectedIndex,unlockables);
        
        unlockableTypeProperty.enumValueIndex = (int) _unlockableType;
        unlockableIdentifierProperty.stringValue = unlockables[_selectedIndex];

        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
    }
}

[Serializable]
public class ArtifactUnlockable {
    public ARTIFACT_UNLOCKABLE_TYPE unlockableType;
    public string identifier;
}