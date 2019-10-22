﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradeUI : MonoBehaviour {

    [Header("General")]
    public Button upgradeBtn;
    public Image upgradeProgress;

    private TheAnvil theAnvil;

    #region General
    public void ShowPlayerUpgradeUI(TheAnvil theAnvil) {
        this.theAnvil = theAnvil;

        gameObject.SetActive(true);
    }
    public void HidePlayerResearchUI() {
        gameObject.SetActive(false);
    }
    public void OnClickUpgrade() {
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character, string>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), LandmarkManager.Instance.anvilResearchData.Keys.ToList(),
            "Choose Minion", "Choose Upgrade",
            CanChooseMinion, CanChooseUpgrade,
            OnHoverEnterMinion, OnHoverAbilityChoice,
            OnHoverExitMinion, OnHoverExitAbilityChoice,
            OnConfirmUpgrade, "Upgrade");
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = character.name + " is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.RESEARCHER)) {
                message = character.name + " does not have the required trait: Researcher";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnConfirmUpgrade(object minionObj, object upgradeObj) {
        Minion minion = (minionObj as Character).minion;
        string upgrade = upgradeObj as string;

        minion.SetAssignedRegion(theAnvil.tileLocation.region);
        theAnvil.tileLocation.region.SetAssignedMinion(minion);
        theAnvil.SetUpgradeIdentifier(upgrade);
        theAnvil.StartUpgradeProcess();
    }
    public void UpdatePlayerUpgradeUI() {
        if (!string.IsNullOrEmpty(theAnvil.upgradeIdentifier)) {
            upgradeProgress.gameObject.SetActive(true);
            upgradeProgress.fillAmount = theAnvil.currentUpgradeTick / (float)theAnvil.upgradeDuration;
            upgradeBtn.interactable = false;
        } else {
            upgradeProgress.gameObject.SetActive(false);
            upgradeBtn.interactable = true;
        }
    }
    //public void OnUpgradeDone() {
    //    //reset
    //    upgradeBtn.interactable = false;
    //    upgradeProgress.fillAmount = 0;
    //    minionName.gameObject.SetActive(false);
    //    minionPortrait.gameObject.SetActive(false);
    //    upgradeText.gameObject.SetActive(false);
    //    selectMinionBtn.interactable = true;
    //    selectUpgradeBtn.interactable = false;
    //}
    #endregion

    #region Minion
    //public void OnClickSelectMinion() {
    //    List<Character> characters = new List<Character>();
    //    for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
    //        characters.Add(PlayerManager.Instance.player.minions[i].character);
    //    }
    //    string title = "Select minion to do upgrade";
    //    UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    //}
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.RESEARCHER);
    }
    //private void SetChosenMinion(Character character) {
    //    chosenMinion = character.minion;
    //    minionPortrait.GeneratePortrait(chosenMinion.character);
    //    minionName.text = chosenMinion.character.name;
    //    minionPortrait.gameObject.SetActive(true);
    //    minionName.gameObject.SetActive(true);
    //    UpdateUpgradeButton();
    //    UpdateSelectUpgradeBtn();
    //    UIManager.Instance.HideObjectPicker();
    //}
    //private void UpdateSelectMinionBtn() {
    //    selectMinionBtn.interactable = string.IsNullOrEmpty(theAnvil.upgradeIdentifier);
    //}
    #endregion

    #region Upgrade
    //public void OnClickSelectAbility() {
    //    List<string> choices = LandmarkManager.Instance.anvilResearchData.Keys.ToList();
    //    UIManager.Instance.ShowClickableObjectPicker(choices, SetChosenUpgrade, null, CanChooseUpgrade, "Select research", OnHoverAbilityChoice, OnHoverExitAbilityChoice, "intervention ability");
    //}
    private bool CanChooseUpgrade(string upgrade) {
        //check if any of the categories are already at max level.
        //if (upgrade == TheAnvil.Improved_Spells_1) {
        //    return !PlayerManager.Instance.player.AreAllInterventionSlotsMaxLevel();
        //} else if (upgrade == TheAnvil.Improved_Summoning_1) {
        //    if (PlayerManager.Instance.player.summonSlots.Count == 0) {
        //        return true;
        //    }
        //    return !PlayerManager.Instance.player.AreAllSummonSlotsMaxLevel();
        //} else if (upgrade == TheAnvil.Improved_Artifacts_1) {
        //    if (PlayerManager.Instance.player.artifactSlots.Count == 0) {
        //        return true;
        //    }
        //    return !PlayerManager.Instance.player.AreAllArtifactSlotsMaxLevel();
        //}
        if (!theAnvil.dynamicResearchData[upgrade].isResearched && PlayerManager.Instance.player.mana >= LandmarkManager.Instance.anvilResearchData[upgrade].manaCost) {
            if (LandmarkManager.Instance.anvilResearchData[upgrade].preRequisiteResearch == string.Empty) {
                return true;
            } else {
                return theAnvil.dynamicResearchData[LandmarkManager.Instance.anvilResearchData[upgrade].preRequisiteResearch].isResearched;
            }
        }
        return false;
    }
    private void OnHoverAbilityChoice(string abilityName) {
        string info = string.Empty;
        if (CanChooseUpgrade(abilityName)) {
            info = TheAnvil.GetUpgradeDescription(abilityName);
            info += "\nCost: " + LandmarkManager.Instance.anvilResearchData[abilityName].manaCost + " mana";
            info += "\nDuration: " + LandmarkManager.Instance.anvilResearchData[abilityName].durationInHours + " hours";
        } else {
            info = theAnvil.GetUnavailabilityDescription(abilityName);
            if (info != string.Empty) {
                info += "\n";
            }
            if (PlayerManager.Instance.player.mana >= LandmarkManager.Instance.anvilResearchData[abilityName].manaCost) {
                info += "Cost: " + LandmarkManager.Instance.anvilResearchData[abilityName].manaCost + " mana";
            } else {
                info += "<color=red>Cost: " + LandmarkManager.Instance.anvilResearchData[abilityName].manaCost + " mana</color>";
            }
            info += "\nDuration: " + LandmarkManager.Instance.anvilResearchData[abilityName].durationInHours + " hours";
        }
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitAbilityChoice(string abilityName) {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

}
