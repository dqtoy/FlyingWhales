﻿using System.Collections;
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

    public RESOURCE resourceGiven;
    public int minResourceGiven;
    public int maxResourceGiven;

    public RESOURCE resourceNeeded;
    public int resourceAmountNeeded;
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
        bool enableDuration = HasDuration(actionType);
        bool enableResourceGiven = GivesResource(actionType);
        bool enableResourceNeeded = NeedResource(actionType);

        float succesRateYPos = position.y + 80;
        float durationYPos = position.y + 100;
        float resourceGivenYPos = position.y + 120;
        float resourceNeededYPos = position.y + 160;

        EditorGUI.BeginDisabledGroup(!enableSuccessRate);
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, succesRateYPos, 50, 50), "Success Rate");
        var successRateRect = new Rect(position.x + 90, succesRateYPos, 80, 16);
        EditorGUI.PropertyField(successRateRect, property.FindPropertyRelative("successRate"), GUIContent.none);
        EditorGUI.indentLevel = 0;
        EditorGUI.EndDisabledGroup();

        if (!enableSuccessRate) {
            property.FindPropertyRelative("successRate").intValue = 0;
        }

        EditorGUI.BeginDisabledGroup(!enableDuration);
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, durationYPos, 50, 50), "Duration");
        var durationRect = new Rect(position.x + 90, durationYPos, 80, 16);
        EditorGUI.PropertyField(durationRect, property.FindPropertyRelative("duration"), GUIContent.none);
        EditorGUI.indentLevel = 0;
        EditorGUI.EndDisabledGroup();
        if (!enableSuccessRate) {
            property.FindPropertyRelative("duration").intValue = 0;
        }

        EditorGUI.BeginDisabledGroup(!enableResourceGiven);
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, resourceGivenYPos, 50, 50), "Gives Resource");
        var resourceGivenRect = new Rect(position.x, resourceGivenYPos + 20, 5, 16);
        var minResourceRect = new Rect(position.x + 120, resourceGivenYPos + 20, -50, 16);
        var maxResourceRect = new Rect(position.x + 180, resourceGivenYPos + 20, -50, 16);
        EditorGUI.PropertyField(resourceGivenRect, property.FindPropertyRelative("resourceGiven"), GUIContent.none);
        EditorGUI.PropertyField(minResourceRect, property.FindPropertyRelative("minResourceGiven"), GUIContent.none);
        EditorGUI.PropertyField(maxResourceRect, property.FindPropertyRelative("maxResourceGiven"), GUIContent.none);
        EditorGUI.indentLevel = 0;
        EditorGUI.EndDisabledGroup();

        if (!enableResourceGiven) {
            property.FindPropertyRelative("resourceGiven").enumValueIndex = 0;
            property.FindPropertyRelative("minResourceGiven").intValue = 0;
            property.FindPropertyRelative("maxResourceGiven").intValue = 0;
        }

        EditorGUI.BeginDisabledGroup(!enableResourceNeeded);
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, resourceNeededYPos, 50, 50), "Needs Resource");
        var resourceNeededRect = new Rect(position.x, resourceNeededYPos + 20, 5, 16);
        var resourceAmountNeededRect = new Rect(position.x + 120, resourceNeededYPos + 20, -50, 16);
        EditorGUI.PropertyField(resourceNeededRect, property.FindPropertyRelative("resourceNeeded"), GUIContent.none);
        EditorGUI.PropertyField(resourceAmountNeededRect, property.FindPropertyRelative("resourceAmountNeeded"), GUIContent.none);
        EditorGUI.indentLevel = 0;
        EditorGUI.EndDisabledGroup();

        if (!enableResourceNeeded) {
            property.FindPropertyRelative("resourceNeeded").enumValueIndex = 0;
            property.FindPropertyRelative("resourceAmountNeeded").intValue = 0;
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) * 10;
    }

    private bool HasSuccessRate(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.HUNT:
                return true;
            case ACTION_TYPE.DESTROY:
                return true;
            default:
                return false;
        }
    }
    private bool HasDuration(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.HUNT:
                return true;
            case ACTION_TYPE.DESTROY:
                return true;
            case ACTION_TYPE.BUILD:
                return true;
            case ACTION_TYPE.REPAIR:
                return true;
            default:
                return false;
        }
    }
    private bool GivesResource(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.HARVEST:
                return true;
            default:
                return false;
        }
    }
    private bool NeedResource(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.REPAIR:
                return true;
            default:
                return false;
        }
    }
}