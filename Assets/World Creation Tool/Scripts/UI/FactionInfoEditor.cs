using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;
using worldcreator;

public class FactionInfoEditor : MonoBehaviour {

    private Faction _faction;

    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField descriptionInputField;
    [SerializeField] private InputField levelInputField;
    [SerializeField] private Text charactersSummaryLbl;
    [SerializeField] private Text areaSummaryLbl;
    [SerializeField] private Text recruitableRacesSummaryLbl;
    [SerializeField] private Text startingFollowersSummaryLbl;
    [SerializeField] private Dropdown areasDropdown;
    [SerializeField] private Dropdown leadersGenderDropdown;
    [SerializeField] private Dropdown leadersRaceDropdown;
    [SerializeField] private Dropdown leadersClassDropdown;
    [SerializeField] private Dropdown emblemDropdown;
    [SerializeField] private Dropdown moralityDropdown;
    [SerializeField] private Dropdown raceDropdown;
    [SerializeField] private Dropdown subRaceDropdown;
    [SerializeField] private Dropdown recruitableRacesDropdown;
    [SerializeField] private Dropdown startingFollowersDropdown;
    [SerializeField] private Image factionColor;
    [SerializeField] private ColorPickerControl factionColorPicker;
    [SerializeField] private Toggle isActiveToggle;

    [Header("Relationships")]
    [SerializeField] private Text relationshipSummaryLbl;
    [SerializeField] private Dropdown factionsDropdown;
    [SerializeField] private Dropdown relStatDropdown;

    //[Header("Favor")]
    //[SerializeField] private Text favorSummaryLbl;
    //[SerializeField] private Dropdown factionsFavorDropdown;
    //[SerializeField] private InputField favorAmountField;

    //[Header("Defenders")]
    //[SerializeField] private ScrollRect defendersScrollView;
    //[SerializeField] private GameObject defenderWeightItemPrefab;
    //[SerializeField] private Dropdown defenderClassDropdown;
    //[SerializeField] private InputField defenderWeightField;

    public void ShowFactionInfo(Faction faction) {
        _faction = faction;
        LoadAreaChoices();
        LoadLeaderChoices();
        LoadRelationshipChoices();
        LoadEmblemChoices();
        LoadMoralityChoices();
        LoadRaceChoices();
        LoadRecruitableRacesChoices();
        LoadStartingFollowersChoices();
        //LoadFavorChoices();
        //LoadDefenderChoices();
        UpdateBasicInfo();
        UpdateAreas();
        UpdateRelationshipInfo();
        UpdateRecruitableRaces();
        UpdateStartingFollowers();
        //UpdateFavorInfo();
        //UpdateDefenderWeights();
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
        //LoadFavorChoices();
        //UpdateFavorInfo();
    }
    public void OnFactionDeleted(Faction deletedFaction) {
        if (_faction == null) {
            return;
        }
        LoadRelationshipChoices();
        UpdateRelationshipInfo();
        //LoadFavorChoices();
        //UpdateFavorInfo();
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

        leadersGenderDropdown.value = Utilities.GetOptionIndex(leadersGenderDropdown, _faction.initialLeaderGender.ToString());
        leadersRaceDropdown.value = Utilities.GetOptionIndex(leadersRaceDropdown, _faction.initialLeaderRace.ToString());
        leadersClassDropdown.value = Utilities.GetOptionIndex(leadersClassDropdown, _faction.initialLeaderClass);

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
        LandmarkManager.Instance.OwnArea(_faction, _faction.raceType, chosenArea);
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
        leadersGenderDropdown.ClearOptions();
        leadersRaceDropdown.ClearOptions();
        leadersClassDropdown.ClearOptions();

        string[] genders = System.Enum.GetNames(typeof(GENDER));
        string[] races = System.Enum.GetNames(typeof(RACE));
        List<string> classes = new List<string>();
        string path = Utilities.dataPath + "CharacterClasses/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            classes.Add(Path.GetFileNameWithoutExtension(file));
        }

