using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;
using Traits;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CombatAttributePanelUI : MonoBehaviour {
    public static CombatAttributePanelUI Instance;

    //General
    public InputField nameInput;
    public InputField descriptionInput;
    public InputField thoughtInput;
    public InputField durationInput;
    public Dropdown traitTypeOptions;
    public Dropdown traitEffectOptions;
    public Dropdown traitTriggerOptions;
    public Dropdown associatedInteractionOptions;
    public Dropdown crimeSeverityOptions;

    //Effects
    public InputField amountInput;
    public InputField traitEffectDescriptionInput;
    public Dropdown statOptions;
    public Dropdown requirementCheckerOptions;
    public Dropdown requirementTargetOptions;
    public Dropdown requirementDamageIdentifierOptions;
    public Dropdown requirementTypeOptions;
    public Dropdown requirementSeparatorOptions;
    public Dropdown requirementOptions;
    public Toggle percentageToggle;
    public Toggle hasRequirementToggle;
    public Toggle isNotToggle;
    public Toggle _isHidden;
    public ScrollRect requirementsScrollRect;
    public ScrollRect effectsScrollRect;
    public GameObject traitEffectBtnGO;
    public GameObject requirementBtnGO;
    public GameObject requirementsParentGO;
    public InputField mutuallyExclusiveInput;
    [NonSerialized] public TraitEffectButton currentSelectedTraitEffectButton;
    [NonSerialized] public RequirementButton currentSelectedRequirementButton;

    private List<string> _allCombatAttributes;
    private List<TraitEffect> _effects;
    private List<string> _requirements;

    #region getters/setters
    public List<string> allCombatAttributes {
        get { return _allCombatAttributes; }
    }
    #endregion
    private void Awake() {
        Instance = this;
    }
    private void UpdateCombatAttributes() {
        _allCombatAttributes.Clear();
        string path = Utilities.dataPath + "CombatAttributes/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            _allCombatAttributes.Add(Path.GetFileNameWithoutExtension(file));
        }
        ItemPanelUI.Instance.UpdateAttributeOptions();
        CharacterPanelUI.Instance.UpdateCombatAttributeOptions();
        ClassPanelUI.Instance.UpdateTraitOptions();
        RacePanelUI.Instance.UpdateTraitOptions();
    }
    public void LoadAllData() {
        _allCombatAttributes = new List<string>();
        _requirements = new List<string>();
        _effects = new List<TraitEffect>();

        statOptions.ClearOptions();
        traitTypeOptions.ClearOptions();
        traitEffectOptions.ClearOptions();
        traitTriggerOptions.ClearOptions();
        associatedInteractionOptions.ClearOptions();
        crimeSeverityOptions.ClearOptions();
        requirementTypeOptions.ClearOptions();
        requirementOptions.ClearOptions();
        requirementTargetOptions.ClearOptions();
        requirementCheckerOptions.ClearOptions();
        requirementDamageIdentifierOptions.ClearOptions();
        requirementSeparatorOptions.ClearOptions();

        string[] stats = System.Enum.GetNames(typeof(STAT));
        string[] traitTypes = System.Enum.GetNames(typeof(TRAIT_TYPE));
        string[] traitEffects = System.Enum.GetNames(typeof(TRAIT_EFFECT));
        string[] traitTriggers = System.Enum.GetNames(typeof(TRAIT_TRIGGER));
        string[] associatedInteractions = System.Enum.GetNames(typeof(INTERACTION_TYPE));
        string[] crimeSeverities = System.Enum.GetNames(typeof(CRIME_CATEGORY));
        string[] requirementTypes = System.Enum.GetNames(typeof(TRAIT_REQUIREMENT));
        string[] requirementTargets = System.Enum.GetNames(typeof(TRAIT_REQUIREMENT_TARGET));
        string[] requirementCheckers = System.Enum.GetNames(typeof(TRAIT_REQUIREMENT_CHECKER));
        string[] requirementDamageIdentifiers = System.Enum.GetNames(typeof(DAMAGE_IDENTIFIER));
        string[] requirementSeparators = System.Enum.GetNames(typeof(TRAIT_REQUIREMENT_SEPARATOR));

        statOptions.AddOptions(stats.ToList());
        traitTypeOptions.AddOptions(traitTypes.ToList());
        traitEffectOptions.AddOptions(traitEffects.ToList());
        traitTriggerOptions.AddOptions(traitTriggers.ToList());
        associatedInteractionOptions.AddOptions(associatedInteractions.ToList());
        crimeSeverityOptions.AddOptions(crimeSeverities.ToList());
        requirementTypeOptions.AddOptions(requirementTypes.ToList());
        requirementTargetOptions.AddOptions(requirementTargets.ToList());
        requirementCheckerOptions.AddOptions(requirementCheckers.ToList());
        requirementDamageIdentifierOptions.AddOptions(requirementDamageIdentifiers.ToList());
        requirementSeparatorOptions.AddOptions(requirementSeparators.ToList());

        //requirementTypeOptions.value = 0;
        OnRequirementTypeChange(requirementTypeOptions.value);

        UpdateCombatAttributes();
    }
    private void ClearData() {
        currentSelectedTraitEffectButton = null;
        currentSelectedRequirementButton = null;

        statOptions.value = 0;
        traitTypeOptions.value = 0;
        traitEffectOptions.value = 0;
        traitTriggerOptions.value = 0;
        associatedInteractionOptions.value = 0;
        crimeSeverityOptions.value = 0;
        requirementTypeOptions.value = 0;
        requirementOptions.value = 0;
        requirementTargetOptions.value = 0;
        requirementCheckerOptions.value = 0;
        requirementDamageIdentifierOptions.value = 0;
        requirementSeparatorOptions.value = 0;

        nameInput.text = string.Empty;
        descriptionInput.text = string.Empty;
        thoughtInput.text = string.Empty;
        traitEffectDescriptionInput.text = string.Empty;
        mutuallyExclusiveInput.text = string.Empty;
        amountInput.text = "0";
        durationInput.text = "0";

        percentageToggle.isOn = false;
        hasRequirementToggle.isOn = false;
        isNotToggle.isOn = false;
        _isHidden.isOn = false;

        _effects.Clear();
        _requirements.Clear();
        effectsScrollRect.content.DestroyChildren();
        requirementsScrollRect.content.DestroyChildren();
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
        //float amountInp = 0f;
        //if (!string.IsNullOrEmpty(amountInput.text)) {
        //    amountInp = float.Parse(amountInput.text);
        //}
        Trait newTrait = new Trait {
            name = nameInput.text,
            description = descriptionInput.text,
            thoughtText = thoughtInput.text,
            type = (TRAIT_TYPE)System.Enum.Parse(typeof(TRAIT_TYPE), traitTypeOptions.options[traitTypeOptions.value].text),
            effect = (TRAIT_EFFECT)System.Enum.Parse(typeof(TRAIT_EFFECT), traitEffectOptions.options[traitEffectOptions.value].text),
            daysDuration = int.Parse(durationInput.text),
            effects = _effects,
            isHidden = _isHidden.isOn,
            mutuallyExclusive = GetMutuallyExclusiveTraits()
        };
        string jsonString = JsonUtility.ToJson(newTrait);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved trait at " + path);

        UpdateCombatAttributes();
    }

    private void LoadCombatAttribute() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Trait", Utilities.dataPath + "CombatAttributes/", "json");
        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            Trait attribute = JsonUtility.FromJson<Trait>(dataAsJson);
            ClearData();
            LoadCombatAttributeDataToUI(attribute);
        }
