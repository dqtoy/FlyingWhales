using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlayerResearchUI : MonoBehaviour {
    [Header("General")]
    public Button researchBtn;
    public Image researchProgress;

    public TheSpire spire { get; private set; }

    private Minion chosenMinion;

    #region General
    public void ShowPlayerResearchUI(TheSpire spire) {
        this.spire = spire;

        UpdatePlayerResearchUI();
        gameObject.SetActive(true);
    }
    public void HidePlayerResearchUI() {
        gameObject.SetActive(false);
    }
    public void OnClickResearch() {
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), "Select minion", CanChooseMinion, null, null, OnChooseMinion, OnConfirmExtract, "Extract");
    }
    private void OnConfirmExtract(object minionObj, object abilityObj) {
        Minion minion = (minionObj as Character).minion;
        string abilityName = (string)abilityObj;
        INTERVENTION_ABILITY ability = INTERVENTION_ABILITY.NONE;
        for (int i = 0; i < chosenMinion.interventionAbilitiesToResearch.Count; i++) {
            INTERVENTION_ABILITY currAbility = chosenMinion.interventionAbilitiesToResearch[i];
            if (PlayerManager.Instance.allInterventionAbilitiesData[currAbility].name == abilityName) {
                ability = currAbility;
                break;
            }
        }

        spire.tileLocation.region.SetAssignedMinion(minion);
        minion.SetAssignedRegion(spire.tileLocation.region);
        spire.ExtractInterventionAbility(ability);
        UpdateResearchButton();
    }
    public void UpdatePlayerResearchUI() {
        if (researchProgress.gameObject.activeSelf && spire.isInCooldown) {
            researchProgress.fillAmount = spire.currentCooldownTick / (float) spire.cooldownDuration;
        }
    }
    private void UpdateResearchButton() {
        researchProgress.gameObject.SetActive(spire.isInCooldown);
        researchBtn.interactable = !spire.isInCooldown;

        if (!researchBtn.interactable) {
            researchProgress.gameObject.SetActive(true);
            researchProgress.fillAmount = 0;
        }
    }
    #endregion

    #region Minion
    private void OnChooseMinion(object characterObj) {
        //populate the second column
        Minion chosenMinion = (characterObj as Character).minion;
        this.chosenMinion = chosenMinion;
        List<string> abilities = new List<string>();
        for (int i = 0; i < chosenMinion.interventionAbilitiesToResearch.Count; i++) {
            abilities.Add(PlayerManager.Instance.allInterventionAbilitiesData[chosenMinion.interventionAbilitiesToResearch[i]].name);
        }
        UIManager.Instance.dualObjectPicker.PopulateColumn(abilities, CanChooseAbility, OnHoverAbilityChoice, OnHoverExitAbilityChoice, UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Ability");
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.interventionAbilitiesToResearch.Count > 0 && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SPELL_SOURCE);
    }
    #endregion

    #region Research
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
            info += "Cost: " + data.manaCost.ToString() + " Mana";
            UIManager.Instance.ShowSmallInfo(info);
        }
    }
    private void OnHoverExitAbilityChoice(string abilityName) {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