        leadersGenderDropdown.AddOptions(genders.ToList());
        leadersRaceDropdown.AddOptions(races.ToList());
        leadersClassDropdown.AddOptions(classes.ToList());
    }
    public void SetLeader(int choice) {
        string characterName = leadersGenderDropdown.options[leadersGenderDropdown.value].text;
        Character character = CharacterManager.Instance.GetCharacterByName(characterName);
        if (character != null) {
            _faction.SetLeader(character);
        } else {
            _faction.SetLeader(null); ;
        }
    }
    public void SetLeaderGender(int choice) {
        GENDER gender = (GENDER) System.Enum.Parse(typeof(GENDER), leadersGenderDropdown.options[leadersGenderDropdown.value].text);
        _faction.SetInitialFactionLeaderGender(gender);
    }
    public void SetLeaderRace(int choice) {
        RACE race = (RACE) System.Enum.Parse(typeof(RACE), leadersRaceDropdown.options[leadersRaceDropdown.value].text);
        _faction.SetInitialFactionLeaderRace(race);
    }
    public void SetLeaderClass(int choice) {
        string className = leadersClassDropdown.options[leadersClassDropdown.value].text;
        _faction.SetInitialFactionLeaderClass(className);
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

        _faction.GetRelationshipWith(faction).SetRelationshipStatus(relStat);
        UpdateRelationshipInfo();
    }
    #endregion

    #region Emblems
    private void LoadEmblemChoices() {
        emblemDropdown.ClearOptions();
        List<Dropdown.OptionData> emblemOptions = new List<Dropdown.OptionData>();
        for (int i = 0; i < FactionManager.Instance.factionEmblems.Count; i++) {
            FactionEmblemSetting currEmblem = FactionManager.Instance.factionEmblems[i];
            Dropdown.OptionData currData = new Dropdown.OptionData();
            currData.image = currEmblem.GetSpriteForSize(96);
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

    #region Recruitable Races
    private void UpdateRecruitableRaces() {
        recruitableRacesSummaryLbl.text = string.Empty;
        if(_faction.recruitableRaces == null) {
            return;
        }
        for (int i = 0; i < _faction.recruitableRaces.Count; i++) {
            RACE currRace = _faction.recruitableRaces[i];
            recruitableRacesSummaryLbl.text += currRace.ToString() + "\n";
        }
    }
    private void LoadRecruitableRacesChoices() {
        recruitableRacesDropdown.ClearOptions();
        string[] races = System.Enum.GetNames(typeof(RACE));
        recruitableRacesDropdown.AddOptions(races.ToList());
    }
    public void AddRecruitableRace() {
        RACE race = (RACE) System.Enum.Parse(typeof(RACE), recruitableRacesDropdown.options[recruitableRacesDropdown.value].text);
        _faction.recruitableRaces.Add(race);
        UpdateRecruitableRaces();
    }
    public void RemoveRecruitableRace() {
        RACE race = (RACE) System.Enum.Parse(typeof(RACE), recruitableRacesDropdown.options[recruitableRacesDropdown.value].text);
        _faction.recruitableRaces.Remove(race);
        UpdateRecruitableRaces();
    }
    #endregion
    #region Starting Followers
    private void UpdateStartingFollowers() {
        startingFollowersSummaryLbl.text = string.Empty;
        if (_faction.startingFollowers == null) {
            return;
        }
        for (int i = 0; i < _faction.startingFollowers.Count; i++) {
            RACE currRace = _faction.startingFollowers[i];
            startingFollowersSummaryLbl.text += currRace.ToString() + "\n";
        }
    }
    private void LoadStartingFollowersChoices() {
        startingFollowersDropdown.ClearOptions();
        string[] races = System.Enum.GetNames(typeof(RACE));
        startingFollowersDropdown.AddOptions(races.ToList());
    }
    public void AddStartingFollower() {
        RACE race = (RACE) System.Enum.Parse(typeof(RACE), startingFollowersDropdown.options[startingFollowersDropdown.value].text);
        _faction.startingFollowers.Add(race);
        UpdateStartingFollowers();
    }
    public void RemoveStartingFollower() {
        RACE race = (RACE) System.Enum.Parse(typeof(RACE), startingFollowersDropdown.options[startingFollowersDropdown.value].text);
        _faction.startingFollowers.Remove(race);
        UpdateStartingFollowers();
    }
    #endregion
}
