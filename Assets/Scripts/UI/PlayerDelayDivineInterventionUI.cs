using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDelayDivineInterventionUI : MonoBehaviour {
    [Header("General")]
    public Button delayBtn;
    public Image delayProgress;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    public TheProfane profane { get; private set; }
    public Minion chosenMinion { get; private set; }

    #region General
    public void ShowPlayerDelayDivineInterventionUI(TheProfane profane) {
        this.profane = profane;

        if (!profane.hasActivatedDelayDivineIntervention) {
            chosenMinion = null;
            delayBtn.interactable = false;
            delayProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
        } else {
            SetChosenMinion(profane.tileLocation.region.assignedMinion.character);
            UpdateSelectMinionBtn();
            UpdatePlayerDelayDivineInterventionUI();
        }

        gameObject.SetActive(true);
    }
    public void HidePlayerDelayDivineInterventionUI() {
        gameObject.SetActive(false);
    }
    public void OnClickDelay() {
        profane.tileLocation.region.SetAssignedMinion(chosenMinion);
        chosenMinion.SetAssignedRegion(profane.tileLocation.region);
        profane.StartDelay(0);
        //currentTile.region.StartBuildingStructure(chosenLandmark, chosenMinion);
        UpdateDelayButton();
        UpdateSelectMinionBtn();
    }
    private void UpdateDelayButton() {
        delayProgress.gameObject.SetActive(false);
        delayBtn.interactable = chosenMinion != null && !profane.hasActivatedDelayDivineIntervention;
        if (!delayBtn.interactable) {
            if (profane.hasActivatedDelayDivineIntervention) {
                delayProgress.gameObject.SetActive(true);
                delayProgress.fillAmount = 0;
            }
        }
    }
    public void UpdatePlayerDelayDivineInterventionUI() {
        if (profane.hasActivatedDelayDivineIntervention && delayProgress.gameObject.activeSelf) {
            delayProgress.fillAmount = profane.currentDelayTick / (float) LandmarkManager.DELAY_DIVINE_INTERVENTION_DURATION;
        }
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion to Delay Divine Intervention";
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
        UpdateDelayButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = !profane.hasActivatedDelayDivineIntervention;
    }
    #endregion
}
