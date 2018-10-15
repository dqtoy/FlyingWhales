using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using ECS;
using TMPro;


public class CharacterPanelUI : MonoBehaviour {
    public static CharacterPanelUI Instance;

    public Dropdown classOptions;
    public Dropdown raceOptions;
    public Dropdown genderOptions;
    public Dropdown weaponOptions;
    public Dropdown armorOptions;
    public Dropdown accessoryOptions;
    public Dropdown consumableOptions;

    public InputField nameInput;
    public InputField levelInput;
    //public InputField dHeadInput;
    //public InputField dBodyInput;
    //public InputField dLegsInput;
    //public InputField dHandsInput;
    //public InputField dFeetInput;

    public TextMeshProUGUI hpLbl;
    public TextMeshProUGUI attackPowerLbl;
    public TextMeshProUGUI speedLbl;
    public TextMeshProUGUI spLbl;
    public TextMeshProUGUI skillsLbl;

    private int _hp;
    private int _sp;
    private float _attackPower;
    private float _speed;
    private string _skillName;

    #region getters/setters
    public string skillName {
        get { return _skillName; }
    }
    public int hp {
        get { return _hp; }
    }
    public int sp {
        get { return _sp; }
    }
    public float speed {
        get { return _speed; }
    }
    public float attackPower {
        get { return _attackPower; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }
    void Start() {
        LoadAllData();
    }

    #region Utilities
    private void LoadAllData() {
        raceOptions.ClearOptions();
        genderOptions.ClearOptions();
        weaponOptions.ClearOptions();
        armorOptions.ClearOptions();
        accessoryOptions.ClearOptions();
        consumableOptions.ClearOptions();
        string[] genders = System.Enum.GetNames(typeof(GENDER));

        List<string> weapons = new List<string>();
        List<string> armors = new List<string>();
        string path = Utilities.dataPath + "Items/";
        string[] directories = Directory.GetDirectories(path);
        for (int i = 0; i < directories.Length; i++) {
            string folderName = new DirectoryInfo(directories[i]).Name;
            string[] files = Directory.GetFiles(directories[i], "*.json");
            for (int j = 0; j < files.Length; j++) {
                string fileName = Path.GetFileNameWithoutExtension(files[j]);
                if (folderName == "WEAPON") {
                    weapons.Add(fileName);
                } else if (folderName == "ARMOR") {
                    armors.Add(fileName);
                }
            }
        }

        List<string> races = new List<string>();
        string path3 = Utilities.dataPath + "RaceSettings/";
        foreach (string file in Directory.GetFiles(path3, "*.json")) {
            races.Add(Path.GetFileNameWithoutExtension(file));
        }

        raceOptions.AddOptions(races);
        weaponOptions.AddOptions(weapons);
        genderOptions.AddOptions(genders.ToList());
        armorOptions.AddOptions(armors);
        //accessoryOptions.AddOptions(armors);
        //consumableOptions.AddOptions(armors);
    }
    public void UpdateClassOptions() {
        classOptions.ClearOptions();
        classOptions.AddOptions(ClassPanelUI.Instance.allClasses);
    }
    public void UpdateItemOptions() {
        UpdateWeaponOptions();
        UpdateArmorOptions();
    }
    private void UpdateWeaponOptions() {
        weaponOptions.ClearOptions();
        weaponOptions.AddOptions(ItemPanelUI.Instance.allWeapons);
    }
    private void UpdateArmorOptions() {
        armorOptions.ClearOptions();
        armorOptions.AddOptions(ItemPanelUI.Instance.allArmors);
    }
    private void ClearData() {
        classOptions.value = 0;
        genderOptions.value = 0;
        weaponOptions.value = 0;
        armorOptions.value = 0;
        accessoryOptions.value = 0;
        consumableOptions.value = 0;

        nameInput.text = string.Empty;
        levelInput.text = "1";

        attackPowerLbl.text = "0";
        speedLbl.text = "0";
        hpLbl.text = "0";
        spLbl.text = "0";

        skillsLbl.text = "None";

        //dHeadInput.text = "0";
        //dBodyInput.text = "0";
        //dLegsInput.text = "0";
        //dHandsInput.text = "0";
        //dFeetInput.text = "0";
    }
    private void SaveCharacter() {
#if UNITY_EDITOR
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
#endif
    }
    private void SaveCharacterJson(string path) {
        CharacterSim newCharacter = new CharacterSim();

        newCharacter.SetDataFromCharacterPanelUI();

        string jsonString = JsonUtility.ToJson(newCharacter);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved character at " + path);

        CombatSimManager.Instance.UpdateAllCharacters();
    }
    private void LoadCharacter() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Character", Utilities.dataPath + "CharacterSims/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            CharacterSim character = JsonUtility.FromJson<CharacterSim>(dataAsJson);
            ClearData();
            LoadCharacterDataToUI(character);
        }
#endif
    }

    private void LoadCharacterDataToUI(CharacterSim character) {
        nameInput.text = character.name;
        levelInput.text = character.level.ToString();

        classOptions.value = GetDropdownIndex(character.className, classOptions);
        genderOptions.value = GetDropdownIndex(character.gender.ToString(), genderOptions);
        weaponOptions.value = GetDropdownIndex(character.weaponName, weaponOptions);
        armorOptions.value = GetDropdownIndex(character.armorName, armorOptions);
        accessoryOptions.value = GetDropdownIndex(character.accessoryName, accessoryOptions);
        consumableOptions.value = GetDropdownIndex(character.consumableName, consumableOptions);

        //dHeadInput.text = character.defHead.ToString();
        //dBodyInput.text = character.defBody.ToString();
        //dLegsInput.text = character.defLegs.ToString();
        //dHandsInput.text = character.defHands.ToString();
        //dFeetInput.text = character.defFeet.ToString();

        _hp = character.maxHP;
        _sp = character.maxSP;
        _attackPower = character.attackPower;
        _speed = character.speed;

        //_skillNames.Clear();
        //for (int i = 0; i < character.skillNames.Count; i++) {
        //    string skillName = character.skillNames[i];
        //    _skillNames.Add(skillName);
        //}

        UpdateUI();
    }
    private int GetDropdownIndex(string armorName, Dropdown ddOptions) {
        for (int i = 0; i < ddOptions.options.Count; i++) {
            if (ddOptions.options[i].text == armorName) {
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
    private RaceSetting GetRace(string raceName) {
        string path = Utilities.dataPath + "RaceSettings/" + raceName + ".json";
        RaceSetting currentRace = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(path));
        return currentRace;
    }
    private void AllocateStats(CharacterClass characterClass) {
        _attackPower = characterClass.baseAttackPower;
        _speed = characterClass.baseSpeed;
        _hp = characterClass.baseHP;
        _sp = characterClass.baseSP;
    }
    private void LevelUp(int level, CharacterClass characterClass, RaceSetting raceSetting) {
        float multiplier = (float)level - 1f;
        if(multiplier < 0f) {
            multiplier = 0f;
        }
        _attackPower += (multiplier * characterClass.attackPowerPerLevel);
        _speed += (multiplier * characterClass.speedPerLevel);
        _hp += ((int)multiplier * characterClass.hpPerLevel);
        _sp += ((int)multiplier * characterClass.spPerLevel);
        //Add stats per level from race
        if(level > 0) {
            int hpIndex = level % raceSetting.hpPerLevel.Length;
            hpIndex = hpIndex == 0 ? raceSetting.hpPerLevel.Length : hpIndex;
            int attackIndex = level % raceSetting.attackPerLevel.Length;
            attackIndex = attackIndex == 0 ? raceSetting.attackPerLevel.Length : attackIndex;

            _hp += raceSetting.hpPerLevel[hpIndex - 1];
            _attackPower += raceSetting.attackPerLevel[attackIndex - 1];
        }
    }
    private void UpdateSkills(CharacterClass characterClass) {
        _skillName = characterClass.skillName;
    }
    private void UpdateUI() {
        attackPowerLbl.text = _attackPower.ToString();
        speedLbl.text = _speed.ToString();
        hpLbl.text = _hp.ToString();
        spLbl.text = _sp.ToString();

        if(_skillName != string.Empty) {
            skillsLbl.text = _skillName;
        } else {
            skillsLbl.text = "None";
        }
    }
    private void ClassChange(int index) {
        CharacterClass characterClass = GetClass(classOptions.options[index].text);
        RaceSetting race = GetRace(raceOptions.options[raceOptions.value].text);
        int level = int.Parse(levelInput.text);
        if (level < 1) {
            level = 1;
            levelInput.text = level.ToString();
        } else if (level > 100) {
            level = 100;
            levelInput.text = level.ToString();
        }
        AllocateStats(characterClass);
        LevelUp(level, characterClass, race);
        UpdateSkills(characterClass);
        UpdateUI();
    }
    private void RaceChange(int index) {
        CharacterClass characterClass = GetClass(classOptions.options[classOptions.value].text);
        RaceSetting race = GetRace(raceOptions.options[index].text);
        int level = int.Parse(levelInput.text);
        if (level < 1) {
            level = 1;
            levelInput.text = level.ToString();
        } else if (level > 100) {
            level = 100;
            levelInput.text = level.ToString();
        }
        AllocateStats(characterClass);
        LevelUp(level, characterClass, race);
        UpdateSkills(characterClass);
        UpdateUI();
    }
    #endregion

    #region OnValueChanged
    public void OnClassChange(int index) {
        ClassChange(index);
    }
    public void OnRaceChange(int index) {
        RaceChange(index);
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
