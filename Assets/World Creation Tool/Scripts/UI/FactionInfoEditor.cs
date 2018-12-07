using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;
using worldcreator;

public class FactionInfoEditor : MonoBehaviour {

    private Faction _faction;

    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField descriptionInputField;
    [SerializeField] private Text charactersSummaryLbl;
    [SerializeField] private Text areaSummaryLbl;
    [SerializeField] private Dropdown areasDropdown;
    [SerializeField] private Dropdown leadersDropdown;
    [SerializeField] private Image factionColor;
    [SerializeField] private ColorPickerControl factionColorPicker;
    [SerializeField] private Dropdown emblemDropdown;
    [SerializeField] private Dropdown moralityDropdown;
    [SerializeField] private Dropdown raceDropdown;
    [SerializeField] private Dropdown subRaceDropdown;
    [SerializeField] private InputField levelInputField;
    [SerializeField] private Toggle isActiveToggle;

    [Header("Relationships")]
    [SerializeField] private Text relationshipSummaryLbl;
    [SerializeField] private Dropdown factionsDropdown;
    [SerializeField] private Dropdown relStatDropdown;

    [Header("Favor")]
    [SerializeField] private Text favorSummaryLbl;
    [SerializeField] private Dropdown factionsFavorDropdown;
    [SerializeField] private InputField favorAmountField;

    [Header("Defenders")]
    [SerializeField] private ScrollRect defendersScrollView;
    [SerializeField] private GameObject defenderWeightItemPrefab;
    [SerializeField] private Dropdown defenderClassDropdown;
    [SerializeField] private InputField defenderWeightField;

    public void ShowFactionInfo(Faction faction) {
        _faction = faction;
        LoadAreaChoices();
        LoadLeaderChoices();
        LoadRelationshipChoices();
        LoadEmblemChoices();
        LoadMoralityChoices();
        LoadRaceChoices();
        LoadFavorChoices();
        LoadDefenderChoices();
        UpdateBasicInfo();
        UpdateAreas();
        UpdateRelationshipInfo();
        UpdateFavorInfo();
        UpdateDefenderWeights();
        this.gameObject.SetActive(true);
    }

    public void CloseMenu() {
        this.gameObject.SetActive(false);
    }

    public void OnNewFactionCreated(Faction newFaction) {
        if (_faction == null) {
            return;
        }
        LoadRelationshipChoices();
        UpdateRelationshipInfo();
        LoadFavorChoices();
        UpdateFavorInfo();
    }
    public void OnFactionDeleted(Faction deletedFaction) {
        if (_faction == null) {
            return;
        }
        LoadRelationshipChoices();
        UpdateRelationshipInfo();
        LoadFavorChoices();
        UpdateFavorInfo();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        nameInputField.text = _faction.name;
        descriptionInputField.text = _faction.description;
        factionColor.color = _faction.factionColor;
        factionColorPicker.CurrentColor = _faction.factionColor;
        //characters
        charactersSummaryLbl.text = string.Empty;
        for (int i = 0; i < _faction.characters.Count; i++) {
            Character currCharacter = _faction.characters[i];
            charactersSummaryLbl.text += currCharacter.name + "\n";
        }

        if (_faction.leader != null) {
            leadersDropdown.value = Utilities.GetOptionIndex(leadersDropdown, _faction.leader.name);
            //leadersDropdown.itemText.text = _faction.leader.name;
        } else {
            leadersDropdown.value = Utilities.GetOptionIndex(leadersDropdown, "None");
        }

        emblemDropdown.value = Utilities.GetOptionIndex(emblemDropdown, FactionManager.Instance.GetFactionEmblemIndex(_faction.emblem).ToString());
        moralityDropdown.value = Utilities.GetOptionIndex(moralityDropdown, _faction.morality.ToString());
        raceDropdown.value = Utilities.GetOptionIndex(raceDropdown, _faction.raceType.ToString());
        subRaceDropdown.value = Utilities.GetOptionIndex(subRaceDropdown, _faction.subRaceType.ToString());
        levelInputField.text = _faction.level.ToString();
        isActiveToggle.isOn = _faction.isActive;
    }
    public void ChangeFactionName(string newName) {
        _faction.SetName(newName);
    }
    public void ChangeFactionLevel(string input) {
        if (!string.IsNullOrEmpty(input)) {
            _faction.SetLevel(Int32.Parse(input));
        }
    }
    public void SetFactionActiveState(bool state) {
        _faction.SetFactionActiveState(state);
    }
    public void ChangeDescriptionName(string description) {
        _faction.SetDescription(description);
    }
    public void ChangeFactionColor(Color color) {
        _faction.SetFactionColor(color);
    }
    private void LoadMoralityChoices() {
        moralityDropdown.ClearOptions();
        moralityDropdown.AddOptions(Utilities.GetEnumChoices<MORALITY>());
    }
    public void ChangeMorality(int choice) {
        string chosen = moralityDropdown.options[choice].text;
        MORALITY morality = (MORALITY)System.Enum.Parse(typeof(MORALITY), chosen);
        _faction.SetMorality(morality);
    }
    private void LoadRaceChoices() {
        raceDropdown.ClearOptions();
        raceDropdown.AddOptions(Utilities.GetEnumChoices<RACE>());

        subRaceDropdown.ClearOptions();
        subRaceDropdown.AddOptions(Utilities.GetEnumChoices<RACE_SUB_TYPE>());
    }
    public void ChangeRace(int choice) {
        string chosen = raceDropdown.options[choice].text;
        RACE race = (RACE)System.Enum.Parse(typeof(RACE), chosen);
        _faction.SetRaceType(race);
    }
    public void ChangeSubRace(int choice) {
        string chosen = subRaceDropdown.options[choice].text;
        RACE_SUB_TYPE race = (RACE_SUB_TYPE)System.Enum.Parse(typeof(RACE_SUB_TYPE), chosen);
        _faction.SetSubRaceType(race);
    }
    #endregion

