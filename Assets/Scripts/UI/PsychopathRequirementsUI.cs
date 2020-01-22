﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Traits;

public class PsychopathRequirementsUI : MonoBehaviour {
    public SERIAL_VICTIM_TYPE victimType { get; private set; }
    public List<string> victimDescriptions { get; private set; }

    public TMP_Dropdown reqTypeDropdown;
    public TMP_Dropdown reqDescriptionDropdown;
    public TextMeshProUGUI reqDescriptionsLabel;
    public Button addRequirementButton;
    public Button removeRequirementButton;

    private List<string> criteriaRaces = new List<string>() { "HUMANS", "ELVES" };

    public void ShowRequirementsUI() {
        if(victimDescriptions == null) {
            victimDescriptions = new List<string>();
        }
        PopulateRequirementsType();
        //reqDescriptionDropdown.gameObject.SetActive(false);
        HideAddRemoveButtons();
        reqDescriptionsLabel.text = "None";
        PopulateRequirementsDescriptions();
    }
    private void PopulateRequirementsType() {
        reqTypeDropdown.ClearOptions();
        reqTypeDropdown.AddOptions(Utilities.GetEnumChoices<SERIAL_VICTIM_TYPE>(true));
        reqTypeDropdown.value = 0;
    }
    private void PopulateRequirementsDescriptions() {
        reqDescriptionDropdown.ClearOptions();
        victimDescriptions.Clear();
        if(victimType == SERIAL_VICTIM_TYPE.NONE) {
            reqDescriptionDropdown.options.Add(new TMP_Dropdown.OptionData("NONE"));
            HideAddRemoveButtons();
        } else if (victimType == SERIAL_VICTIM_TYPE.GENDER) {
            reqDescriptionDropdown.AddOptions(Utilities.GetEnumChoices<GENDER>());
            HideAddRemoveButtons();
        } else if (victimType == SERIAL_VICTIM_TYPE.RACE) {
            reqDescriptionDropdown.AddOptions(criteriaRaces);
            HideAddRemoveButtons();
        } else if (victimType == SERIAL_VICTIM_TYPE.CLASS) {
            List<CharacterClass> allClasses = CharacterManager.Instance.GetAllClasses();
            for (int i = 0; i < allClasses.Count; i++) {
                reqDescriptionDropdown.options.Add(new TMP_Dropdown.OptionData(allClasses[i].className));
            }
            ShowAddRemoveButtons();
        } else if (victimType == SERIAL_VICTIM_TYPE.TRAIT) {
            foreach (Trait trait in TraitManager.Instance.allTraits.Values) {
                if (trait.type != TRAIT_TYPE.STATUS) {
                    reqDescriptionDropdown.options.Add(new TMP_Dropdown.OptionData(trait.name));
                }
            }
            ShowAddRemoveButtons();
        }
        reqDescriptionDropdown.value = 0;
        reqDescriptionDropdown.RefreshShownValue();
        UpdateRequirementsLabel();
    }
    private void ShowAddRemoveButtons() {
        addRequirementButton.gameObject.SetActive(true);
        removeRequirementButton.gameObject.SetActive(true);
    }
    private void HideAddRemoveButtons() {
        addRequirementButton.gameObject.SetActive(false);
        removeRequirementButton.gameObject.SetActive(false);
    }
    private void UpdateRequirementsLabel() {
        string desc = string.Empty;
        if(victimType == SERIAL_VICTIM_TYPE.GENDER || victimType == SERIAL_VICTIM_TYPE.RACE) {
            desc = reqDescriptionDropdown.options[reqDescriptionDropdown.value].text;
        } else {
            if(victimDescriptions.Count > 0) {
                for (int i = 0; i < victimDescriptions.Count; i++) {
                    if(i > 0) {
                        desc += ", ";
                    }
                    desc += victimDescriptions[i];
                }
            } else {
                desc = "None";
            }
        }
        reqDescriptionsLabel.text = victimType.ToString() + ": " + desc;
    }

    #region On Value Change
    public void OnChangedReqType(int index) {
        victimType = (SERIAL_VICTIM_TYPE) reqTypeDropdown.value;
        PopulateRequirementsDescriptions();
    }
    public void OnChangedReqDescription(int index) {
        UpdateRequirementsLabel();
    }
    #endregion

    #region Button Functions
    public void OnClickAddReq() {
        string desc = reqDescriptionDropdown.options[reqDescriptionDropdown.value].text;
        if (!victimDescriptions.Contains(desc)) {
            victimDescriptions.Add(desc);
            UpdateRequirementsLabel();
        }
    }
    public void OnClickRemoveReq() {
        victimDescriptions.Remove(reqDescriptionDropdown.options[reqDescriptionDropdown.value].text);
        UpdateRequirementsLabel();
    }
    #endregion
}