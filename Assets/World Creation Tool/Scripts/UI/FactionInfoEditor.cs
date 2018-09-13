using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;

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

    [Header("Relationships")]
    [SerializeField] private Text relationshipSummaryLbl;
    [SerializeField] private Dropdown factionsDropdown;
    [SerializeField] private Dropdown relStatDropdown;

    public void ShowFactionInfo(Faction faction) {
        _faction = faction;
        LoadAreaChoices();
        LoadLeaderChoices();
        LoadRelationshipChoices();
        LoadEmblemChoices();
        UpdateBasicInfo();
        UpdateAreas();
        UpdateRelationshipInfo();
        this.gameObject.SetActive(true);
    }

    public void CloseMenu() {
        this.gameObject.SetActive(false);
    }

    private void UpdateBasicInfo() {
        nameInputField.text = _faction.name;
        descriptionInputField.text = _faction.description;
        factionColor.color = _faction.factionColor;
        factionColorPicker.CurrentColor = _faction.factionColor;
        //characters
        charactersSummaryLbl.text = string.Empty;
        for (int i = 0; i < _faction.characters.Count; i++) {
            ECS.Character currCharacter = _faction.characters[i];
            charactersSummaryLbl.text += currCharacter.name + "\n";
        }

        if (_faction.leader != null) {
            leadersDropdown.value = Utilities.GetOptionIndex(leadersDropdown, _faction.leader.name);
            leadersDropdown.itemText.text = _faction.leader.name;
        }

        emblemDropdown.value = Utilities.GetOptionIndex(emblemDropdown, FactionManager.Instance.GetFactionEmblemIndex(_faction.emblem).ToString());
    }

    public void OnNewFactionCreated(Faction newFaction) {
        if (_faction == null) {
            return;
        }
        LoadRelationshipChoices();
        UpdateRelationshipInfo();
    }
    public void OnFactionDeleted(Faction deletedFaction) {
        if (_faction == null) {
            return;
        }
        LoadRelationshipChoices();
        UpdateRelationshipInfo();
    }

    #region Basic Info
    public void ChangeFactionName(string newName) {
        _faction.SetName(newName);
    }
    public void ChangeDescriptionName(string description) {
        _faction.SetDescription(description);
    }
    public void ChangeFactionColor(Color color) {
        _faction.SetFactionColor(color);
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
        leadersDropdown.AddOptions(_faction.characters.Select(x => x.name).ToList());
    }
    public void SetLeader(int choice) {
        string characterName = leadersDropdown.options[leadersDropdown.value].text;
        ECS.Character character = CharacterManager.Instance.GetCharacterByName(characterName);
        _faction.SetLeader(character);
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
}
