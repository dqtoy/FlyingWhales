using ECS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class CharacterInfoEditor : MonoBehaviour {

    private Character _character;

    [SerializeField] private CharacterPortrait portrait;

    [Header("Portrait Editor")]
    [SerializeField] private Stepper headStepper;
    [SerializeField] private Stepper hairStepper;
    [SerializeField] private Stepper eyesStepper;
    [SerializeField] private Stepper noseStepper;
    [SerializeField] private Stepper mouthStepper;
    [SerializeField] private Stepper eyebrowsStepper;
    [SerializeField] private ColorPickerControl hairColorPicker;
    [SerializeField] private Image hairColorImage;

    [Header("Basic Info")]
    [SerializeField] private InputField nameField;
    [SerializeField] private Dropdown raceField;
    [SerializeField] private Dropdown genderField;
    [SerializeField] private Dropdown roleField;
    [SerializeField] private Dropdown classField;

    [Header("Relationship Info")]
    [SerializeField] private GameObject relationshipItemPrefab;
    [SerializeField] private ScrollRect relationshipScrollView;
    [SerializeField] private Dropdown charactersRelationshipDropdown;
    [SerializeField] private Button createRelationshipBtn;

    #region Monobehaviours
    private void Awake() {
        Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_CREATED, OnRelationshipCreated);
        Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_REMOVED, OnRelationshipRemoved);

        SetStepperValues();
        LoadDropdownOptions();
    }
    #endregion

    public void ShowCharacterInfo(Character character) {
        _character = character;
        portrait.GeneratePortrait(character);
        this.gameObject.SetActive(true);

        UpdatePortraitControls();
        UpdateBasicInfo();
        LoadRelationships();
        LoadCharacters();
    }
    public void Close() {
        this.gameObject.SetActive(false);
    }

    #region Portrait Editor
    private void SetStepperValues() {
        headStepper.maximum = CharacterManager.Instance.headSprites.Count - 1;
        hairStepper.maximum = CharacterManager.Instance.hairSettings.Count - 1;
        eyesStepper.maximum = CharacterManager.Instance.eyeSprites.Count - 1;
        noseStepper.maximum = CharacterManager.Instance.noseSprites.Count - 1;
        mouthStepper.maximum = CharacterManager.Instance.mouthSprites.Count - 1;
        eyebrowsStepper.maximum = CharacterManager.Instance.eyeBrowSprites.Count - 1;
    }
    private void UpdatePortraitControls() {
        headStepper.SetStepperValue(_character.portraitSettings.headIndex);
        hairStepper.SetStepperValue(_character.portraitSettings.hairIndex);
        eyesStepper.SetStepperValue(_character.portraitSettings.eyesIndex);
        noseStepper.SetStepperValue(_character.portraitSettings.noseIndex);
        mouthStepper.SetStepperValue(_character.portraitSettings.mouthIndex);
        eyebrowsStepper.SetStepperValue(_character.portraitSettings.eyeBrowIndex);
        hairColorImage.color = _character.portraitSettings.hairColor;
        hairColorPicker.CurrentColor = _character.portraitSettings.hairColor;
    }
    public void ShowHairColorPicker() {
        hairColorPicker.ShowMenu();
    }
    public void UpdateHair(int index) {
        portrait.SetHair(index);
        _character.portraitSettings.hairIndex = index;
    }
    public void UpdateHead(int index) {
        portrait.SetHead(index);
        _character.portraitSettings.headIndex = index;
    }
    public void UpdateEyes(int index) {
        portrait.SetEyes(index);
        _character.portraitSettings.eyesIndex = index;
    }
    public void UpdateNose(int index) {
        portrait.SetNose(index);
        _character.portraitSettings.noseIndex = index;
    }
    public void UpdateMouth(int index) {
        portrait.SetMouth(index);
        _character.portraitSettings.mouthIndex = index;
    }
    public void UpdateEyebrows(int index) {
        portrait.SetEyebrows(index);
        _character.portraitSettings.eyeBrowIndex = index;
    }
    public void UpdateHairColor(Color color) {
        portrait.SetHairColor(color);
        _character.portraitSettings.hairColor = color;
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
    private void UpdateBasicInfo() {
        nameField.text = _character.name;
        raceField.value = Utilities.GetOptionIndex(raceField, _character.raceSetting.race.ToString());
        genderField.value = Utilities.GetOptionIndex(genderField, _character.gender.ToString());
        roleField.value = Utilities.GetOptionIndex(roleField, _character.role.roleType.ToString());
        classField.value = Utilities.GetOptionIndex(classField, _character.characterClass.className);
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
        if (_character == null) {
            return;
        }
        RelationshipEditorItem itemToRemove = GetRelationshipItem(removedRel);
        GameObject.Destroy(itemToRemove.gameObject);
        LoadCharacters();
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
