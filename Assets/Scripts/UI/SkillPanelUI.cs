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

public class SkillPanelUI : MonoBehaviour {
    public static SkillPanelUI Instance;

    public InputField skillNameInput;
    public InputField skillDescInput;
    public InputField powerInput;
    public InputField spCostInput;
    public InputField actWeightInput;
    public InputField rangeInput;
    public InputField cellAmountInput;

    public Dropdown attackTypeOptions;
    public Dropdown elementOptions;
    public Dropdown targetTypeOptions;
    public Dropdown allowedWeaponsOptions;

    public GameObject cellAmountGO;
    public GameObject weaponTypeBtnGO;

    public Transform contentTransform;

    [NonSerialized] public WeaponTypeButton currentSelectedWeaponTypeButton;
    [NonSerialized] public List<string> allSkills;
    //private string[] _attackCategories;
    //private string[] _elements;
    //private string[] _targetTypes;
    //private string[] _weaponTypes;

    private List<string> _allowedWeaponTypes;

    void Awake() {
        Instance = this;
    }
    // Use this for initialization
    void Start () {
        _allowedWeaponTypes = new List<string>();
        allSkills = new List<string>();
        LoadAllData();
	}

    #region Utilities
    private void LoadAllData() {
        attackTypeOptions.ClearOptions();
        elementOptions.ClearOptions();
        targetTypeOptions.ClearOptions();
        allowedWeaponsOptions.ClearOptions();

        string[] attackCategories = System.Enum.GetNames(typeof(ATTACK_CATEGORY));
        string[] elements = System.Enum.GetNames(typeof(ELEMENT));
        string[] targetTypes = System.Enum.GetNames(typeof(TARGET_TYPE));
        string[] weaponTypes = System.Enum.GetNames(typeof(WEAPON_TYPE));

        attackTypeOptions.AddOptions(attackCategories.ToList());
        elementOptions.AddOptions(elements.ToList());
        targetTypeOptions.AddOptions(targetTypes.ToList());
        allowedWeaponsOptions.AddOptions(weaponTypes.ToList());

        allSkills = new List<string>();
        UpdateSkillList();
    }

