using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResearchAbilityUI : MonoBehaviour {
    [Header("General")]
    public Button okBtn;
    public ToggleGroup toggleGroup;

    [Header("Ability 1")]
    public Toggle ability1Toggle;
    public Image ability1Icon;
    public TextMeshProUGUI ability1Text;
    private INTERVENTION_ABILITY ability1;

    [Header("Ability 2")]
    public Toggle ability2Toggle;
    public Image ability2Icon;
    public TextMeshProUGUI ability2Text;
    private INTERVENTION_ABILITY ability2;

    [Header("Ability 3")]
    public Toggle ability3Toggle;
    public Image ability3Icon;
    public TextMeshProUGUI ability3Text;
    private INTERVENTION_ABILITY ability3;

    private INTERVENTION_ABILITY chosenAbility;
    
    //void Start() {
    //    ability1Toggle.group = toggleGroup;
    //    ability2Toggle.group = toggleGroup;
    //    ability3Toggle.group = toggleGroup;
    //}
    public void ShowResearchUI() {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowResearchUI());
            return;
        }
        ability1Toggle.isOn = false;
        ability2Toggle.isOn = false;
        ability3Toggle.isOn = false;
        okBtn.interactable = false;

        gameObject.SetActive(true);
    }
    public void SetAbility1(INTERVENTION_ABILITY ability) {
        ability1 = ability;
        string name = Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString());
        ability1Icon.sprite = PlayerManager.Instance.GetJobActionSprite(name);
        string text = name;
        text += "\nTier: " + PlayerManager.Instance.GetInterventionAbilityTier(ability);
        ability1Text.text = text;
    }
    public void SetAbility2(INTERVENTION_ABILITY ability) {
        ability2 = ability;
        string name = Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString());
        ability2Icon.sprite = PlayerManager.Instance.GetJobActionSprite(name);
        string text = name;
        text += "\nTier: " + PlayerManager.Instance.GetInterventionAbilityTier(ability);
        ability2Text.text = text;
    }
    public void SetAbility3(INTERVENTION_ABILITY ability) {
        ability3 = ability;
        string name = Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString());
        ability3Icon.sprite = PlayerManager.Instance.GetJobActionSprite(name);
        string text = name;
        text += "\nTier: " + PlayerManager.Instance.GetInterventionAbilityTier(ability);
        ability3Text.text = text;
    }

    public void OnClickAbility1(bool state) {
        if (!state) { return; }
        chosenAbility = ability1;
        okBtn.interactable = true;
    }
    public void OnClickAbility2(bool state) {
        if (!state) { return; }
        chosenAbility = ability2;
        okBtn.interactable = true;
    }
    public void OnClickAbility3(bool state) {
        if (!state) { return; }
        chosenAbility = ability3;
        okBtn.interactable = true;
    }
    public void OnClickOk() {
        PlayerManager.Instance.player.NewCycleForNewInterventionAbility(chosenAbility);
        gameObject.SetActive(false);
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            //UIManager.Instance.Unpause(); //if no other UI was shown, unpause game
            UIManager.Instance.SetSpeedTogglesState(true);
        }
    }
}
