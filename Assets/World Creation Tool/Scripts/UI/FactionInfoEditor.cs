using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FactionInfoEditor : MonoBehaviour {

    private Faction _faction;

    [SerializeField] private InputField nameInputField;
    [SerializeField] private Text charactersSummaryText;
    [SerializeField] private Text areaSummaryText;
    [SerializeField] private Dropdown areasDropdown;
    [SerializeField] private Dropdown leadersDropdown;

    public void ShowFactionInfo(Faction faction) {
        _faction = faction;
        LoadAreaChoices();
        LoadLeaderChoices();
        UpdateInfo();
        UpdateAreas();
        this.gameObject.SetActive(true);
    }

    public void CloseMenu() {
        this.gameObject.SetActive(false);
    }

    private void UpdateInfo() {
        nameInputField.text = _faction.name;
        //characters
        charactersSummaryText.text = string.Empty;
        for (int i = 0; i < _faction.characters.Count; i++) {
            ECS.Character currCharacter = _faction.characters[i];
            charactersSummaryText.text += currCharacter.name + "\n";
        }

        if (_faction.leader != null) {
            leadersDropdown.value = Utilities.GetOptionIndex(leadersDropdown, _faction.leader.name);
            leadersDropdown.itemText.text = _faction.leader.name;
        }
    }
    private void UpdateAreas() {
        areaSummaryText.text = string.Empty;
        for (int i = 0; i < _faction.ownedAreas.Count; i++) {
            Area currArea = _faction.ownedAreas[i];
            areaSummaryText.text += currArea.name + "\n";
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
    public void ChangeFactionName(string newName) {
        _faction.SetName(newName);
    }

    private void LoadLeaderChoices() {
        leadersDropdown.ClearOptions();
        leadersDropdown.AddOptions(_faction.characters.Select(x => x.name).ToList());
    }
    public void SetLeader(int choice) {
        string characterName = leadersDropdown.options[leadersDropdown.value].text;
        ECS.Character character = CharacterManager.Instance.GetCharacterByName(characterName);
        _faction.SetLeader(character);
    }

}