    private void ClearData() {
        currentSelectedWeaponTypeButton = null;
        skillNameInput.text = string.Empty;
        skillDescInput.text = string.Empty;

        powerInput.text = "0";
        spCostInput.text = "0";
        actWeightInput.text = "0";
        rangeInput.text = "0";

        attackTypeOptions.value = 0;
        elementOptions.value = 0;
        targetTypeOptions.value = 0;
        allowedWeaponsOptions.value = 0;

        _allowedWeaponTypes.Clear();
        foreach (Transform child in contentTransform) {
            GameObject.Destroy(child.gameObject);
        }
    }
    private void SaveSkill() {
        if(skillNameInput.text == string.Empty) {
            EditorUtility.DisplayDialog("Error", "Please specify a Skill Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "Skills/CLASS/ATTACK/" + skillNameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Skill", "A skill with name " + skillNameInput.text + " already exists. Replace with this skill?", "Yes", "No")) {
                File.Delete(path);
                SaveSkillJson(path);
            }
        } else {
            SaveSkillJson(path);
        }
    }
    private void SaveSkillJson(string path) {
        AttackSkill newSkill = new AttackSkill();

        SetCommonData(newSkill);

        newSkill.power = int.Parse(powerInput.text);
        newSkill.spCost = int.Parse(spCostInput.text);
        newSkill.attackCategory = (ATTACK_CATEGORY) System.Enum.Parse(typeof(ATTACK_CATEGORY), attackTypeOptions.options[attackTypeOptions.value].text);
        newSkill.element = (ELEMENT) System.Enum.Parse(typeof(ELEMENT), elementOptions.options[elementOptions.value].text);

        string jsonString = JsonUtility.ToJson(newSkill);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
        Debug.Log("Successfully saved skill at " + path);

        UpdateSkillList();
    }

    private void SetCommonData(Skill newSkill) {
        newSkill.skillType = SKILL_TYPE.ATTACK;
        newSkill.skillName = skillNameInput.text;
        newSkill.skillCategory = SKILL_CATEGORY.CLASS;
        newSkill.description = skillDescInput.text;
        newSkill.activationWeight = int.Parse(actWeightInput.text);
        newSkill.range = int.Parse(rangeInput.text);
        newSkill.targetType = (TARGET_TYPE) System.Enum.Parse(typeof(TARGET_TYPE), targetTypeOptions.options[targetTypeOptions.value].text);
        
        newSkill.numOfRowsHit = int.Parse(cellAmountInput.text);
        if (newSkill.numOfRowsHit <= 0) {
            newSkill.numOfRowsHit = 1;
        }
        //newSkill.skillRequirements = skillComponent.skillRequirements;
        newSkill.allowedWeaponTypes = new WEAPON_TYPE[_allowedWeaponTypes.Count];
        for (int i = 0; i < _allowedWeaponTypes.Count; i++) {
            newSkill.allowedWeaponTypes[i] = (WEAPON_TYPE) System.Enum.Parse(typeof(WEAPON_TYPE), _allowedWeaponTypes[i]);
        }
    }

    public void LoadSkill() {
        string filePath = EditorUtility.OpenFilePanel("Select Skill", Utilities.dataPath + "Skills/CLASS/ATTACK/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
            ClearData();
            LoadSkillDataToUI(attackSkill);
        }
    }

    private void LoadSkillDataToUI(AttackSkill attackSkill) {
        skillNameInput.text = attackSkill.skillName;
        skillDescInput.text = attackSkill.description;
        powerInput.text = attackSkill.power.ToString();
        spCostInput.text = attackSkill.spCost.ToString();
        actWeightInput.text = attackSkill.activationWeight.ToString();
        rangeInput.text = attackSkill.range.ToString();
        cellAmountInput.text = attackSkill.numOfRowsHit.ToString();

        attackTypeOptions.value = GetAttackTypeIndex(attackSkill.attackCategory);
        elementOptions.value = GetElementIndex(attackSkill.element);
        targetTypeOptions.value = GetTargetTypeIndex(attackSkill.targetType);

        for (int i = 0; i < attackSkill.allowedWeaponTypes.Length; i++) {
            string weaponType = attackSkill.allowedWeaponTypes[i].ToString();
            _allowedWeaponTypes.Add(weaponType);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, contentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = weaponType;
            go.GetComponent<WeaponTypeButton>().panelName = "skill";
        }
    }
    private int GetAttackTypeIndex(ATTACK_CATEGORY attackType) {
        for (int i = 0; i < attackTypeOptions.options.Count; i++) {
            if (attackTypeOptions.options[i].text == attackType.ToString()) {
                return i;
            }
        }
        return 0;
    }
    private int GetElementIndex(ELEMENT element) {
        for (int i = 0; i < elementOptions.options.Count; i++) {
            if (elementOptions.options[i].text == element.ToString()) {
                return i;
            }
        }
        return 0;
    }
    private int GetTargetTypeIndex(TARGET_TYPE targetType) {
        for (int i = 0; i < targetTypeOptions.options.Count; i++) {
            if (targetTypeOptions.options[i].text == targetType.ToString()) {
                return i;
            }
        }
        return 0;
    }
    private void UpdateSkillList() {
        allSkills.Clear();
        string path = Utilities.dataPath + "Skills/CLASS/ATTACK/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allSkills.Add(Path.GetFileNameWithoutExtension(file));
        }

        foreach (Transform child in ClassPanelUI.Instance.skillsContentTransform) {
            child.GetComponent<LevelCollapseUI>().UpdateSkillList();
        }

        MonsterPanelUI.Instance.UpdateSkillList();
    }
    #endregion

    #region OnValueChanged
    public void OnTargetTypeChanged(int index) {
        TARGET_TYPE targetType = (TARGET_TYPE) System.Enum.Parse(typeof(TARGET_TYPE), targetTypeOptions.options[index].text);
        if (targetType == TARGET_TYPE.SINGLE) {
            cellAmountGO.SetActive(false);
        }else if (targetType == TARGET_TYPE.ROW) {
            cellAmountGO.SetActive(true);
        }
    }
    #endregion

    #region Button Clicks
    public void OnAddNewSkill() {
        ClearData();
    }
    public void OnAddWeaponType() {
        string weaponTypeToAdd = allowedWeaponsOptions.options[allowedWeaponsOptions.value].text;
        if (weaponTypeToAdd != "NONE" && !_allowedWeaponTypes.Contains(weaponTypeToAdd)) {
            _allowedWeaponTypes.Add(weaponTypeToAdd);
            GameObject go = GameObject.Instantiate(weaponTypeBtnGO, contentTransform);
            go.GetComponent<WeaponTypeButton>().buttonText.text = weaponTypeToAdd;
            go.GetComponent<WeaponTypeButton>().panelName = "skill";
        }
    }
    public void OnRemoveWeaponType() {
        if(currentSelectedWeaponTypeButton != null) {
            string weaponTypeToRemove = currentSelectedWeaponTypeButton.buttonText.text;
            if (_allowedWeaponTypes.Remove(weaponTypeToRemove)) {
                GameObject.Destroy(currentSelectedWeaponTypeButton.gameObject);
                currentSelectedWeaponTypeButton = null;
            }
        }
    }
    public void OnSaveSkill() {
        SaveSkill();
    }
    public void OnEditSkill() {
        LoadSkill();
    }
    #endregion
}
#endif