#endif
    }
    private void LoadCombatAttributeDataToUI(Trait trait) {
        nameInput.text = trait.name;
        descriptionInput.text = trait.description;
        thoughtInput.text = trait.thoughtText;
        traitTypeOptions.value = GetOptionIndex(trait.type.ToString(), traitTypeOptions);
        traitEffectOptions.value = GetOptionIndex(trait.effect.ToString(), traitEffectOptions);
        durationInput.text = trait.daysDuration.ToString();
        mutuallyExclusiveInput.text = ConvertMutuallyExclusiveTraitsToText(trait);

        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            _effects.Add(traitEffect);
            GameObject go = GameObject.Instantiate(traitEffectBtnGO, effectsScrollRect.content);
            go.GetComponent<TraitEffectButton>().SetTraitEffect(traitEffect);
        }
    }
    private void PopulateRequirements(List<string> requirements) {
        if(requirements != null) {
            requirementOptions.ClearOptions();
            requirementOptions.AddOptions(requirements);
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
    private List<string> GetCombatRequirementsByType(TRAIT_REQUIREMENT requirementType) {
        //if (requirementType == TRAIT_REQUIREMENT.ATTRIBUTE) {
        //    return System.Enum.GetNames(typeof(ATTRIBUTE)).ToList();
        //} else if (requirementType == TRAIT_REQUIREMENT.CLASS) {
        //    return ClassPanelUI.Instance.allClasses;
        //} else if (requirementType == TRAIT_REQUIREMENT.ELEMENT) {
        //    return System.Enum.GetNames(typeof(ELEMENT)).ToList();
        //}
        if (requirementType == TRAIT_REQUIREMENT.RACE) {
            return System.Enum.GetNames(typeof(RACE)).ToList();
        }else if (requirementType == TRAIT_REQUIREMENT.TRAIT) {
            return _allCombatAttributes;
        } else if (requirementType == TRAIT_REQUIREMENT.ROLE) {
            return System.Enum.GetNames(typeof(CHARACTER_ROLE)).ToList();
        }
        return null;
    }

    public void OnRequirementTypeChange(int index) {
        TRAIT_REQUIREMENT requirementType = (TRAIT_REQUIREMENT) System.Enum.Parse(typeof(TRAIT_REQUIREMENT), requirementTypeOptions.options[index].text);
        List<string> requirements = GetCombatRequirementsByType(requirementType);
        PopulateRequirements(requirements);
    }
    public void OnToggleRequirement(bool state) {
        RequirementOptionsActivation(state);
    }
    private void RequirementOptionsActivation(bool state) {
        requirementsParentGO.SetActive(state);
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
    public void OnClickAddRequirement() {
        string requirementToAdd = requirementOptions.options[requirementOptions.value].text;
        if (!_requirements.Contains(requirementToAdd)) {
            _requirements.Add(requirementToAdd);
            GameObject go = GameObject.Instantiate(requirementBtnGO, requirementsScrollRect.content);
            go.GetComponent<RequirementButton>().SetRequirement(requirementToAdd);
        }
    }
    public void OnClickRemoveRequirement() {
        if (currentSelectedRequirementButton != null) {
            string requirementToRemove = currentSelectedRequirementButton.requirement;
            if (_requirements.Remove(requirementToRemove)) {
                GameObject.Destroy(currentSelectedRequirementButton.gameObject);
                currentSelectedRequirementButton = null;
            }
        }
    }
    public void OnClickAddTraitEffect() {
        TraitEffect traitEffect = new TraitEffect {
            stat = (STAT) System.Enum.Parse(typeof(STAT), statOptions.options[statOptions.value].text),
            amount = float.Parse(amountInput.text),
            isPercentage = percentageToggle.isOn,
            target = (TRAIT_REQUIREMENT_TARGET) System.Enum.Parse(typeof(TRAIT_REQUIREMENT_TARGET), requirementTargetOptions.options[requirementTargetOptions.value].text),
            checker = (TRAIT_REQUIREMENT_CHECKER) System.Enum.Parse(typeof(TRAIT_REQUIREMENT_CHECKER), requirementCheckerOptions.options[requirementCheckerOptions.value].text),
            damageIdentifier = (DAMAGE_IDENTIFIER) System.Enum.Parse(typeof(DAMAGE_IDENTIFIER), requirementDamageIdentifierOptions.options[requirementDamageIdentifierOptions.value].text),
            description = traitEffectDescriptionInput.text,
            hasRequirement = hasRequirementToggle.isOn,
            isNot = isNotToggle.isOn,
            requirementType = (TRAIT_REQUIREMENT) System.Enum.Parse(typeof(TRAIT_REQUIREMENT), requirementTypeOptions.options[requirementTypeOptions.value].text),
            requirementSeparator = (TRAIT_REQUIREMENT_SEPARATOR) System.Enum.Parse(typeof(TRAIT_REQUIREMENT_SEPARATOR), requirementSeparatorOptions.options[requirementSeparatorOptions.value].text),
            requirements = new List<string>(_requirements)
        };
        _effects.Add(traitEffect);
        GameObject go = GameObject.Instantiate(traitEffectBtnGO, effectsScrollRect.content);
        go.GetComponent<TraitEffectButton>().SetTraitEffect(traitEffect);
    }
    public void OnClickRemoveTraitEffect() {
        if (currentSelectedTraitEffectButton != null) {
            TraitEffect traitEffectToRemove = currentSelectedTraitEffectButton.traitEffect;
            if (_effects.Remove(traitEffectToRemove)) {
                GameObject.Destroy(currentSelectedTraitEffectButton.gameObject);
                currentSelectedTraitEffectButton = null;
            }
        }
    }
    #endregion

    #region Mutually Exclusive
    private string[] GetMutuallyExclusiveTraits() {
        string text = mutuallyExclusiveInput.text;
        //string[] words = text.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return Utilities.ConvertStringToArray(text, ',');
    }
    private string ConvertMutuallyExclusiveTraitsToText(Trait trait) {
        return Utilities.ConvertArrayToString(trait.mutuallyExclusive, ',');
        //string text = string.Empty;
        //if (trait.mutuallyExclusive != null) {
        //    for (int i = 0; i < trait.mutuallyExclusive.Length; i++) {
        //        text += trait.mutuallyExclusive[i];
        //        if (i + 1 < trait.mutuallyExclusive.Length) {
        //            text += ",";
        //        }
        //    }
        //}
        //return text;
    }
    #endregion
}
