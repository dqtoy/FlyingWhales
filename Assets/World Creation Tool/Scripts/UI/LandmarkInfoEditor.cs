
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
    }
    public void CloseMenu() {
        this.gameObject.SetActive(false);
    }

    #region Change Handlers
    public void SetName(string newName) {
        landmark.SetName(newName);
    }
    //public void SetMaxDailySupplyProductionAmount(string amountStr) {
    //    int amount = System.Int32.Parse(amountStr);
    //    landmark.SetMaxDailySupplyProductionAmount(amount);
    //}
    //public void SetMinDailySupplyProductionAmount(string amountStr) {
    //    int amount = System.Int32.Parse(amountStr);
    //    landmark.SetMinDailySupplyProductionAmount(amount);
    //}
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
