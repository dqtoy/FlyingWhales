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


public class ClassPanelUI : MonoBehaviour {
    public static ClassPanelUI Instance;

    public InputField classNameInput;
    //public InputField baseAttackPowerInput;
    public InputField attackPowerPerLevelInput;
    //public InputField baseSpeedInput;
    public InputField speedPerLevelInput;
    //public InputField baseHPInput;
    public InputField hpPerLevelInput;
    public InputField baseSPInput;
    public InputField spPerLevelInput;
    public InputField armyCountInput;
    public InputField recruitmentCostInput;

    public Dropdown weaponsOptions;
    public Dropdown armorsOptions;
    public Dropdown accessoriesOptions;
    public Dropdown traitOptions;

    public Dropdown workActionOptions;
    public Dropdown roleOptions;
    public Dropdown skillOptions;
    public Dropdown jobTypeOptions;
    public Dropdown recruitmentCostOptions;

    public GameObject weaponsGO;
    public GameObject armorsGO;
    public GameObject accessoriesGO;
    public GameObject traitsGO;

    public GameObject weaponTypeBtnGO;
    public GameObject traitBtnGO;

    public Transform weaponsContentTransform;
    public Transform armorsContentTransform;
    public Transform accessoriesContentTransform;
    public ScrollRect traitsScrollRect;

    [NonSerialized] public WeaponTypeButton currentSelectedWeaponButton;
    [NonSerialized] public WeaponTypeButton currentSelectedArmorButton;
    [NonSerialized] public WeaponTypeButton currentSelectedAccessoryButton;
    [NonSerialized] public ClassTraitButton currentSelectedClassTraitButton;
    [NonSerialized] public int latestLevel;
    [NonSerialized] public List<string> allClasses;

    private List<string> _weaponTiers;
    private List<string> _armorTiers;
    private List<string> _accessoryTiers;
    private List<string> _traitNames;

    #region getters/setters
    public List<string> weaponTiers {
        get { return _weaponTiers; }
    }
    public List<string> armorTiers {
        get { return _armorTiers; }
    }
    public List<string> accessoryTiers {
        get { return _accessoryTiers; }
    }
    public List<string> traitNames {
        get { return _traitNames; }
    }
    #endregion

    void Awake() {
        Instance = this;
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
    public void UpdateSkillOptions() {
        skillOptions.ClearOptions();
        skillOptions.AddOptions(SkillPanelUI.Instance.allSkills);
    }
    public void UpdateItemOptions() {
        weaponsOptions.ClearOptions();
        armorsOptions.ClearOptions();
        accessoriesOptions.ClearOptions();

        weaponsOptions.AddOptions(ItemPanelUI.Instance.allWeapons);
        armorsOptions.AddOptions(ItemPanelUI.Instance.allArmors);
        accessoriesOptions.AddOptions(ItemPanelUI.Instance.allItems);
    }
    public void UpdateTraitOptions() {
        traitOptions.ClearOptions();
        traitOptions.AddOptions(CombatAttributePanelUI.Instance.allCombatAttributes);
    }
    public void LoadAllData() {
        allClasses = new List<string>();
        _weaponTiers = new List<string>();
        _armorTiers = new List<string>();
        _accessoryTiers = new List<string>();
        _traitNames = new List<string>();

        armyCountInput.text = "1";
        recruitmentCostInput.text = "0";
        workActionOptions.ClearOptions();
        roleOptions.ClearOptions();
        jobTypeOptions.ClearOptions();
        recruitmentCostOptions.ClearOptions();

        string[] workActions = System.Enum.GetNames(typeof(ACTION_TYPE));
        string[] roles = System.Enum.GetNames(typeof(CHARACTER_ROLE));
        string[] jobs = System.Enum.GetNames(typeof(JOB));
        string[] cost = System.Enum.GetNames(typeof(CURRENCY));

        workActionOptions.AddOptions(workActions.ToList());
        roleOptions.AddOptions(roles.ToList());
        jobTypeOptions.AddOptions(jobs.ToList());
        recruitmentCostOptions.AddOptions(cost.ToList());
        UpdateClassList();
    }
    private void ClearData() {
        latestLevel = 0;
        currentSelectedWeaponButton = null;
        currentSelectedArmorButton = null;
        currentSelectedAccessoryButton = null;
        currentSelectedClassTraitButton = null;

        classNameInput.text = string.Empty;

        //baseAttackPowerInput.text = "0";
        attackPowerPerLevelInput.text = "0";
        //baseSpeedInput.text = "0";
        speedPerLevelInput.text = "0";
        //baseHPInput.text = "0";
        hpPerLevelInput.text = "0";
        baseSPInput.text = "0";
        spPerLevelInput.text = "0";
        armyCountInput.text = "1";
        recruitmentCostInput.text = "0";

        weaponsOptions.value = 0;
        armorsOptions.value = 0;
        accessoriesOptions.value = 0;
        traitOptions.value = 0;
        workActionOptions.value = 0;
        skillOptions.value = 0;
        roleOptions.value = 0;
        jobTypeOptions.value = 0;
        recruitmentCostOptions.value = 0;

        _weaponTiers.Clear();
        _armorTiers.Clear();
        _accessoryTiers.Clear();
        _traitNames.Clear();
        weaponsContentTransform.DestroyChildren();
        armorsContentTransform.DestroyChildren();
        accessoriesContentTransform.DestroyChildren();
        traitsScrollRect.content.DestroyChildren();
    }
    private void SaveClass() {
        if (string.IsNullOrEmpty(classNameInput.text)) {
#if UNTIY_EDITOR
            EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
            return;
#endif
        }
        string path = Utilities.dataPath + "CharacterClasses/" + classNameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Overwrite Class", "A class with name " + classNameInput.text + " already exists. Replace with this class?", "Yes", "No")) {
                File.Delete(path);
                SaveClassJson(path);
            }
#endif
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

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved class at " + path);

        UpdateClassList();
    }

