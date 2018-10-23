using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CombatAttributePanelUI : MonoBehaviour {
    public static CombatAttributePanelUI Instance;

    public InputField nameInput;
    public InputField descriptionInput;
    public InputField amountInput;

    public Dropdown statOptions;
    public Dropdown damageIdentifierOptions;
    public Dropdown requirementTypeOptions;
    public Dropdown requirementOptions;

    public Toggle percentageToggle;
    public Toggle hasRequirementToggle;

    private List<string> _allCombatAttributes;

    #region getters/setters
    public List<string> allCombatAttributes {
        get { return _allCombatAttributes; }
    }
    #endregion
    private void Awake() {
        Instance = this;
        _allCombatAttributes = new List<string>();
    }
    private void Start() {
        LoadAllData();
    }
    private void UpdateCombatAttributes() {
        _allCombatAttributes.Clear();
        string path = Utilities.dataPath + "CombatAttributes/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            _allCombatAttributes.Add(Path.GetFileNameWithoutExtension(file));
        }
        ItemPanelUI.Instance.UpdateAttributeOptions();
        CharacterPanelUI.Instance.UpdateCombatAttributeOptions();
    }
    private void LoadAllData() {
        statOptions.ClearOptions();
        damageIdentifierOptions.ClearOptions();
        requirementTypeOptions.ClearOptions();
        requirementOptions.ClearOptions();

        string[] stats = System.Enum.GetNames(typeof(STAT));
        string[] damageIdentifier = System.Enum.GetNames(typeof(DAMAGE_IDENTIFIER));
        string[] requirementTypes = System.Enum.GetNames(typeof(COMBAT_ATTRIBUTE_REQUIREMENT));

        statOptions.AddOptions(stats.ToList());
        damageIdentifierOptions.AddOptions(damageIdentifier.ToList());
        requirementTypeOptions.AddOptions(requirementTypes.ToList());
        requirementTypeOptions.value = 0;

        UpdateCombatAttributes();
    }
    private void ClearData() {
        statOptions.value = 0;
        damageIdentifierOptions.value = 0;
        requirementTypeOptions.value = 0;
        requirementOptions.value = 0;

        nameInput.text = string.Empty;
        descriptionInput.text = string.Empty;
        amountInput.text = string.Empty;

        percentageToggle.isOn = false;
        hasRequirementToggle.isOn = false;
    }

    private void SaveCombatAttribute() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(nameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Combat Attribute Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "CombatAttributes/" + nameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Combat Attribute", "A combat attribute with name " + nameInput.text + " already exists. Replace with this combat attribute?", "Yes", "No")) {
                File.Delete(path);
                SaveCombatAttributeJson(path);
            }
        } else {
            SaveCombatAttributeJson(path);
        }
#endif
    }

    private void SaveCombatAttributeJson(string path) {
        CombatAttribute combatAttribute = new CombatAttribute {
            name = nameInput.text,
            description = descriptionInput.text,
            amount = float.Parse(amountInput.text),
            stat = (STAT) System.Enum.Parse(typeof(STAT), statOptions.options[statOptions.value].text),
            damageIdentifier = (DAMAGE_IDENTIFIER) System.Enum.Parse(typeof(DAMAGE_IDENTIFIER), damageIdentifierOptions.options[damageIdentifierOptions.value].text),
            requirementType = (COMBAT_ATTRIBUTE_REQUIREMENT) System.Enum.Parse(typeof(COMBAT_ATTRIBUTE_REQUIREMENT), requirementTypeOptions.options[requirementTypeOptions.value].text),
            hasRequirement = hasRequirementToggle.isOn,
            isPercentage = percentageToggle.isOn
        };

        if (hasRequirementToggle.isOn) {
            CombatAttribute newCombatAtt = combatAttribute;
            newCombatAtt.requirement = requirementOptions.options[requirementOptions.value].text;
            combatAttribute = newCombatAtt;
        }
        string jsonString = JsonUtility.ToJson(combatAttribute);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved combat attribute at " + path);

        UpdateCombatAttributes();
    }

    private void LoadCombatAttribute() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Combat Attribute", Utilities.dataPath + "CombatAttributes/", "json");
        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            CombatAttribute attribute = JsonUtility.FromJson<CombatAttribute>(dataAsJson);
            ClearData();
            LoadCombatAttributeDataToUI(attribute);
        }
