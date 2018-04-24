using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct CharacterActionData {
    public ACTION_TYPE actionType;
    public string actionName;

    public int advertisedFullness;
    public int advertisedJoy;
    public int advertisedEnergy;
    public int advertisedPrestige;

    public int providedFullness;
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

    public UnityEvent successFunction;
    public UnityEvent failFunction;

}

[CustomPropertyDrawer(typeof(CharacterActionData))]
public class CharacterActionDrawer : PropertyDrawer {
    private bool enableSuccessRate, enableDuration, enableResourceGiven, enableResourceNeeded;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var actionTypeRect = new Rect(position.x, position.y, 100, 16);
        var actionNameRect = new Rect(position.x + 180, position.y, 70, 16);

        EditorGUI.PropertyField(actionTypeRect, property.FindPropertyRelative("actionType"), GUIContent.none);
        //EditorGUI.LabelField(new Rect(position.x + 135, position.y, 50, 16), "Name");
        //EditorGUI.PropertyField(actionNameRect, property.FindPropertyRelative("actionName"), GUIContent.none);

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

        EditorGUI.PropertyField(aHungerRect, property.FindPropertyRelative("advertisedFullness"), GUIContent.none);
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

        EditorGUI.PropertyField(pHungerRect, property.FindPropertyRelative("providedFullness"), GUIContent.none);
        EditorGUI.PropertyField(pJoyRect, property.FindPropertyRelative("providedJoy"), GUIContent.none);
        EditorGUI.PropertyField(pEnergyRect, property.FindPropertyRelative("providedEnergy"), GUIContent.none);
        EditorGUI.PropertyField(pPrestigeRect, property.FindPropertyRelative("providedPrestige"), GUIContent.none);


        SerializedProperty successFunctionProperty = property.FindPropertyRelative("successFunction");
        SerializedProperty failFunctionProperty = property.FindPropertyRelative("failFunction");

        EditorGUI.LabelField(new Rect(position.x - 120, position.y + 80, 60, 50), "On Success");
        var pSuccessFunctionRect = new Rect(position.x - 120, position.y + 100, 350, 100);
        EditorGUI.PropertyField(pSuccessFunctionRect, successFunctionProperty, GUIContent.none);

        var pFailFunctionLabelRect = new Rect(pSuccessFunctionRect.x, pSuccessFunctionRect.y + pSuccessFunctionRect.height, 60, 50);
        var pFailFunctionRect = new Rect(pFailFunctionLabelRect.x, pFailFunctionLabelRect.y + 20, 350, 100);
        EditorGUI.LabelField(pFailFunctionLabelRect, "On Fail");
        EditorGUI.PropertyField(pFailFunctionRect, failFunctionProperty, GUIContent.none);

        ACTION_TYPE actionType = (ACTION_TYPE)property.FindPropertyRelative("actionType").enumValueIndex;

        enableSuccessRate = HasSuccessRate(actionType);
        enableDuration = HasDuration(actionType);
        enableResourceGiven = GivesResource(actionType);
        enableResourceNeeded = NeedResource(actionType);

        float defaultSuccessRateYPos = position.y + 80;
        float defaultDurationYPos = position.y + 100;
        float defaultResourceGivenYPos = position.y + 110;
        float defaultResourceNeededYPos = position.y + 160;

        float successRateYPos = defaultSuccessRateYPos;
        float durationYPos = defaultDurationYPos;
        float resourceGivenYPos = defaultResourceGivenYPos;
        float resourceNeededYPos = defaultResourceNeededYPos;

        //Success Rate
        if (enableSuccessRate) {
            LoadSuccessRateField(successRateYPos, position, property, label);
        } else {
            property.FindPropertyRelative("successRate").intValue = 0;
        }

        //Duration
        if (enableDuration) {
            if (!enableSuccessRate) {
                durationYPos = defaultSuccessRateYPos;
            }
            LoadDurationField(durationYPos, position, property, label);
        } else {
            property.FindPropertyRelative("duration").intValue = 0;
        }

