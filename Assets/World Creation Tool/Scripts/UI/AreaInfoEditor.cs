using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfoEditor : MonoBehaviour {

    internal Area currentArea;

    [Header("Basic Info")]
    [SerializeField] private InputField areaNameField;
    [SerializeField] private InputField maxDefendersField;
    [SerializeField] private InputField initialDefenderGroupsField;
    [SerializeField] private InputField minInitialDefendersField;
    [SerializeField] private InputField maxInitialDefendersField;
    [SerializeField] private InputField initialDefenderLevelField;
    [SerializeField] private InputField supplyCapacityField;
    [SerializeField] private Dropdown defaultRaceDropdown;
    [SerializeField] private Dropdown possibleOccupantsRaceDropdown;
    [SerializeField] private Text occupantsSummary;

    public void Initialize() {
        defaultRaceDropdown.ClearOptions();
        possibleOccupantsRaceDropdown.ClearOptions();

        defaultRaceDropdown.AddOptions(Utilities.GetEnumChoices<RACE>());
        possibleOccupantsRaceDropdown.AddOptions(Utilities.GetEnumChoices<RACE>());
    }

    public void Show(Area area) {
        currentArea = area;
        this.gameObject.SetActive(true);
        LoadData();
    }
    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void LoadData() {
        areaNameField.text = currentArea.name;
        maxDefendersField.text = currentArea.maxDefenderGroups.ToString();
        initialDefenderGroupsField.text = currentArea.initialDefenderGroups.ToString();
        minInitialDefendersField.text = currentArea.minInitialDefendersPerGroup.ToString();
        maxInitialDefendersField.text = currentArea.maxInitialDefendersPerGroup.ToString();
        initialDefenderLevelField.text = currentArea.initialDefenderLevel.ToString();
        supplyCapacityField.text = currentArea.supplyCapacity.ToString();
        defaultRaceDropdown.value = Utilities.GetOptionIndex(defaultRaceDropdown, currentArea.defaultRace.ToString());
        occupantsSummary.text = string.Empty;
        for (int i = 0; i < currentArea.possibleOccupants.Count; i++) {
            RACE race = currentArea.possibleOccupants[i];
            occupantsSummary.text += "(" + race.ToString() + ")";
        }
    }

    #region Basic Info
    public void SetAreaName(string name) {
        currentArea.SetName(name);
    }
    public void SetMaxDefenderGroups(string amountStr) {
        int amount;
        if (Int32.TryParse(amountStr, out amount)) {
            currentArea.SetMaxDefenderGroups(amount);
        }
    }
    public void SetInitialDefenderGroups(string amountStr) {
        int amount;
        if (Int32.TryParse(amountStr, out amount)) {
            currentArea.SetInitialDefenderGroups(amount);
        }
    }
    public void SetMinInitialDefendersPerGroup(string amountStr) {
        int amount;
        if (Int32.TryParse(amountStr, out amount)) {
            currentArea.SetMinInitialDefendersPerGroup(amount);
        }
    }
    public void SetMaxInitialDefendersPerGroup(string amountStr) {
        int amount;
        if (Int32.TryParse(amountStr, out amount)) {
            currentArea.SetMaxInitialDefendersPerGroup(amount);
        }
    }
    public void SetInitialDefenderLevel(string amountStr) {
        int amount;
        if (Int32.TryParse(amountStr, out amount)) {
            currentArea.SetInitialDefenderLevel(amount);
        }
    }
    public void SetSupplyCapacity(string amountStr) {
        int amount;
        if (Int32.TryParse(amountStr, out amount)) {
            currentArea.SetSupplyCapacity(amount);
        }
    }
    public void SetDefaultRace(int choice) {
        RACE result;
        if (Enum.TryParse(defaultRaceDropdown.options[defaultRaceDropdown.value].text, out result)) {
            currentArea.SetDefaultRace(result);
        }
    }
    public void AddRemovePossibleOccupants() {
        RACE race = (RACE)Enum.Parse(typeof(RACE), possibleOccupantsRaceDropdown.options[possibleOccupantsRaceDropdown.value].text);
        if (currentArea.possibleOccupants.Contains(race)) {
            currentArea.RemovePossibleOccupant(race);
        } else {
            currentArea.AddPossibleOccupant(race);
        }
        LoadData();
    }
    #endregion

}
