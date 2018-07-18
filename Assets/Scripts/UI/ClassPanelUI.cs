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

public class ClassPanelUI : MonoBehaviour {
    public static ClassPanelUI Instance;

    public InputField classNameInput;
    public InputField strWeightAllocInput;
    public InputField intWeightAllocInput;
    public InputField agiWeightAllocInput;
    public InputField vitWeightAllocInput;
    public InputField hpMultiplierInput;
    public InputField spMultiplierInput;

    public Dropdown allowedWeaponsOptions;

    public GameObject allowedWeaponsGO;
    public GameObject skillsGO;
    public GameObject weaponTypeBtnGO;
    public GameObject skillsPerLevelGO;

    public Transform allowedWeaponsContentTransform;
    public Transform skillsContentTransform;

    public ScrollRect skillsScrollView;

    [NonSerialized] public WeaponTypeButton currentSelectedWeaponTypeButton;
    [NonSerialized] public int latestLevel;
    [NonSerialized] public List<string> allClasses;

    private List<string> _allowedWeaponTypes;

    #region getters/setters
    public List<string> allowedWeaponTypes {
        get { return _allowedWeaponTypes; }
    }

    #endregion

    void Awake() {
        Instance = this;
    }
    void Start() {
        allClasses = new List<string>();
        _allowedWeaponTypes = new List<string>();
        LoadAllData();
    }

    #region Utilities
    private void UpdateClassList() {
        allClasses.Clear();
        string path = Utilities.dataPath + "CharacterClasses/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allClasses.Add(Path.GetFileNameWithoutExtension(file));
        }
        CharacterPanelUI.Instance.UpdateClassOptions();
    }
    private void LoadAllData() {
        allowedWeaponsOptions.ClearOptions();

        string[] weaponTypes = System.Enum.GetNames(typeof(WEAPON_TYPE));

        allowedWeaponsOptions.AddOptions(weaponTypes.ToList());
        UpdateClassList();
    }
    private void ClearData() {
        latestLevel = 0;
        currentSelectedWeaponTypeButton = null;
        classNameInput.text = string.Empty;

        strWeightAllocInput.text = "0";
        intWeightAllocInput.text = "0";
        agiWeightAllocInput.text = "0";
        vitWeightAllocInput.text = "0";
        hpMultiplierInput.text = "0";
        spMultiplierInput.text = "0";

        allowedWeaponsOptions.value = 0;

        _allowedWeaponTypes.Clear();
        foreach (Transform child in allowedWeaponsContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in skillsContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
    }
    private void SaveClass() {
        if (classNameInput.text == string.Empty) {
            EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "CharacterClasses/" + classNameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Class", "A class with name " + classNameInput.text + " already exists. Replace with this class?", "Yes", "No")) {
                File.Delete(path);
                SaveClassJson(path);
            }
        } else {
            SaveClassJson(path);
        }
    }
    private void SaveClassJson(string path) {
        CharacterClass newClass = new CharacterClass();

        newClass.SetDataFromClassPanelUI();

        string jsonString = JsonUtility.ToJson(newClass);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
        Debug.Log("Successfully saved class at " + path);

        UpdateClassList();
    }

    private void LoadClass() {
        string filePath = EditorUtility.OpenFilePanel("Select Class", Utilities.dataPath + "CharacterClasses/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            CharacterClass characterClass = JsonUtility.FromJson<CharacterClass>(dataAsJson);
            ClearData();
            LoadClassDataToUI(characterClass);
        }
    }

    private void LoadClassDataToUI(CharacterClass characterClass) {
        classNameInput.text = characterClass.className;
        strWeightAllocInput.text = characterClass.strWeightAllocation.ToString();
        intWeightAllocInput.text = characterClass.intWeightAllocation.ToString();
        agiWeightAllocInput.text = characterClass.agiWeightAllocation.ToString();
        vitWeightAllocInput.text = characterClass.vitWeightAllocation.ToString();
        hpMultiplierInput.text = characterClass.hpModifier.ToString();
        spMultiplierInput.text = characterClass.spModifier.ToString();

        for (int i = 0; i < characterClass.allowedWeaponTypes.Count; i++) {
            string weaponType = characterClass.allowedWeaponTypes[i].ToString();
            _allowedWeaponTypes.Add(weaponType);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, allowedWeaponsContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = weaponType;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
        }

        for (int i = 0; i < characterClass.skillsPerLevelNames.Count; i++) {
            StringListWrapper listWrapper = characterClass.skillsPerLevelNames[i];
            GameObject go = AddLevel();
            go.GetComponent<LevelCollapseUI>().SetSkills(listWrapper.list);
        }
    }

    private GameObject AddLevel() {
        if (latestLevel < 100) {
            latestLevel++;
            GameObject go = GameObject.Instantiate(skillsPerLevelGO, skillsContentTransform);
            go.GetComponent<LevelCollapseUI>().lvlText.text = latestLevel.ToString();
            return go;
        }
        return null;
    }
    #endregion

    #region Button Clicks
    public void OnAddNewClass() {
        ClearData();
    }
    public void OnAddWeaponType() {
        string weaponTypeToAdd = allowedWeaponsOptions.options[allowedWeaponsOptions.value].text;
        if (weaponTypeToAdd != "NONE" && !_allowedWeaponTypes.Contains(weaponTypeToAdd)) {
            _allowedWeaponTypes.Add(weaponTypeToAdd);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, allowedWeaponsContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = weaponTypeToAdd;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
        }
    }
    public void OnRemoveWeaponType() {
        if (currentSelectedWeaponTypeButton != null) {
            string weaponTypeToRemove = currentSelectedWeaponTypeButton.buttonText.text;
            if (_allowedWeaponTypes.Remove(weaponTypeToRemove)) {
                GameObject.Destroy(currentSelectedWeaponTypeButton.gameObject);
                currentSelectedWeaponTypeButton = null;
            }
        }
    }
    public void OnSaveClass() {
        SaveClass();
    }
    public void OnEditClass() {
        LoadClass();
    }
    public void OnClickAllowedWeaponsTab() {
        allowedWeaponsGO.SetActive(true);
        skillsGO.SetActive(false);
    }
    public void OnClickSkillsTab() {
        allowedWeaponsGO.SetActive(false);
        skillsGO.SetActive(true);
    }
    public void OnClickAddLevel() {
        AddLevel();
    }
    public void OnClickRemoveLevel() {
        if(latestLevel > 0) {
            latestLevel--;
            GameObject.Destroy(skillsContentTransform.GetChild(latestLevel).gameObject);
        }
    }
    #endregion
}
#endif
