using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlayerResearchUI : MonoBehaviour {
    private TheSpire spire { get; set; }
    private Minion chosenMinion;

    #region General
    public void OnClickResearch(BaseLandmark landmark) {
        spire = landmark as TheSpire;
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), "Select minion", CanChooseMinion, OnHoverEnterMinion, OnHoverExitMinion, OnChooseMinion, OnConfirmExtract, "Extract");
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = character.name + " is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SPELL_SOURCE)) {
                message = character.name + " does not have the required trait: Spell Source";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
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
