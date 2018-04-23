using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class CharacterActionData {
    public ACTION_TYPE actionType;
    public string actionName;

    public int advertisedHunger;
    public int advertisedJoy;
    public int advertisedEnergy;
    public int advertisedPrestige;

    public int providedHunger;
    public int providedJoy;
    public int providedEnergy;
    public int providedPrestige;

    public int successRate;
    public int duration;
}

[CustomPropertyDrawer(typeof(CharacterActionData))]
public class CharacterActionDrawer : PropertyDrawer {
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var actionTypeRect = new Rect(position.x, position.y, 100, 16);
        var actionNameRect = new Rect(position.x + 135, position.y, 50, 16);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(actionTypeRect, property.FindPropertyRelative("actionType"), GUIContent.none);
        EditorGUI.PropertyField(actionNameRect, property.FindPropertyRelative("actionName"), GUIContent.none);

        EditorGUI.indentLevel = -1;
        EditorGUI.LabelField(new Rect(position.x, position.y + 20, 50, 50), "Hunger");
        EditorGUI.LabelField(new Rect(position.x + 57, position.y + 20, 50, 50), "Joy");
        EditorGUI.LabelField(new Rect(position.x + 100, position.y + 20, 50, 50), "Energy");
        EditorGUI.LabelField(new Rect(position.x + 150, position.y + 20, 50, 50), "Prestige");
        EditorGUI.indentLevel = 0;

        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, position.y + 40, 50, 50), "Advertised");

        EditorGUI.indentLevel = -1;
        var aHungerRect = new Rect(position.x, position.y + 40, 30, 16);
        var aJoyRect = new Rect(position.x + 50, position.y + 40, 30, 16);
        var aEnergyRect = new Rect(position.x + 100, position.y + 40, 30, 16);
        var aPrestigeRect = new Rect(position.x + 150, position.y + 40, 30, 16);

        EditorGUI.PropertyField(aHungerRect, property.FindPropertyRelative("advertisedHunger"), GUIContent.none);
        EditorGUI.PropertyField(aJoyRect, property.FindPropertyRelative("advertisedJoy"), GUIContent.none);
        EditorGUI.PropertyField(aEnergyRect, property.FindPropertyRelative("advertisedEnergy"), GUIContent.none);
        EditorGUI.PropertyField(aPrestigeRect, property.FindPropertyRelative("advertisedPrestige"), GUIContent.none);

        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, position.y + 60, 50, 50), "Provided");

        EditorGUI.indentLevel = -1;
        var pHungerRect = new Rect(position.x, position.y + 60, 30, 16);
        var pJoyRect = new Rect(position.x + 50, position.y + 60, 30, 16);
        var pEnergyRect = new Rect(position.x + 100, position.y + 60, 30, 16);
        var pPrestigeRect = new Rect(position.x + 150, position.y + 60, 30, 16);

        EditorGUI.PropertyField(pHungerRect, property.FindPropertyRelative("providedHunger"), GUIContent.none);
        EditorGUI.PropertyField(pJoyRect, property.FindPropertyRelative("providedJoy"), GUIContent.none);
        EditorGUI.PropertyField(pEnergyRect, property.FindPropertyRelative("providedEnergy"), GUIContent.none);
        EditorGUI.PropertyField(pPrestigeRect, property.FindPropertyRelative("providedPrestige"), GUIContent.none);

        EditorGUI.indentLevel = 0;

        ACTION_TYPE actionType = (ACTION_TYPE)property.FindPropertyRelative("actionType").enumValueIndex;

        bool enableSuccessRate = HasSuccessRate(actionType);
        EditorGUI.BeginDisabledGroup(!enableSuccessRate);
        EditorGUI.indentLevel = -7;

        var successRateRect = new Rect(position.x, position.y + 80, 30, 16);
        EditorGUI.PropertyField(successRateRect, property.FindPropertyRelative("successRate"));

        EditorGUI.indentLevel = 0;
        EditorGUI.EndDisabledGroup();

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) * 10;
    }

    private bool HasSuccessRate(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.REST:
                return false;
            case ACTION_TYPE.MOVE:
                return false;
            case ACTION_TYPE.HUNT:
                return true;
            case ACTION_TYPE.DESTROY:
                return true;
            case ACTION_TYPE.EAT:
                return false;
            case ACTION_TYPE.BUILD:
                return false;
            case ACTION_TYPE.REPAIR:
                return false;
            case ACTION_TYPE.DRINK:
                return false;
            case ACTION_TYPE.HARVEST:
                return false;
            default:
                throw new System.Exception(actionType.ToString() + " not implemented!");
        }
    }
}