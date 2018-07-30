#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using ECS;
using TMPro;


public class CharacterPanelUI : MonoBehaviour {
    public static CharacterPanelUI Instance;

    public Dropdown classOptions;
    public Dropdown genderOptions;
    public Dropdown weaponOptions;

    public InputField nameInput;
    public InputField levelInput;
    public InputField dHeadInput;
    public InputField dBodyInput;
    public InputField dLegsInput;
    public InputField dHandsInput;
    public InputField dFeetInput;


    public TextMeshProUGUI strBuildLbl;
    public TextMeshProUGUI intBuildLbl;
    public TextMeshProUGUI agiBuildLbl;
    public TextMeshProUGUI vitBuildLbl;
    public TextMeshProUGUI strAllocLbl;
    public TextMeshProUGUI intAllocLbl;
    public TextMeshProUGUI agiAllocLbl;
    public TextMeshProUGUI vitAllocLbl;
    public TextMeshProUGUI hpLbl;
    public TextMeshProUGUI spLbl;
    public TextMeshProUGUI skillsLbl;

    private int _str;
    private int _int;
    private int _agi;
    private int _vit;
    private int _hp;
    private int _sp;

    private int _strBuild;
    private int _intBuild;
    private int _agiBuild;
    private int _vitBuild;

    private List<string> _skillNames;

