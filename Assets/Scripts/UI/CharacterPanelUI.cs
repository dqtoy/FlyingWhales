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
    private List<string> _skillNames;

    private int _hp;
    private int _sp;
    private float _attackPower;
    private float _speed;

    #region getters/setters
    public List<string> skillNames {
        get { return _skillNames; }
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
        _skillNames = new List<string>();
        LoadAllData();
    }

    #region Utilities
    private void LoadAllData() {
        genderOptions.ClearOptions();
        weaponOptions.ClearOptions();
        armorOptions.ClearOptions();
        accessoryOptions.ClearOptions();
        consumableOptions.ClearOptions();
        string[] genders = System.Enum.GetNames(typeof(GENDER));

        List<string> weapons = new List<string>();
        string path = Utilities.dataPath + "Items/WEAPON/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            weapons.Add(Path.GetFileNameWithoutExtension(file));
        }

        List<string> armors = new List<string>();
        armors.Add("None");
        string path2 = Utilities.dataPath + "Items/ARMOR/";
        foreach (string file in Directory.GetFiles(path2, "*.json")) {
            armors.Add(Path.GetFileNameWithoutExtension(file));
        }

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

        classOptions.value = GetClassIndex(character.className);
        genderOptions.value = GetGenderIndex(character.gender);
        weaponOptions.value = GetWeaponIndex(character.weaponName);
        armorOptions.value = GetArmorIndex(character.armorName, armorOptions);
        accessoryOptions.value = GetArmorIndex(character.accessoryName, accessoryOptions);
        consumableOptions.value = GetArmorIndex(character.consumableName, consumableOptions);

        //dHeadInput.text = character.defHead.ToString();
        //dBodyInput.text = character.defBody.ToString();
        //dLegsInput.text = character.defLegs.ToString();
        //dHandsInput.text = character.defHands.ToString();
        //dFeetInput.text = character.defFeet.ToString();

        _hp = character.maxHP;
        _sp = character.maxSP;
        _attackPower = character.attackPower;
        _speed = character.speed;

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
    private int GetArmorIndex(string armorName, Dropdown ddOptions) {
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
    private void AllocateStats(CharacterClass characterClass) {
        _attackPower = characterClass.baseAttackPower;
        _speed = characterClass.baseSP;
        _hp = characterClass.baseHP;
        _sp = characterClass.baseSP;
    }
    private void LevelUp(int level, CharacterClass characterClass) {
        float multiplier = (float)level - 1f;
        _attackPower += (multiplier * characterClass.attackPowerPerLevel);
        _speed += (multiplier * characterClass.speedPerLevel);
        _hp += ((int)multiplier * characterClass.hpPerLevel);
        _sp += ((int)multiplier * characterClass.spPerLevel);
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
        attackPowerLbl.text = _attackPower.ToString();
        speedLbl.text = _speed.ToString();
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
        AllocateStats(characterClass);
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
