using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerResearchUI : MonoBehaviour {
    [Header("General")]
    public Button researchBtn;
    public Image researchProgress;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    [Header("Ability")]
    public TextMeshProUGUI abilityText;
    public Image abilityImg;
    public Button selectAbilityBtn;

    public TheSpire spire { get; private set; }
    public Minion chosenMinion { get; private set; }
    public INTERVENTION_ABILITY chosenAbility { get; private set; }

    #region General
    public void ShowPlayerResearchUI(TheSpire spire) {
        this.spire = spire;

        if (spire.interventionAbilityToResearch == INTERVENTION_ABILITY.NONE) {
            chosenMinion = null;
            chosenAbility = INTERVENTION_ABILITY.NONE;
            researchBtn.interactable = false;
            researchProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            abilityText.gameObject.SetActive(false);
            abilityImg.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
            selectAbilityBtn.interactable = false;
        } else {
            SetChosenMinion(spire.tileLocation.region.assignedMinion.character);
            SetChosenAbility(spire.interventionAbilityToResearch);
            UpdateSelectMinionBtn();
            UpdateSelectAbilityBtn();
            UpdatePlayerResearchUI();
        }

        gameObject.SetActive(true);
    }
    public void HidePlayerResearchUI() {
        gameObject.SetActive(false);
    }
    public void OnClickResearch() {
        spire.tileLocation.region.SetAssignedMinion(chosenMinion);
        chosenMinion.SetAssignedRegion(spire.tileLocation.region);
        spire.StartResearchNewInterventionAbility(chosenAbility, 0);
        //currentTile.region.StartBuildingStructure(chosenLandmark, chosenMinion);
        UpdateResearchButton();
        UpdateSelectMinionBtn();
        UpdateSelectAbilityBtn();
    }
    private void UpdateResearchButton() {
        researchProgress.gameObject.SetActive(false);
        researchBtn.interactable = chosenMinion != null && chosenAbility != INTERVENTION_ABILITY.NONE
            && spire.interventionAbilityToResearch == INTERVENTION_ABILITY.NONE;
        if (!researchBtn.interactable) {
            if (spire.interventionAbilityToResearch != INTERVENTION_ABILITY.NONE) {
                researchProgress.gameObject.SetActive(true);
                researchProgress.fillAmount = 0;
            }
        }
    }
    public void UpdatePlayerResearchUI() {
        if (spire.interventionAbilityToResearch != INTERVENTION_ABILITY.NONE && researchProgress.gameObject.activeSelf) {
            researchProgress.fillAmount = spire.currentResearchTick / (float) spire.researchDuration;
        }
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion to Research";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.interventionAbilitiesToResearch.Count > 0 && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.RESEARCH_SPELL);
    }
    private void SetChosenMinion(Character character) {
        chosenMinion = character.minion;
        minionPortrait.GeneratePortrait(chosenMinion.character);
        minionName.text = chosenMinion.character.name;
        minionPortrait.gameObject.SetActive(true);
        minionName.gameObject.SetActive(true);
        UpdateResearchButton();
        UpdateSelectAbilityBtn();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = spire.interventionAbilityToResearch == INTERVENTION_ABILITY.NONE;
    }
    #endregion

    #region Research
    public void OnClickSelectAbility() {
        List<string> abilities = new List<string>();
        for (int i = 0; i < chosenMinion.interventionAbilitiesToResearch.Count; i++) {
            abilities.Add(PlayerManager.Instance.allInterventionAbilitiesData[chosenMinion.interventionAbilitiesToResearch[i]].name);
        }
        string title = "Select Ability to Research";
        UIManager.Instance.ShowClickableObjectPicker(abilities, SetChosenAbility, null, CanChooseAbility, title, OnHoverAbilityChoice, OnHoverExitAbilityChoice, "intervention ability");
    }
    private bool CanChooseAbility(string abilityName) {
        return true;
    }
    private void OnHoverAbilityChoice(string abilityName) {
        PlayerJobActionData data = null;
        for (int i = 0; i < chosenMinion.interventionAbilitiesToResearch.Count; i++) {
            INTERVENTION_ABILITY currAbility = chosenMinion.interventionAbilitiesToResearch[i];
            if (PlayerManager.Instance.allInterventionAbilitiesData[currAbility].name == abilityName) {
                data = PlayerManager.Instance.allInterventionAbilitiesData[currAbility];
                break;
            }
        }
        if (data != null) {
            string info = data.description;
            if (info != string.Empty) {
                info += "\n";
            }
            info += "Duration: " + GameManager.Instance.GetCeilingHoursBasedOnTicks(data.durationInTicks) + " hours";
            UIManager.Instance.ShowSmallInfo(info);
        }
    }
    private void OnHoverExitAbilityChoice(string abilityName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void SetChosenAbility(string abilityName) {
        INTERVENTION_ABILITY ability = INTERVENTION_ABILITY.NONE;
        for (int i = 0; i < chosenMinion.interventionAbilitiesToResearch.Count; i++) {
            INTERVENTION_ABILITY currAbility = chosenMinion.interventionAbilitiesToResearch[i];
            if (PlayerManager.Instance.allInterventionAbilitiesData[currAbility].name == abilityName) {
                ability = currAbility;
                break;
            }
        }
        if(ability != INTERVENTION_ABILITY.NONE) {
            chosenAbility = ability;
            abilityImg.sprite = PlayerManager.Instance.GetJobActionSprite(abilityName);
            abilityText.text = abilityName;
            abilityImg.gameObject.SetActive(true);
            abilityText.gameObject.SetActive(true);
        }
        UpdateResearchButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void SetChosenAbility(INTERVENTION_ABILITY abilityType) {
        if (abilityType != INTERVENTION_ABILITY.NONE) {
            PlayerJobActionData data = PlayerManager.Instance.allInterventionAbilitiesData[abilityType];
            chosenAbility = abilityType;
            abilityImg.sprite = PlayerManager.Instance.GetJobActionSprite(data.name);
            abilityText.text = data.name;
            abilityImg.gameObject.SetActive(true);
            abilityText.gameObject.SetActive(true);
        }
        UpdateResearchButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectAbilityBtn() {
        selectAbilityBtn.interactable = spire.interventionAbilityToResearch == INTERVENTION_ABILITY.NONE
            && chosenMinion != null;
    }
    #endregion
}
