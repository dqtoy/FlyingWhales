using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradeUI : MonoBehaviour {

    [Header("General")]
    public Button upgradeBtn;
    public Image upgradeProgress;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    [Header("Upgrade")]
    public TextMeshProUGUI upgradeText;
    public Button selectUpgradeBtn;

    public Minion chosenMinion { get; private set; }
    public string chosenUpgrade { get; private set; }

    private TheAnvil theAnvil;

    #region General
    public void ShowPlayerUpgradeUI(TheAnvil theAnvil) {
        this.theAnvil = theAnvil;

        if (string.IsNullOrEmpty(theAnvil.upgradeIdentifier)) {
            upgradeBtn.interactable = false;
            upgradeProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            upgradeText.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
            selectUpgradeBtn.interactable = false;
        } else {
            SetChosenMinion(theAnvil.tileLocation.region.assignedMinion.character);
            SetChosenUpgrade(theAnvil.upgradeIdentifier);
            UpdateSelectMinionBtn();
            UpdateSelectUpgradeBtn();
            UpdatePlayerUpgradeUI();
        }

        gameObject.SetActive(true);
    }
    public void HidePlayerResearchUI() {
        gameObject.SetActive(false);
    }
    public void OnClickUpgrade() {
        theAnvil.tileLocation.region.SetAssignedMinion(chosenMinion);
        theAnvil.SetUpgradeIdentifier(chosenUpgrade);
        theAnvil.StartUpgradeProcess();
        UpdateUpgradeButton();
        UpdateSelectMinionBtn();
        UpdateSelectUpgradeBtn();
    }
    private void UpdateUpgradeButton() {
        upgradeProgress.gameObject.SetActive(false);
        upgradeBtn.interactable = chosenMinion != null && !string.IsNullOrEmpty(chosenUpgrade)
            && string.IsNullOrEmpty(theAnvil.upgradeIdentifier);
        if (!upgradeBtn.interactable) {
            if (!string.IsNullOrEmpty(theAnvil.upgradeIdentifier)) {
                upgradeProgress.gameObject.SetActive(true);
                upgradeProgress.fillAmount = 0;
            }
        }
    }
    public void UpdatePlayerUpgradeUI() {
        if (!string.IsNullOrEmpty(theAnvil.upgradeIdentifier) && upgradeProgress.gameObject.activeSelf) {
            upgradeProgress.fillAmount = theAnvil.currentUpgradeTick / (float)theAnvil.upgradeDuration;
        }
    }
    public void OnUpgradeDone() {
        //reset
        upgradeBtn.interactable = false;
        upgradeProgress.fillAmount = 0;
        minionName.gameObject.SetActive(false);
        minionPortrait.gameObject.SetActive(false);
        upgradeText.gameObject.SetActive(false);
        selectMinionBtn.interactable = true;
        selectUpgradeBtn.interactable = false;
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select minion to do upgrade";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.UPGRADE);
    }
    private void SetChosenMinion(Character character) {
        chosenMinion = character.minion;
        minionPortrait.GeneratePortrait(chosenMinion.character);
        minionName.text = chosenMinion.character.name;
        minionPortrait.gameObject.SetActive(true);
        minionName.gameObject.SetActive(true);
        UpdateUpgradeButton();
        UpdateSelectUpgradeBtn();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = string.IsNullOrEmpty(theAnvil.upgradeIdentifier);
    }
    #endregion

    #region Upgrade
    public void OnClickSelectAbility() {
        List<string> choices = new List<string>() {
           TheAnvil.All_Intervention,
           TheAnvil.All_Summon,
           TheAnvil.All_Artifact,
        };
       
        UIManager.Instance.ShowClickableObjectPicker(choices, SetChosenUpgrade, null, CanChooseUpgrade, "Select upgrade", OnHoverAbilityChoice, OnHoverExitAbilityChoice, "intervention ability");
    }
    private bool CanChooseUpgrade(string upgrade) {
        //check if any of the categories are already at max level.
        if (upgrade == TheAnvil.All_Intervention) {
            return !PlayerManager.Instance.player.AreAllInterventionSlotsMaxLevel();
        } else if (upgrade == TheAnvil.All_Summon) {
            return !PlayerManager.Instance.player.AreAllSummonSlotsMaxLevel();
        } else if (upgrade == TheAnvil.All_Artifact) {
            return !PlayerManager.Instance.player.AreAllArtifactSlotsMaxLevel();
        }
        return true;
    }
    private void OnHoverAbilityChoice(string abilityName) {
        string info = TheAnvil.GetUpgradeDescription(abilityName);
        info += "\nUpgrade Duration: " + GameManager.Instance.GetCeilingHoursBasedOnTicks(TheAnvil.GetUpgradeDuration(abilityName)) + " hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitAbilityChoice(string abilityName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void SetChosenUpgrade(string abilityName) {
        chosenUpgrade = abilityName;
        upgradeText.text = chosenUpgrade;
        upgradeText.gameObject.SetActive(!string.IsNullOrEmpty(chosenUpgrade));
        UpdateUpgradeButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectUpgradeBtn() {
        selectUpgradeBtn.interactable = string.IsNullOrEmpty(theAnvil.upgradeIdentifier) && chosenMinion != null;
    }
    #endregion

}
