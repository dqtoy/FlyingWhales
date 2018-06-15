using ECS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EditCharactersMenu : MonoBehaviour {

    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private Dropdown raceDropdown;
    [SerializeField] private Dropdown genderDropdown;
    [SerializeField] private Dropdown roleDropdown;
    [SerializeField] private Dropdown classDropdown;
    [SerializeField] private ScrollRect charactersScrollView;

    [SerializeField] private CharacterInfoEditor characterInfoEditor;

    #region Monobehaviours
    private void Awake() {
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_CREATED, OnCreateNewCharacter);
        PopulateClassDropdown();
    }
    #endregion

    #region Character Creation
    public void CreateNewCharacter() {
        RACE race = (RACE)Enum.Parse(typeof(RACE), raceDropdown.options[raceDropdown.value].text);
        GENDER gender = (GENDER)Enum.Parse(typeof(GENDER), genderDropdown.options[genderDropdown.value].text);
        CHARACTER_ROLE role = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), roleDropdown.options[roleDropdown.value].text);
        string className = classDropdown.options[classDropdown.value].text;

        ECS.CharacterSetup setup = ECS.CombatManager.Instance.GetBaseCharacterSetup(className, race);
        ECS.Character newCharacter = CharacterManager.Instance.CreateNewCharacter(role, gender, setup);
        //Debug.Log("Created new character " + newCharacter.name + "")
    }
    private void OnCreateNewCharacter(Character newCharacter) {
        GameObject characterItemGO = GameObject.Instantiate(characterItemPrefab, charactersScrollView.content.transform);
        CharacterEditorItem characterItem = characterItemGO.GetComponent<CharacterEditorItem>();
        characterItem.SetCharacter(newCharacter);
        characterItem.SetEditAction(() => ShowCharacterInfoEditor(newCharacter));
    }
    #endregion

    #region Dropdown Data
    private void PopulateClassDropdown() {
        classDropdown.ClearOptions();
        List<string> choices = new List<string>();
        string path = Utilities.dataPath + "CharacterClasses/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            ECS.CharacterClass currentClass = JsonUtility.FromJson<ECS.CharacterClass>(System.IO.File.ReadAllText(classes[i]));
            choices.Add(currentClass.className);
        }
        classDropdown.AddOptions(choices);

        
    }
    #endregion

    #region Character Info Editor
    private void ShowCharacterInfoEditor(Character character) {
        characterInfoEditor.ShowCharacterInfo(character);
    }
    #endregion
}