    #region Areas
    private void UpdateAreas() {
        areaSummaryLbl.text = string.Empty;
        for (int i = 0; i < _faction.ownedAreas.Count; i++) {
            Area currArea = _faction.ownedAreas[i];
            areaSummaryLbl.text += currArea.name + "\n";
        }
    }
    private void LoadAreaChoices() {
        areasDropdown.ClearOptions();
        List<string> choices = new List<string>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            choices.Add(currArea.name);
        }
        areasDropdown.AddOptions(choices);
    }
    public void AddArea() {
        string chosenAreaName = areasDropdown.options[areasDropdown.value].text;
        Area chosenArea = LandmarkManager.Instance.GetAreaByName(chosenAreaName);
        LandmarkManager.Instance.OwnArea(_faction, chosenArea);
        UpdateAreas();
    }
    public void RemoveArea() {
        string chosenAreaName = areasDropdown.options[areasDropdown.value].text;
        Area chosenArea = LandmarkManager.Instance.GetAreaByName(chosenAreaName);
        LandmarkManager.Instance.UnownArea(chosenArea);
        UpdateAreas();
    }
    #endregion

    #region Leader
    private void LoadLeaderChoices() {
        leadersDropdown.ClearOptions();
        List<string> choices = new List<string>();
        choices.Add("None");
        choices.AddRange(_faction.characters.Select(x => x.name));
        leadersDropdown.AddOptions(choices);
    }
    public void SetLeader(int choice) {
        string characterName = leadersDropdown.options[leadersDropdown.value].text;
        Character character = CharacterManager.Instance.GetCharacterByName(characterName);
        if (character != null) {
            _faction.SetLeader(character);
        } else {
            _faction.SetLeader(null); ;
        }
        
    }
    #endregion

    #region Relationships
    private void LoadRelationshipChoices() {
        factionsDropdown.ClearOptions();
        List<string> factionOnptions = FactionManager.Instance.allFactions.Where(x => x.id != _faction.id).Select(x => x.name).ToList();
        factionsDropdown.AddOptions(factionOnptions);

        relStatDropdown.ClearOptions();
        relStatDropdown.AddOptions(Utilities.GetEnumChoices<FACTION_RELATIONSHIP_STATUS>());
    } 
    private void UpdateRelationshipInfo() {
        string text = string.Empty;
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in _faction.relationships) {
            text += kvp.Key.name + " - " + kvp.Value.relationshipStatus.ToString() + "\n";
        }
        relationshipSummaryLbl.text = text;

    }
    public void ApplyRelationshipStatus() {
        string factionName = factionsDropdown.options[factionsDropdown.value].text;
        string relStatString = relStatDropdown.options[relStatDropdown.value].text;

        Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionName);
        FACTION_RELATIONSHIP_STATUS relStat = (FACTION_RELATIONSHIP_STATUS)System.Enum.Parse(typeof(FACTION_RELATIONSHIP_STATUS), relStatString);

        _faction.GetRelationshipWith(faction).ChangeRelationshipStatus(relStat);
        UpdateRelationshipInfo();
    }
    #endregion

    #region Emblems
    private void LoadEmblemChoices() {
        emblemDropdown.ClearOptions();
        List<Dropdown.OptionData> emblemOptions = new List<Dropdown.OptionData>();
        for (int i = 0; i < FactionManager.Instance.factionEmblems.Count; i++) {
            Sprite currEmblem = FactionManager.Instance.factionEmblems[i];
            Dropdown.OptionData currData = new Dropdown.OptionData();
            currData.image = currEmblem;
            currData.text = i.ToString();
            emblemOptions.Add(currData);
        }
        emblemDropdown.AddOptions(emblemOptions);
    }
    public void SetFactionEmblem(int choice) {
        //int chosenBGIndex = System.Int32.Parse(emblemDropdown.options[choice].text);
        _faction.SetEmblem(FactionManager.Instance.GetFactionEmblem(choice));
    }
    #endregion

    #region Favor
    private void LoadFavorChoices() {
        factionsFavorDropdown.ClearOptions();
        List<string> factionOnptions = FactionManager.Instance.allFactions.Where(x => x.id != _faction.id).Select(x => x.name).ToList();
        factionsFavorDropdown.AddOptions(factionOnptions);
    }
    private void UpdateFavorInfo() {
        string text = string.Empty;
        foreach (KeyValuePair<Faction, int> kvp in _faction.favor) {
            text += kvp.Key.name + " - " + kvp.Value.ToString() + "\n";
        }
        favorSummaryLbl.text = text;
    }
    public void ApplyFavor() {
        string factionName = factionsFavorDropdown.options[factionsFavorDropdown.value].text;
        string favorAmountStr = favorAmountField.text;

        Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionName);
        int favorAmount = Int32.Parse(favorAmountStr);

        _faction.AddNewFactionFavor(faction, favorAmount);
        UpdateFavorInfo();
    }
    #endregion

    #region Defenders
    private void LoadDefenderChoices() {
        defenderClassDropdown.ClearOptions();
        defenderClassDropdown.AddOptions(Utilities.GetFileChoices(Utilities.dataPath + "CharacterClasses/", "*.json"));
    }
    private void UpdateDefenderWeights() {
        Utilities.DestroyChildren(defendersScrollView.content);
        foreach (KeyValuePair<AreaCharacterClass, int> kvp in _faction.defenderWeights.dictionary) {
            GameObject itemGO = GameObject.Instantiate(defenderWeightItemPrefab, defendersScrollView.content);
            DefenderWeightItem item = itemGO.GetComponent<DefenderWeightItem>();
            item.SetDefender(_faction, kvp.Key, kvp.Value);
        }
    }
    public void AddDefenderWeight() {
        string defenderClass = defenderClassDropdown.options[defenderClassDropdown.value].text;
        int weight = 0;
        if (HasDefenderWeightForClass(defenderClass)) {
            WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Invalid defender class!", "Cannot add defender class " + defenderClass + " because landmark already has that type in it's defender weights");
        } else if (!System.Int32.TryParse(defenderWeightField.text, out weight)) {
            WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Invalid weight!", "Please enter a weight value!");
        } else {
            weight = Mathf.Max(0, weight);
            _faction.defenderWeights.AddElement(new AreaCharacterClass() { className = defenderClass }, weight);
            UpdateDefenderWeights();
        }
    }
    private bool HasDefenderWeightForClass(string className) {
        foreach (KeyValuePair<AreaCharacterClass, int> kvp in _faction.defenderWeights.dictionary) {
            if (kvp.Key.className.Equals(className)) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
