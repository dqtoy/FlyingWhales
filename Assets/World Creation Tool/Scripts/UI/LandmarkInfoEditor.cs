
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using worldcreator;

public class LandmarkInfoEditor : MonoBehaviour {

    private BaseLandmark landmark;

    [SerializeField] private InputField landmarkName;
    [SerializeField] private Text landmarkType;

    [Space(10)]
    [Header("Scenarios")]
    [SerializeField] private Dropdown interactionTypesDropdown;
    [SerializeField] private InputField scenarioWeightField;
    [SerializeField] private ScrollRect scenariosScrollView;
    [SerializeField] private GameObject scenarioItemPrefab;
    [SerializeField] private InputField eventTriggerField;
    [SerializeField] private InputField noEventTriggerField;

    public void Initialize() {
        interactionTypesDropdown.ClearOptions();
        interactionTypesDropdown.AddOptions(Utilities.GetEnumChoices<INTERACTION_TYPE>());
    }

    public void ShowLandmarkInfo(BaseLandmark landmark) {
        this.gameObject.SetActive(true);
        this.landmark = landmark;
        landmarkName.text = landmark.landmarkName;
        landmarkType.text = landmark.specificLandmarkType.ToString();
        UpdateScenarioInfo();
    }
    public void CloseMenu() {
        this.gameObject.SetActive(false);
    }

    #region Scenarios
    private void UpdateScenarioInfo() {
        Utilities.DestroyChildren(scenariosScrollView.content);
        foreach (KeyValuePair<INTERACTION_TYPE, int> kvp in landmark.scenarios.dictionary) {
            GameObject scenarioGO = GameObject.Instantiate(scenarioItemPrefab, scenariosScrollView.content);
            ScenarioWeightItem item = scenarioGO.GetComponent<ScenarioWeightItem>();
            item.SetData(landmark, kvp.Key, kvp.Value);
        }
        eventTriggerField.text = landmark.eventTriggerWeight.ToString();
        noEventTriggerField.text = landmark.noEventTriggerWeight.ToString();
    }
    public void AddInteraction() {
        INTERACTION_TYPE type = (INTERACTION_TYPE)System.Enum.Parse(typeof(INTERACTION_TYPE), interactionTypesDropdown.options[interactionTypesDropdown.value].text);
        int weight = 0;
        if (landmark.scenarios.dictionary.ContainsKey(type)) {
            //cannot add type, because it already exists in landmark dictionary
            WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Invalid interaction!", "Cannot add interaction type " + type.ToString() + " because landmark already has that type in it's scenarios");
        } else if (!System.Int32.TryParse(scenarioWeightField.text, out weight)) {
            //cannot add type, because it already exists in landmark dictionary
            WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Invalid weight!", "Please enter a weight value!");
        } else {
            weight = Mathf.Max(0, weight);
            landmark.scenarios.AddElement(type, weight);
            UpdateScenarioInfo();
        }
    }
    #endregion

    #region Change Handlers
    public void SetName(string newName) {
        landmark.SetName(newName);
    }
    public void SetEventTriggerWeight(string newWeight) {
        int weight = System.Int32.Parse(newWeight);
        landmark.SetEventTriggerWeight(weight);
    }
    public void SetNoEventTriggerWeight(string newWeight) {
        int weight = System.Int32.Parse(newWeight);
        landmark.SetNoEventTriggerWeight(weight);
    }
    public void SetMaxDailySupplyProductionAmount(string amountStr) {
        int amount = System.Int32.Parse(amountStr);
        landmark.SetMaxDailySupplyProductionAmount(amount);
    }
    public void SetMinDailySupplyProductionAmount(string amountStr) {
        int amount = System.Int32.Parse(amountStr);
        landmark.SetMinDailySupplyProductionAmount(amount);
    }
    public void SetInitialDefenderCount(string countStr) {
        int count = System.Int32.Parse(countStr);
        //landmark.SetInitialDefenderCount(count);
    }
    public void SetMaxDefenderCount(string countStr) {
        int count = System.Int32.Parse(countStr);
        //landmark.SetMaxDefenderCount(count);
    }
    #endregion
}