    private void LoadClass() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Class", Utilities.dataPath + "CharacterClasses/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            CharacterClass characterClass = JsonUtility.FromJson<CharacterClass>(dataAsJson);
            ClearData();
            LoadClassDataToUI(characterClass);
        }
#endif
    }

    private void LoadClassDataToUI(CharacterClass characterClass) {
        classNameInput.text = characterClass.className;
        //baseAttackPowerInput.text = characterClass.baseAttackPower.ToString();
        attackPowerPerLevelInput.text = characterClass.attackPowerPerLevel.ToString();
        //baseSpeedInput.text = characterClass.baseSpeed.ToString();
        speedPerLevelInput.text = characterClass.speedPerLevel.ToString();
        armyCountInput.text = characterClass.armyCount.ToString();
        //baseHPInput.text = characterClass.baseHP.ToString();
        hpPerLevelInput.text = characterClass.hpPerLevel.ToString();
        baseSPInput.text = characterClass.baseSP.ToString();
        spPerLevelInput.text = characterClass.spPerLevel.ToString();
        recruitmentCostInput.text = characterClass.recruitmentCost.amount.ToString();

        workActionOptions.value = GetDropdownIndex(workActionOptions, characterClass.workActionType.ToString());
        roleOptions.value = GetDropdownIndex(roleOptions, characterClass.roleType.ToString());
        skillOptions.value = GetDropdownIndex(skillOptions, characterClass.skillName.ToString());
        jobTypeOptions.value = GetDropdownIndex(jobTypeOptions, characterClass.jobType.ToString());
        recruitmentCostOptions.value = GetDropdownIndex(recruitmentCostOptions, characterClass.recruitmentCost.currency.ToString());

        for (int i = 0; i < characterClass.weaponTierNames.Count; i++) {
            string weaponName = characterClass.weaponTierNames[i];
            _weaponTiers.Add(weaponName);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, weaponsContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = weaponName;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
            go.GetComponent<WeaponTypeButton>().categoryName = "weapon";
        }
        for (int i = 0; i < characterClass.armorTierNames.Count; i++) {
            string armorName = characterClass.armorTierNames[i];
            _armorTiers.Add(armorName);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, armorsContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = armorName;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
            go.GetComponent<WeaponTypeButton>().categoryName = "armor";
        }
        for (int i = 0; i < characterClass.accessoryTierNames.Count; i++) {
            string accessoryName = characterClass.accessoryTierNames[i];
            _accessoryTiers.Add(accessoryName);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, accessoriesContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = accessoryName;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
            go.GetComponent<WeaponTypeButton>().categoryName = "accessory";
        }
        for (int i = 0; i < characterClass.traitNames.Length; i++) {
            string traitName = characterClass.traitNames[i];
            _traitNames.Add(traitName);
            GameObject go = GameObject.Instantiate(traitBtnGO, traitsScrollRect.content);
            go.GetComponent<ClassTraitButton>().SetTraitName(traitName);
        }
    }
    private int GetDropdownIndex(Dropdown options, string name) {
        for (int i = 0; i < options.options.Count; i++) {
            if (options.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    #endregion

    #region Button Clicks
    public void OnAddNewClass() {
        ClearData();
    }
    public void OnSaveClass() {
        SaveClass();
    }
    public void OnEditClass() {
        LoadClass();
    }
    public void OnAddWeapon() {
        string weaponTypeToAdd = weaponsOptions.options[weaponsOptions.value].text;
        if (!_weaponTiers.Contains(weaponTypeToAdd)) {
            _weaponTiers.Add(weaponTypeToAdd);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, weaponsContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = weaponTypeToAdd;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
            go.GetComponent<WeaponTypeButton>().categoryName = "weapon";
        }
    }
    public void OnRemoveWeapon() {
        if (currentSelectedWeaponButton != null) {
            string weaponTypeToRemove = currentSelectedWeaponButton.buttonText.text;
            if (_weaponTiers.Remove(weaponTypeToRemove)) {
                GameObject.Destroy(currentSelectedWeaponButton.gameObject);
                currentSelectedWeaponButton = null;
            }
        }
    }
    public void OnAddArmor() {
        string armorToAdd = armorsOptions.options[armorsOptions.value].text;
        if (!_armorTiers.Contains(armorToAdd)) {
            _armorTiers.Add(armorToAdd);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, armorsContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = armorToAdd;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
            go.GetComponent<WeaponTypeButton>().categoryName = "armor";
        }
    }
    public void OnRemoveArmor() {
        if (currentSelectedArmorButton != null) {
            string armorToRemove = currentSelectedArmorButton.buttonText.text;
            if (_armorTiers.Remove(armorToRemove)) {
                GameObject.Destroy(currentSelectedArmorButton.gameObject);
                currentSelectedArmorButton = null;
            }
        }
    }
    public void OnAddAccessory() {
        string accessoryToAdd = accessoriesOptions.options[accessoriesOptions.value].text;
        if (!_accessoryTiers.Contains(accessoryToAdd)) {
            _accessoryTiers.Add(accessoryToAdd);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, accessoriesContentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = accessoryToAdd;
            go.GetComponent<WeaponTypeButton>().panelName = "class";
            go.GetComponent<WeaponTypeButton>().categoryName = "accessory";
        }
    }
    public void OnRemoveAccessory() {
        if (currentSelectedAccessoryButton != null) {
            string accessoryToRemove = currentSelectedAccessoryButton.buttonText.text;
            if (_accessoryTiers.Remove(accessoryToRemove)) {
                GameObject.Destroy(currentSelectedAccessoryButton.gameObject);
                currentSelectedAccessoryButton = null;
            }
        }
    }
    public void OnAddTrait() {
        string traitToAdd = traitOptions.options[traitOptions.value].text;
        if (!_traitNames.Contains(traitToAdd)) {
            _traitNames.Add(traitToAdd);
            GameObject go = GameObject.Instantiate(traitBtnGO, traitsScrollRect.content);
            go.GetComponent<ClassTraitButton>().SetTraitName(traitToAdd);
        }
    }
    public void OnRemoveTrait() {
        if (currentSelectedClassTraitButton != null) {
            string traitToRemove = currentSelectedClassTraitButton.buttonText.text;
            if (_traitNames.Remove(traitToRemove)) {
                GameObject.Destroy(currentSelectedClassTraitButton.gameObject);
                currentSelectedClassTraitButton = null;
            }
        }
    }
    public void OnClickWeaponsTab() {
        weaponsGO.SetActive(true);
        armorsGO.SetActive(false);
        accessoriesGO.SetActive(false);
        traitsGO.SetActive(false);
    }
    public void OnClickArmorsTab() {
        weaponsGO.SetActive(false);
        armorsGO.SetActive(true);
        accessoriesGO.SetActive(false);
        traitsGO.SetActive(false);
    }
    public void OnClickAccessoriesTab() {
        weaponsGO.SetActive(false);
        armorsGO.SetActive(false);
        accessoriesGO.SetActive(true);
        traitsGO.SetActive(false);
    }
    public void OnClickTraitsTab() {
        weaponsGO.SetActive(false);
        armorsGO.SetActive(false);
        accessoriesGO.SetActive(false);
        traitsGO.SetActive(true);
    }
    #endregion
}
