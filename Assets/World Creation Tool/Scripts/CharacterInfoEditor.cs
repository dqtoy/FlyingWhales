using BayatGames.SaveGameFree;
using ECS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class CharacterInfoEditor : MonoBehaviour {

    private Character _character;

    [Header("Portrait Settings")]
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private Dropdown templatesDropdown;

    [Header("Basic Info")]
    [SerializeField] private InputField nameField;
    [SerializeField] private Dropdown raceField;
    [SerializeField] private Dropdown genderField;
    [SerializeField] private Dropdown roleField;
    [SerializeField] private Dropdown classField;
    [SerializeField] private Text otherInfoLbl;

    [Header("Relationship Info")]
    [SerializeField] private GameObject relationshipItemPrefab;
    [SerializeField] private ScrollRect relationshipScrollView;
    [SerializeField] private Dropdown charactersRelationshipDropdown;
    [SerializeField] private Button createRelationshipBtn;

    public Dictionary<string, PortraitSettings> portraitTemplates;

    #region Monobehaviours
    private void Awake() {
        Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_CREATED, OnRelationshipCreated);
        Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_REMOVED, OnRelationshipRemoved);

        LoadTemplateChoices();
        LoadDropdownOptions();
    }
    #endregion

    public void ShowCharacterInfo(Character character) {
        _character = character;
        portrait.GeneratePortrait(_character);
        //UpdatePortraitControls();
        UpdateBasicInfo();
        LoadRelationships();
        LoadCharacters();
        this.gameObject.SetActive(true);
    }
    public void Close() {
        this.gameObject.SetActive(false);
    }

    #region Portrait Editor
    public void LoadTemplateChoices() {
        portraitTemplates = new Dictionary<string, PortraitSettings>();
        Directory.CreateDirectory(Utilities.portraitsSavePath);
        DirectoryInfo info = new DirectoryInfo(Utilities.portraitsSavePath);
        FileInfo[] files = info.GetFiles("*" + Utilities.portraitFileExt);
        for (int i = 0; i < files.Length; i++) {
            FileInfo currInfo = files[i];
            portraitTemplates.Add(currInfo.Name, SaveGame.Load<PortraitSettings>(currInfo.FullName));
        }
        templatesDropdown.ClearOptions();
        templatesDropdown.AddOptions(portraitTemplates.Keys.ToList());
    }
    //public void OnValueChangedPortraitTemplate(int choice) {
    //    string chosenTemplateName = templatesDropdown.options[choice].text;
    //    PortraitSettings chosenSettings = portraitTemplates[chosenTemplateName];
    //    _character.SetPortraitSettings(chosenSettings);
    //    portrait.GeneratePortrait(_character);
    //}
    public void ApplyPortraitTemplate() {
        string chosenTemplateName = templatesDropdown.options[templatesDropdown.value].text;
        PortraitSettings chosenSettings = portraitTemplates[chosenTemplateName];
        _character.SetPortraitSettings(chosenSettings);
        portrait.GeneratePortrait(_character);
    }
    #endregion

    #region Basic Info
    private void LoadDropdownOptions() {
        raceField.ClearOptions();
        genderField.ClearOptions();
        roleField.ClearOptions();
        classField.ClearOptions();

        raceField.AddOptions(Utilities.GetEnumChoices<RACE>());
        genderField.AddOptions(Utilities.GetEnumChoices<GENDER>());
        roleField.AddOptions(Utilities.GetEnumChoices<CHARACTER_ROLE>());
        classField.AddOptions(Utilities.GetFileChoices(Utilities.dataPath + "CharacterClasses/", "*.json"));
    }
    public void UpdateBasicInfo() {
        nameField.text = _character.name;
        raceField.value = Utilities.GetOptionIndex(raceField, _character.raceSetting.race.ToString());
        genderField.value = Utilities.GetOptionIndex(genderField, _character.gender.ToString());
        roleField.value = Utilities.GetOptionIndex(roleField, _character.role.roleType.ToString());
        classField.value = Utilities.GetOptionIndex(classField, _character.characterClass.className);
        otherInfoLbl.text = string.Empty;
        otherInfoLbl.text += "Home: " + _character.home.ToString();
        otherInfoLbl.text += "\nLocation: " + _character.specificLocation.ToString();
    }
    public void SetName(string newName) {
        _character.SetName(newName);
    }
    public void SetRace(int choice) {
        RACE newRace = (RACE)Enum.Parse(typeof(RACE), raceField.options[choice].text);
        _character.ChangeRace(newRace);
    }
    public void SetGender(int choice) {
        GENDER newGender = (GENDER)Enum.Parse(typeof(GENDER), genderField.options[choice].text);
        _character.ChangeGender(newGender);
    }
    public void SetRole(int choice) {
        CHARACTER_ROLE newRole = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), roleField.options[choice].text);
        _character.AssignRole(newRole);
    }
    public void SetClass(int choice) {
        string newClass = classField.options[choice].text;
        _character.ChangeClass(newClass);
    }
    #endregion

    #region Relationship Info
    private void LoadRelationships() {
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(relationshipScrollView.content.gameObject);
        for (int i = 0; i < children.Length; i++) {
            GameObject.Destroy(children[i].gameObject);
        }
        foreach (KeyValuePair<Character, Relationship> kvp in _character.relationships) {
            GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
            RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
            relItem.SetRelationship(kvp.Value);
        }
    }
    public void LoadCharacters() {
        List<string> options = new List<string>();
        charactersRelationshipDropdown.ClearOptions();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter.id != _character.id && _character.GetRelationshipWith(currCharacter) == null) {
                options.Add(currCharacter.name);
            }
        }
        charactersRelationshipDropdown.AddOptions(options);
        if (charactersRelationshipDropdown.options.Count == 0) {
            createRelationshipBtn.interactable = false;
        } else {
            createRelationshipBtn.interactable = true;
        }
    }
    public void CreateRelationship() {
        string chosenCharacterName = charactersRelationshipDropdown.options[charactersRelationshipDropdown.value].text;
        Character chosenCharacter = CharacterManager.Instance.GetCharacterByName(chosenCharacterName);
        CharacterManager.Instance.CreateNewRelationshipTowards(_character, chosenCharacter);
    }
    private void OnRelationshipCreated(Relationship newRel) {
        if (_character == null) {
            return;
        }
        GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
        RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
        relItem.SetRelationship(newRel);
        LoadCharacters();
    }
    public void OnRelationshipRemoved(Relationship removedRel) {
        if (_character == null || !this.gameObject.activeSelf) {
            return;
        }
        RelationshipEditorItem itemToRemove = GetRelationshipItem(removedRel);
        if (itemToRemove != null) {
            GameObject.Destroy(itemToRemove.gameObject);
            LoadCharacters();
        }
    }
    private RelationshipEditorItem GetRelationshipItem(Relationship rel) {
        RelationshipEditorItem[] children = Utilities.GetComponentsInDirectChildren<RelationshipEditorItem>(relationshipScrollView.content.gameObject);
        for (int i = 0; i < children.Length; i++) {
            RelationshipEditorItem currItem = children[i];
            if (currItem.relationship == rel) {
                return currItem;
            }
        }
        return null;
    }
    #endregion
}
