using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSummonMinionUI : MonoBehaviour {
    [Header("General")]
    public Button summonBtn;
    public Image summonProgress;

    [Header("Minion To Assign")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionToAssignBtn;

    [Header("Minion To Summon")]
    public TextMeshProUGUI minionToSummonText;
    public Image minionToSummonImg;
    public Button selectMinionToSummonBtn;

    public ThePortal portal { get; private set; }
    public Minion chosenMinion { get; private set; }
    public string chosenMinionClassToSummon { get; private set; }
    public int summonDuration { get; private set; }

    #region General
    public void ShowPlayerSummonMinionUI(ThePortal portal) {
        this.portal = portal;

        if (portal.currentMinionClassToSummon == string.Empty) {
            chosenMinion = null;
            chosenMinionClassToSummon = string.Empty;
            summonBtn.interactable = false;
            summonProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            minionToSummonText.gameObject.SetActive(false);
            minionToSummonImg.gameObject.SetActive(false);
            selectMinionToAssignBtn.interactable = true;
            selectMinionToSummonBtn.interactable = true;
            UpdateSummonDuration();
        } else {
            if(portal.tileLocation.region.assignedMinion != null) {
                SetChosenMinion(portal.tileLocation.region.assignedMinion.character);
            }
            SetChosenMinionToSummon(portal.currentMinionClassToSummon);
            UpdateSelectMinionToAssignBtn();
            UpdateSelectMinionToSummonBtn();
            UpdatePlayerSummonMinionUI();
        }

        gameObject.SetActive(true);
    }
    public void HidePlayerSummonMinionUI() {
        gameObject.SetActive(false);
    }
    public void OnClickSummon() {
        if(chosenMinion != null) {
            portal.tileLocation.region.SetAssignedMinion(chosenMinion);
            chosenMinion.SetAssignedRegion(portal.tileLocation.region);
        }
        portal.StartSummon(chosenMinionClassToSummon, 0, summonDuration);
        //currentTile.region.StartBuildingStructure(chosenLandmark, chosenMinion);
        UpdateSummonButton();
        UpdateSelectMinionToAssignBtn();
        UpdateSelectMinionToSummonBtn();
    }
    private void UpdateSummonButton() {
        summonProgress.gameObject.SetActive(false);
        summonBtn.interactable = chosenMinionClassToSummon != string.Empty && portal.currentMinionClassToSummon == string.Empty;
        if (!summonBtn.interactable) {
            if (portal.currentMinionClassToSummon != string.Empty) {
                summonProgress.gameObject.SetActive(true);
                summonProgress.fillAmount = 0;
            }
        }
    }
    public void UpdatePlayerSummonMinionUI() {
        if (portal.currentMinionClassToSummon != string.Empty && summonProgress.gameObject.activeSelf) {
            summonProgress.fillAmount = portal.currentSummonTick / (float) portal.currentSummonDuration;
        }
    }
    private void UpdateSummonDuration() {
        summonDuration = LandmarkManager.SUMMON_MINION_DURATION;
        if(chosenMinion != null) {
            int speedUpDuration = Mathf.CeilToInt(LandmarkManager.SUMMON_MINION_DURATION * 0.25f);
            summonDuration -= speedUpDuration;
        }
    }
    #endregion

    #region Minion To Assign
    public void OnClickSelectMinionToAssign() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion to Speed Up Summoning";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    private void SetChosenMinion(Character character) {
        chosenMinion = character.minion;
        minionPortrait.GeneratePortrait(chosenMinion.character);
        minionName.text = chosenMinion.character.name;
        minionPortrait.gameObject.SetActive(true);
        minionName.gameObject.SetActive(true);
        UpdateSummonDuration();
        UpdateSummonButton();
        UpdateSelectMinionToSummonBtn();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionToAssignBtn() {
        selectMinionToAssignBtn.interactable = portal.currentMinionClassToSummon == string.Empty;
    }
    #endregion

    #region Minion To Summon
    public void OnClickSelectMinionToSummon() {
        string title = "Select Minion to Summon";
        UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.minionClassesToSummon.ToList(), SetChosenMinionToSummon, null, CanChooseMinionToSummon, title, OnHoverMinionToSummonChoice, OnHoverExitMinionToSummonChoice, "minion");
    }
    private bool CanChooseMinionToSummon(string minionClassName) {
        return true;
    }
    private void OnHoverMinionToSummonChoice(string minionClassName) {
        string info = "Duration: " + GameManager.Instance.GetCeilingHoursBasedOnTicks(summonDuration) + " hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitMinionToSummonChoice(string minionClassName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void SetChosenMinionToSummon(string minionClassName) {
        chosenMinionClassToSummon = minionClassName;
        minionToSummonImg.sprite = CharacterManager.Instance.GetClassPortraitSprite(minionClassName);
        minionToSummonText.text = minionClassName;
        minionToSummonImg.gameObject.SetActive(true);
        minionToSummonText.gameObject.SetActive(true);
        UpdateSummonButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionToSummonBtn() {
        selectMinionToSummonBtn.interactable = portal.currentMinionClassToSummon == string.Empty;
    }
    #endregion
}
