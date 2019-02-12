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
    [SerializeField] private InputField residentCapacityField;
    [SerializeField] private InputField monthlySupplyField;
    [SerializeField] private InputField monthlyActionsField;
    [SerializeField] private InputField initialResidentsField;
    [SerializeField] private Dropdown possibleOccupantsRaceDropdown;
    [SerializeField] private Text occupantsSummary;
    [SerializeField] private InputField minDungeonSupplyField;
    [SerializeField] private InputField maxDungeonSupplyField;

    [Header("Race Spawns")]
    [SerializeField] private GameObject raceSpawnPrefab;
    [SerializeField] private ScrollRect raceSpawnScrollView;
    [SerializeField] private Dropdown initialRaceDropdown;
    [SerializeField] private Dropdown initialRaceSubTypeDropdown;

    [Header("Structures")]
    [SerializeField] private GameObject structureItemPrefab;
    [SerializeField] private ScrollRect structureItemsScrollView;
    [SerializeField] private Dropdown structureItemsDropdown;
    [SerializeField] private Text structuresSummary;

    public void Initialize() {
        possibleOccupantsRaceDropdown.ClearOptions();
        initialRaceDropdown.ClearOptions();
        initialRaceSubTypeDropdown.ClearOptions();
        structureItemsDropdown.ClearOptions();

        initialRaceDropdown.AddOptions(Utilities.GetEnumChoices<RACE>());
        initialRaceSubTypeDropdown.AddOptions(Utilities.GetEnumChoices<RACE_SUB_TYPE>(true));
        possibleOccupantsRaceDropdown.AddOptions(Utilities.GetEnumChoices<RACE>());
        structureItemsDropdown.AddOptions(Utilities.GetEnumChoices<STRUCTURE_TYPE>());
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
        //supplyCapacityField.text = currentArea.supplyCapacity.ToString();
        //initialSupplyField.text = currentArea.initialSupply.ToString();
        //residentCapacityField.text = currentArea.residentCapacity.ToString();
        monthlySupplyField.text = currentArea.monthlySupply.ToString();
        monthlyActionsField.text = currentArea.monthlyActions.ToString();
        initialResidentsField.text = currentArea.initialResidents.ToString();
        minDungeonSupplyField.text = currentArea.dungeonSupplyRangeMin.ToString();
        maxDungeonSupplyField.text = currentArea.dungeonSupplyRangeMax.ToString();
        //defaultRaceDropdown.value = Utilities.GetOptionIndex(defaultRaceDropdown, currentArea.defaultRace.ToString());
        occupantsSummary.text = string.Empty;
        for (int i = 0; i < currentArea.possibleOccupants.Count; i++) {
            RACE race = currentArea.possibleOccupants[i];
            occupantsSummary.text += "(" + race.ToString() + ")";
        }
        UpdateRaceSpawnData();
        UpdateStructures();
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
    //public void SetSupplyCapacity(string amountStr) {
    //    int amount;
    //    if (Int32.TryParse(amountStr, out amount)) {
    //        currentArea.SetSupplyCapacity(amount);
    //    }
    //}
    public void SetDefaultRace(int choice) {
        RACE result;
        //if (Enum.TryParse(defaultRaceDropdown.options[defaultRaceDropdown.value].text, out result)) {
        //    currentArea.SetDefaultRace(result);
        //}
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
    //public void SetInitialSupplies(string amountStr) {
    //    if (!string.IsNullOrEmpty(amountStr)) {
    //        currentArea.SetInitialSupplies(System.Int32.Parse(amountStr));
    //    }

    //}
    //public void SetResidentCapacity(string amountStr) {
    //    if (!string.IsNullOrEmpty(amountStr)) {
    //        currentArea.SetResidentCapacity(System.Int32.Parse(amountStr));
    //    }
    //}
    public void SetMonthlySupply(string amountStr) {
        if (!string.IsNullOrEmpty(amountStr)) {
            currentArea.SetMonthlySupply(System.Int32.Parse(amountStr));
        }
    }
    public void SetMonthlyActions(string amountStr) {
        if (!string.IsNullOrEmpty(amountStr)) {
            currentArea.SetMonthlyActions(System.Int32.Parse(amountStr));
        }
    }
    public void SetInitialResidents(string amountStr) {
        if (!string.IsNullOrEmpty(amountStr)) {
            currentArea.SetInitialResidents(System.Int32.Parse(amountStr));
        }
    }

    #endregion

    #region Race Spawns
    public void AddRemoveRaceSpawns() {
        RACE chosenRace = (RACE) Enum.Parse(typeof(RACE), initialRaceDropdown.options[initialRaceDropdown.value].text);
        RACE_SUB_TYPE chosenRaceSubType = (RACE_SUB_TYPE)Enum.Parse(typeof(RACE_SUB_TYPE), initialRaceSubTypeDropdown.options[initialRaceSubTypeDropdown.value].text);
        if (currentArea.HasRaceSetup(chosenRace, chosenRaceSubType)) {
            currentArea.RemoveRaceSetup(chosenRace, chosenRaceSubType);
        } else {
            currentArea.AddRaceSetup(chosenRace, chosenRaceSubType);
        }
        UpdateRaceSpawnData();
    }
    private void UpdateRaceSpawnData() {
        Utilities.DestroyChildren(raceSpawnScrollView.content);
        for (int i = 0; i < currentArea.initialSpawnSetup.Count; i++) {
            InitialRaceSetup setup = currentArea.initialSpawnSetup[i];
            GameObject go = GameObject.Instantiate(raceSpawnPrefab, raceSpawnScrollView.content);
            go.GetComponent<RaceSpawnItem>().SetSetup(setup);
        }
    }
    #endregion

    #region Structures
    private void UpdateStructures() {
        Utilities.DestroyChildren(structureItemsScrollView.content);
        structuresSummary.text = "Structures Summary:";
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in currentArea.structures) {
            int inside = 0;
            int outside = 0;
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                GameObject go = GameObject.Instantiate(structureItemPrefab, structureItemsScrollView.content);
                go.GetComponent<StructureItem>().SetStructure(structure);
                if (structure.isInside) {
                    inside++;
                } else {
                    outside++;
                }
            }
            structuresSummary.text += "\n" + keyValuePair.Key.ToString() + " - " + keyValuePair.Value.Count.ToString() + "(Inside - " + inside.ToString() + " Outside - " + outside.ToString() + ")";
        }
    }
    public void AddNewStructure() {
        STRUCTURE_TYPE chosenType = (STRUCTURE_TYPE)Enum.Parse(typeof(STRUCTURE_TYPE), structureItemsDropdown.options[structureItemsDropdown.value].text);
        LandmarkManager.Instance.CreateNewStructureAt(currentArea, chosenType);
        UpdateStructures();
    }
    public void SetMinDungeonSupplyRange(string text) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }
        currentArea.SetDungeonSupplyMinRange(Int32.Parse(text));
    }
    public void SetMaxDungeonSupplyRange(string text) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }
        currentArea.SetDungeonSupplyMaxRange(Int32.Parse(text));
    }
    #endregion

}
