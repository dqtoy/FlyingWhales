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


public class SkillPanelUI : MonoBehaviour {
    public static SkillPanelUI Instance;

    public InputField skillNameInput;
    public InputField skillDescInput;
    //public InputField powerInput;
    //public InputField spCostInput;
    //public InputField actWeightInput;
    //public InputField rangeInput;
    //public InputField cellAmountInput;

    //public Dropdown attackTypeOptions;
    public Dropdown skillTypeOptions;
    public Dropdown elementOptions;
    public Dropdown targetTypeOptions;
    //public Dropdown allowedWeaponsOptions;

    public GameObject cellAmountGO;
    public GameObject weaponTypeBtnGO;

    public Transform contentTransform;

    [NonSerialized] public WeaponTypeButton currentSelectedWeaponTypeButton;
    [NonSerialized] public List<string> allSkills;
    //private string[] _attackCategories;
    //private string[] _elements;
    //private string[] _targetTypes;
    //private string[] _weaponTypes;

    //private List<string> _allowedWeaponTypes;

    void Awake() {
        Instance = this;
    }

    #region Utilities
    public void LoadAllData() {
        //attackTypeOptions.ClearOptions();
        elementOptions.ClearOptions();
        targetTypeOptions.ClearOptions();
        skillTypeOptions.ClearOptions();
        //allowedWeaponsOptions.ClearOptions();

        //string[] attackCategories = System.Enum.GetNames(typeof(ATTACK_CATEGORY));
        string[] elements = System.Enum.GetNames(typeof(ELEMENT));
        string[] targetTypes = System.Enum.GetNames(typeof(TARGET_TYPE));
        string[] skillTypes = System.Enum.GetNames(typeof(SKILL_TYPE));

        //attackTypeOptions.AddOptions(attackCategories.ToList());
        elementOptions.AddOptions(elements.ToList());
        targetTypeOptions.AddOptions(targetTypes.ToList());
        skillTypeOptions.AddOptions(skillTypes.ToList());
        //allowedWeaponsOptions.AddOptions(weaponTypes.ToList());

        allSkills = new List<string>();
        UpdateSkillList();
    }

    private void ClearData() {
        currentSelectedWeaponTypeButton = null;
        skillNameInput.text = string.Empty;
        skillDescInput.text = string.Empty;

        //powerInput.text = "0";
        //spCostInput.text = "0";
        //actWeightInput.text = "0";
        //rangeInput.text = "0";

        //attackTypeOptions.value = 0;
        elementOptions.value = 0;
        targetTypeOptions.value = 0;
        skillTypeOptions.value = 0;
        //allowedWeaponsOptions.value = 0;

        //_allowedWeaponTypes.Clear();
        //foreach (Transform child in contentTransform) {
        //    GameObject.Destroy(child.gameObject);
        //}
    }
    private void SaveSkill() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(skillNameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Skill Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "Skills/" + skillNameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Skill", "A skill with name " + skillNameInput.text + " already exists. Replace with this skill?", "Yes", "No")) {
                File.Delete(path);
                SaveSkillJson(path);
            }
        } else {
            SaveSkillJson(path);
        }
#endif
    }
    private void SaveSkillJson(string path) {
        Skill newSkill = new Skill();

        SetCommonData(newSkill);

        //newSkill.power = int.Parse(powerInput.text);
        //newSkill.spCost = int.Parse(spCostInput.text);
        //newSkill.attackCategory = (ATTACK_CATEGORY) System.Enum.Parse(typeof(ATTACK_CATEGORY), attackTypeOptions.options[attackTypeOptions.value].text);
        //newSkill.element = (ELEMENT) System.Enum.Parse(typeof(ELEMENT), elementOptions.options[elementOptions.value].text);

        string jsonString = JsonUtility.ToJson(newSkill);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved skill at " + path);

        UpdateSkillList();
    }

    private void SetCommonData(Skill newSkill) {
        newSkill.skillName = skillNameInput.text;
        //newSkill.skillCategory = SKILL_CATEGORY.CLASS;
        newSkill.description = skillDescInput.text;
        //newSkill.activationWeight = int.Parse(actWeightInput.text);
        //newSkill.range = int.Parse(rangeInput.text);
        newSkill.targetType = (TARGET_TYPE) System.Enum.Parse(typeof(TARGET_TYPE), targetTypeOptions.options[targetTypeOptions.value].text);
        newSkill.element = (ELEMENT) System.Enum.Parse(typeof(ELEMENT), elementOptions.options[elementOptions.value].text);
        newSkill.skillType = (SKILL_TYPE) System.Enum.Parse(typeof(SKILL_TYPE), skillTypeOptions.options[skillTypeOptions.value].text);

        //newSkill.numOfRowsHit = int.Parse(cellAmountInput.text);
        //if (newSkill.numOfRowsHit <= 0) {
        //    newSkill.numOfRowsHit = 1;
        //}
        //newSkill.skillRequirements = skillComponent.skillRequirements;
        //newSkill.allowedWeaponTypes = new WEAPON_TYPE[_allowedWeaponTypes.Count];
        //for (int i = 0; i < _allowedWeaponTypes.Count; i++) {
        //    newSkill.allowedWeaponTypes[i] = (WEAPON_TYPE) System.Enum.Parse(typeof(WEAPON_TYPE), _allowedWeaponTypes[i]);
        //}
    }

    public void LoadSkill() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Skill", Utilities.dataPath + "Skills/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            Skill skill = JsonUtility.FromJson<Skill>(dataAsJson);
            ClearData();
            LoadSkillDataToUI(skill);
        }
