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
    public GameObject minionsToSummonGO;
    public MinionCard[] minionCards;

    public ThePortal portal { get; private set; }
    public Minion chosenMinion { get; private set; }
    public int chosenMinionToSummonIndex { get; private set; }
    public int summonDuration { get; private set; }

    #region General
    public void ShowPlayerSummonMinionUI(ThePortal portal) {
        this.portal = portal;

        if (portal.currentMinionToSummonIndex == -1) {
            chosenMinion = null;
            chosenMinionToSummonIndex = -1;
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
            SetChosenMinionToSummon(portal.currentMinionToSummonIndex);
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
        portal.StartSummon(chosenMinionToSummonIndex, 0, summonDuration);
        //currentTile.region.StartBuildingStructure(chosenLandmark, chosenMinion);
        UpdateSummonButton();
        UpdateSelectMinionToAssignBtn();
        UpdateSelectMinionToSummonBtn();
    }
    private void UpdateSummonButton() {
        summonProgress.gameObject.SetActive(false);
        summonBtn.interactable = chosenMinionToSummonIndex != -1 && portal.currentMinionToSummonIndex == -1;
        if (!summonBtn.interactable) {
            if (portal.currentMinionToSummonIndex != -1) {
                summonProgress.gameObject.SetActive(true);
                summonProgress.fillAmount = 0;
            }
        }
    }
    public void UpdatePlayerSummonMinionUI() {
        if (portal.currentMinionToSummonIndex != -1 && summonProgress.gameObject.activeSelf) {
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
        HideMinionsToSummon();
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
        selectMinionToAssignBtn.interactable = portal.currentMinionToSummonIndex == -1;
    }
    #endregion

    #region Minion To Summon
    public void OnClickSelectMinionToSummon() {
        ShowMinionsToSummon();
        UIManager.Instance.HideObjectPicker();
    }
    private void ShowMinionsToSummon() {
        for (int i = 0; i < PlayerManager.Instance.player.minionsToSummon.Length; i++) {
            minionCards[i].SetMinion(PlayerManager.Instance.player.minionsToSummon[i]);
        }
        minionsToSummonGO.SetActive(true);
    }
    public void HideMinionsToSummon() {
        minionsToSummonGO.SetActive(false);
    }
    private bool CanChooseMinionToSummon(string minionClassName) {
        return true;
    }
    public void OnHoverMinionToSummonChoice(string minionClassName) {
        string info = "Duration: " + GameManager.Instance.GetCeilingHoursBasedOnTicks(summonDuration) + " hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    public void OnHoverExitMinionToSummonChoice(string minionClassName) {
        UIManager.Instance.HideSmallInfo();
    }
    public void SetChosenMinionToSummon(int index) {
        chosenMinionToSummonIndex = index;
        UnsummonedMinionData minionData = PlayerManager.Instance.player.minionsToSummon[index];
        minionToSummonImg.sprite = CharacterManager.Instance.GetClassPortraitSprite(minionData.className);
        minionToSummonText.text = minionData.className;
        minionToSummonImg.gameObject.SetActive(true);
        minionToSummonText.gameObject.SetActive(true);
        UpdateSummonButton();
        HideMinionsToSummon();
        //UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionToSummonBtn() {
        selectMinionToSummonBtn.interactable = portal.currentMinionToSummonIndex == -1;
    }
    #endregion
}