#endif
    }
    private void LoadCombatAttributeDataToUI(CombatAttribute attribute) {
        nameInput.text = attribute.name;
        descriptionInput.text = attribute.description;
        amountInput.text = attribute.amount.ToString();

        percentageToggle.isOn = attribute.isPercentage;
        hasRequirementToggle.isOn = attribute.hasRequirement;

        statOptions.value = GetOptionIndex(attribute.stat.ToString(), statOptions);
        damageIdentifierOptions.value = GetOptionIndex(attribute.damageIdentifier.ToString(), damageIdentifierOptions);
        requirementTypeOptions.value = GetOptionIndex(attribute.requirementType.ToString(), requirementTypeOptions);

        if (requirementOptions.transform.parent.gameObject.activeSelf) {
            requirementOptions.value = GetOptionIndex(attribute.requirement, requirementOptions);
        }
        //if (attribute.hasRequirement) {
        //    List<string> requirements = GetCombatRequirementsByType(attribute.requirementType);
        //    PopulateRequirements(requirements);
        //} else {
        //    requirementOptions.gameObject.SetActive(false);
        //}
    }
    private void PopulateRequirements(List<string> requirements) {
        if(requirements != null) {
            requirementOptions.ClearOptions();
            requirementOptions.AddOptions(requirements);
            requirementOptions.transform.parent.gameObject.SetActive(true);
        } else {
            requirementOptions.transform.parent.gameObject.SetActive(false);
        }
    }
    private int GetOptionIndex(string identifier, Dropdown options) {
        for (int i = 0; i < options.options.Count; i++) {
            if (options.options[i].text.ToLower() == identifier.ToLower()) {
                return i;
            }
        }
        return 0;
    }
    private List<string> GetCombatRequirementsByType(COMBAT_ATTRIBUTE_REQUIREMENT requirementType) {
        if (requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.ATTRIBUTE) {
            return System.Enum.GetNames(typeof(ATTRIBUTE)).ToList();
        } else if (requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.CLASS) {
            return ClassPanelUI.Instance.allClasses;
        } else if (requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.ELEMENT) {
            return System.Enum.GetNames(typeof(ELEMENT)).ToList();
        } else if (requirementType == COMBAT_ATTRIBUTE_REQUIREMENT.RACE) {
            return System.Enum.GetNames(typeof(RACE)).ToList();
        }
        return null;
    }

    public void OnRequirementTypeChange(int index) {
        COMBAT_ATTRIBUTE_REQUIREMENT requirementType = (COMBAT_ATTRIBUTE_REQUIREMENT) System.Enum.Parse(typeof(COMBAT_ATTRIBUTE_REQUIREMENT), requirementTypeOptions.options[index].text);
        List<string> requirements = GetCombatRequirementsByType(requirementType);
        PopulateRequirements(requirements);
    }
    public void OnToggleRequirement(bool state) {
        RequirementOptionsActivation(state);
    }
    private void RequirementOptionsActivation(bool state) {
        requirementTypeOptions.transform.parent.gameObject.SetActive(state);
        if (!state) {
            requirementOptions.transform.parent.gameObject.SetActive(false);
        }
    }
    #region Button Clicks
    public void OnClickAddNewAttribute() {
        ClearData();
    }
    public void OnClickEditAttribute() {
        LoadCombatAttribute();
    }
    public void OnClickSaveAttribute() {
        SaveCombatAttribute();
    }
    #endregion
}