#endif
    }

    private void LoadSkillDataToUI(Skill skill) {
        skillNameInput.text = skill.skillName;
        skillDescInput.text = skill.description;
        //powerInput.text = skill.power.ToString();
        //spCostInput.text = skill.spCost.ToString();
        //actWeightInput.text = skill.activationWeight.ToString();
        //rangeInput.text = skill.range.ToString();
        //cellAmountInput.text = skill.numOfRowsHit.ToString();

        //attackTypeOptions.value = GetAttackTypeIndex(skill.attackCategory);
        elementOptions.value = GetOptionIndex(skill.element.ToString(), elementOptions);
        targetTypeOptions.value = GetOptionIndex(skill.targetType.ToString(), targetTypeOptions);
        skillTypeOptions.value = GetOptionIndex(skill.skillType.ToString(), skillTypeOptions);

        //for (int i = 0; i < skill.allowedWeaponTypes.Length; i++) {
        //    string weaponType = skill.allowedWeaponTypes[i].ToString();
        //    _allowedWeaponTypes.Add(weaponType);
        //    GameObject go = GameObject.Instantiate(weaponTypeBtnGO, contentTransform);
        //    go.GetComponent<WeaponTypeButton>().buttonText.text = weaponType;
        //    go.GetComponent<WeaponTypeButton>().panelName = "skill";
        //}
    }
    private int GetOptionIndex(string name, Dropdown ddOptions) {
        for (int i = 0; i < ddOptions.options.Count; i++) {
            if (ddOptions.options[i].text.ToLower() == name.ToLower()) {
                return i;
            }
        }
        return 0;
    }
    private void UpdateSkillList() {
        allSkills.Clear();
        string path = Utilities.dataPath + "Skills/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allSkills.Add(Path.GetFileNameWithoutExtension(file));
        }

        ClassPanelUI.Instance.UpdateSkillOptions();
        MonsterPanelUI.Instance.UpdateSkillList();
    }
    #endregion

    #region Button Clicks
    public void OnAddNewSkill() {
        ClearData();
    }
    public void OnAddWeaponType() {
        //string weaponTypeToAdd = allowedWeaponsOptions.options[allowedWeaponsOptions.value].text;
        //if (weaponTypeToAdd != "NONE" && !_allowedWeaponTypes.Contains(weaponTypeToAdd)) {
        //    _allowedWeaponTypes.Add(weaponTypeToAdd);
        //    GameObject go = GameObject.Instantiate(weaponTypeBtnGO, contentTransform);
        //    go.GetComponent<WeaponTypeButton>().buttonText.text = weaponTypeToAdd;
        //    go.GetComponent<WeaponTypeButton>().panelName = "skill";
        //}
    }
    public void OnRemoveWeaponType() {
        //if(currentSelectedWeaponTypeButton != null) {
        //    string weaponTypeToRemove = currentSelectedWeaponTypeButton.buttonText.text;
        //    if (_allowedWeaponTypes.Remove(weaponTypeToRemove)) {
        //        GameObject.Destroy(currentSelectedWeaponTypeButton.gameObject);
        //        currentSelectedWeaponTypeButton = null;
        //    }
        //}
    }
    public void OnSaveSkill() {
        SaveSkill();
    }
    public void OnEditSkill() {
        LoadSkill();
    }
    #endregion
}