    #region getters/setters
    public int str {
        get { return _str; }
    }
    public int intl {
        get { return _int; }
    }
    public int agi {
        get { return _agi; }
    }
    public int vit {
        get { return _vit; }
    }
    public int hp {
        get { return _hp; }
    }
    public int sp {
        get { return _sp; }
    }
    public int strBuild {
        get { return _strBuild; }
    }
    public int intBuild {
        get { return _intBuild; }
    }
    public int agiBuild {
        get { return _agiBuild; }
    }
    public int vitBuild {
        get { return _vitBuild; }
    }
    public List<string> skillNames {
        get { return _skillNames; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }
    void Start() {
        _skillNames = new List<string>();
        LoadAllData();
    }

    #region Utilities
    private void LoadAllData() {
        genderOptions.ClearOptions();
        weaponOptions.ClearOptions();

        string[] genders = System.Enum.GetNames(typeof(GENDER));

        List<string> weapons = new List<string>();
        string path = Utilities.dataPath + "Items/WEAPON/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            weapons.Add(Path.GetFileNameWithoutExtension(file));
        }

        weaponOptions.AddOptions(weapons);
        genderOptions.AddOptions(genders.ToList());
    }
    public void UpdateClassOptions() {
        classOptions.ClearOptions();
        classOptions.AddOptions(ClassPanelUI.Instance.allClasses);
    }
    private void ClearData() {
        classOptions.value = 0;
        genderOptions.value = 0;
        weaponOptions.value = 0;

        nameInput.text = string.Empty;
        levelInput.text = "1";

        strBuildLbl.text = "0";
        intBuildLbl.text = "0";
        agiBuildLbl.text = "0";
        vitBuildLbl.text = "0";
        strAllocLbl.text = "0";
        intAllocLbl.text = "0";
        agiAllocLbl.text = "0";
        vitAllocLbl.text = "0";
        hpLbl.text = "0";
        spLbl.text = "0";

        skillsLbl.text = "None";

        dHeadInput.text = "0";
        dBodyInput.text = "0";
        dLegsInput.text = "0";
        dHandsInput.text = "0";
        dFeetInput.text = "0";
    }
    private void SaveCharacter() {
        if (nameInput.text == string.Empty) {
            EditorUtility.DisplayDialog("Error", "Please specify a Character Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "CharacterSims/" + nameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Character", "A character with name " + nameInput.text + " already exists. Replace with this character?", "Yes", "No")) {
                File.Delete(path);
                SaveCharacterJson(path);
            }
        } else {
            SaveCharacterJson(path);
        }
    }
    private void SaveCharacterJson(string path) {
        CharacterSim newCharacter = new CharacterSim();

        newCharacter.SetDataFromCharacterPanelUI();

        string jsonString = JsonUtility.ToJson(newCharacter);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
        Debug.Log("Successfully saved character at " + path);

        CombatSimManager.Instance.UpdateAllCharacters();
    }
    private void LoadCharacter() {
        string filePath = EditorUtility.OpenFilePanel("Select Character", Utilities.dataPath + "CharacterSims/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            CharacterSim character = JsonUtility.FromJson<CharacterSim>(dataAsJson);
            ClearData();
            LoadCharacterDataToUI(character);
        }
    }

    private void LoadCharacterDataToUI(CharacterSim character) {
        nameInput.text = character.name;
        classOptions.value = GetClassIndex(character.className);
        genderOptions.value = GetGenderIndex(character.gender);
        weaponOptions.value = GetWeaponIndex(character.weaponName);
        levelInput.text = character.level.ToString();

        dHeadInput.text = character.defHead.ToString();
        dBodyInput.text = character.defBody.ToString();
        dLegsInput.text = character.defLegs.ToString();
        dHandsInput.text = character.defHands.ToString();
        dFeetInput.text = character.defFeet.ToString();

        _strBuild = character.strBuild;
        _intBuild = character.intBuild;
        _agiBuild = character.agiBuild;
        _vitBuild = character.vitBuild;
        _str = character.strength;
        _int = character.intelligence;
        _agi = character.agility;
        _vit = character.vitality;
        _hp = character.maxHP;
        _sp = character.maxSP;

        _skillNames.Clear();
        for (int i = 0; i < character.skillNames.Count; i++) {
            string skillName = character.skillNames[i];
            _skillNames.Add(skillName);
        }

        UpdateUI();
    }
    private int GetClassIndex(string className) {
        for (int i = 0; i < classOptions.options.Count; i++) {
            if (classOptions.options[i].text == className) {
                return i;
            }
        }
        return 0;
    }
    private int GetGenderIndex(GENDER gender) {
        for (int i = 0; i < genderOptions.options.Count; i++) {
            if (genderOptions.options[i].text == gender.ToString()) {
                return i;
            }
        }
        return 0;
    }
    private int GetWeaponIndex(string weaponName) {
        for (int i = 0; i < weaponOptions.options.Count; i++) {
            if (weaponOptions.options[i].text == weaponName) {
                return i;
            }
        }
        return 0;
    }
    private CharacterClass GetClass(string className) {
        string path = Utilities.dataPath + "CharacterClasses/" + className + ".json";
        CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(path));
        return currentClass;
    }
    private void AllocateStatPoints(int statAllocation, CharacterClass characterClass) {
        _strBuild = 0;
        _intBuild = 0;
        _agiBuild = 0;
        _vitBuild = 0;

        WeightedDictionary<string> statWeights = new WeightedDictionary<string>();
        statWeights.AddElement("strength", (int) characterClass.strWeightAllocation);
        statWeights.AddElement("intelligence", (int) characterClass.intWeightAllocation);
        statWeights.AddElement("agility", (int) characterClass.agiWeightAllocation);
        statWeights.AddElement("vitality", (int) characterClass.vitWeightAllocation);

        if (statWeights.GetTotalOfWeights() > 0) {
            string chosenStat = string.Empty;
            for (int i = 0; i < statAllocation; i++) {
                chosenStat = statWeights.PickRandomElementGivenWeights();
                if (chosenStat == "strength") {
                    _strBuild += 1;
                } else if (chosenStat == "intelligence") {
                    _intBuild += 1;
                } else if (chosenStat == "agility") {
                    _agiBuild += 1;
                } else if (chosenStat == "vitality") {
                    _vitBuild += 1;
                }
            }
        }
    }
    private void LevelUp(int level, CharacterClass characterClass) {
        _str = (level * _strBuild) + 1;
        _int = (level * _intBuild) + 1;
        _agi = (level * _agiBuild) + 1;
        _vit = (level * _vitBuild) + 1;

        RecomputeMaxHP(level, characterClass);
        RecomputeMaxSP(level, characterClass);
    }
    private void RecomputeMaxHP(int level, CharacterClass characterClass) {
        float vitality = (float) vit;
        _hp = (int) ((characterClass.baseHP + (characterClass.hpPerLevel * (float) level)) * (1f + ((vitality / 5f) / 100f)) + (vitality * 2f)); //TODO: + passive hp bonus
    }
    private void RecomputeMaxSP(int level, CharacterClass characterClass) {
        float intelligence = (float) intl;
        _sp = (int) ((characterClass.baseSP + (characterClass.spPerLevel * (float) level)) * (1f + ((intelligence / 5f) / 100f)) + (intelligence * 2f)); //TODO: + passive sp bonus
    }
    private void UpdateSkills(int level, CharacterClass characterClass) {
        _skillNames.Clear();
        for (int i = 0; i < level; i++) {
            if (i < characterClass.skillsPerLevelNames.Count) {
                StringListWrapper listWrapper = characterClass.skillsPerLevelNames[i];
                if (listWrapper.list != null) {
                    for (int j = 0; j < listWrapper.list.Count; j++) {
                        string skillName = listWrapper.list[j];
                        if (!_skillNames.Contains(skillName)) {
                            _skillNames.Add(skillName);
                        }
                    }
                }
            } else {
                break;
            }
        }
    }
    private void UpdateUI() {
        strBuildLbl.text = _strBuild.ToString();
        intBuildLbl.text = _intBuild.ToString();
        agiBuildLbl.text = _agiBuild.ToString();
        vitBuildLbl.text = _vitBuild.ToString();

        strAllocLbl.text = _str.ToString();
        intAllocLbl.text = _int.ToString();
        agiAllocLbl.text = _agi.ToString();
        vitAllocLbl.text = _vit.ToString();

        hpLbl.text = _hp.ToString();
        spLbl.text = _sp.ToString();

        skillsLbl.text = "None";
        if (_skillNames.Count > 0) {
            skillsLbl.text = _skillNames[0];
            for (int i = 1; i < _skillNames.Count; i++) {
                skillsLbl.text += ", " + _skillNames[i];
            }
        }
    }
    private void ClassChange(int index) {
        CharacterClass characterClass = GetClass(classOptions.options[index].text);
        int level = int.Parse(levelInput.text);
        if (level < 1) {
            level = 1;
        } else if (level > 100) {
            level = 100;
        }
        AllocateStatPoints(10, characterClass);
        LevelUp(level, characterClass);
        UpdateSkills(level, characterClass);
        UpdateUI();
    }
    #endregion

    #region OnValueChanged
    public void OnClassChange(int index) {
        ClassChange(index);
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewCharacter() {
        ClearData();
    }
    public void OnClickEditCharacter() {
        LoadCharacter();
    }
    public void OnClickSaveCharacter() {
        SaveCharacter();
    }
    public void OnClickApply() {
        ClassChange(classOptions.value);
    }
    #endregion
}
#endif