        //Resource Given
        if (enableResourceGiven) {
            if (!enableDuration) {
                resourceGivenYPos = defaultDurationYPos;
                if (!enableSuccessRate) {
                    resourceGivenYPos = defaultSuccessRateYPos;
                }
            }
            LoadResourceGivenField(resourceGivenYPos, position, property, label);
        } else {
            property.FindPropertyRelative("resourceGiven").enumValueIndex = 0;
            property.FindPropertyRelative("minResourceGiven").intValue = 0;
            property.FindPropertyRelative("maxResourceGiven").intValue = 0;
        }

        //Resource Needed
        if (enableResourceNeeded) {
            if (!enableResourceGiven) {
                resourceNeededYPos = defaultResourceGivenYPos;
                if (!enableDuration) {
                    resourceNeededYPos = defaultDurationYPos;
                    if (!enableSuccessRate) {
                        resourceNeededYPos = defaultSuccessRateYPos;
                    }
                }
            }
            LoadNeedsResourceField(resourceNeededYPos, position, property, label);
        } else {
            property.FindPropertyRelative("resourceNeeded").enumValueIndex = 0;
            property.FindPropertyRelative("resourceAmountNeeded").intValue = 0;
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        int modifier = 5;
        if (enableSuccessRate) {
            modifier += 2;
        }
        if (enableDuration) {
            modifier += 2;
        }
        if (enableResourceGiven) {
            modifier += 3;
        }
        if (enableResourceNeeded) {
            modifier += 3;
        }
        return base.GetPropertyHeight(property, label) * modifier;
    }

    #region Special Fields
    private void LoadSuccessRateField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, yPos, 50, 50), "Success Rate");
        var successRateRect = new Rect(position.x + 90, yPos, 80, 16);
        EditorGUI.PropertyField(successRateRect, property.FindPropertyRelative("successRate"), GUIContent.none);
        EditorGUI.indentLevel = 0;
    }
    private void LoadDurationField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, yPos, 50, 50), "Duration");
        var durationRect = new Rect(position.x + 90, yPos, 80, 16);
        EditorGUI.PropertyField(durationRect, property.FindPropertyRelative("duration"), GUIContent.none);
        EditorGUI.indentLevel = 0;
    }
    private void LoadResourceGivenField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, yPos, 50, 50), "Gives Resource");
        var resourceGivenRect = new Rect(position.x, yPos + 20, 5, 16);
        var minResourceRect = new Rect(position.x + 120, yPos + 20, -50, 16);
        var maxResourceRect = new Rect(position.x + 180, yPos + 20, -50, 16);
        EditorGUI.PropertyField(resourceGivenRect, property.FindPropertyRelative("resourceGiven"), GUIContent.none);
        EditorGUI.PropertyField(minResourceRect, property.FindPropertyRelative("minResourceGiven"), GUIContent.none);
        EditorGUI.PropertyField(maxResourceRect, property.FindPropertyRelative("maxResourceGiven"), GUIContent.none);
        EditorGUI.indentLevel = 0;
    }
    private void LoadNeedsResourceField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.indentLevel = -7;
        EditorGUI.LabelField(new Rect(position.x, yPos, 50, 50), "Needs Resource");
        var resourceNeededRect = new Rect(position.x, yPos + 20, 5, 16);
        var resourceAmountNeededRect = new Rect(position.x + 120, yPos + 20, -50, 16);
        EditorGUI.PropertyField(resourceNeededRect, property.FindPropertyRelative("resourceNeeded"), GUIContent.none);
        EditorGUI.PropertyField(resourceAmountNeededRect, property.FindPropertyRelative("resourceAmountNeeded"), GUIContent.none);
        EditorGUI.indentLevel = 0;
    }
    #endregion

    #region Special Field Conditions
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
    #endregion
}