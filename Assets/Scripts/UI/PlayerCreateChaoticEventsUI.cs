using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCreateChaoticEventsUI : MonoBehaviour {
    [Header("General")]
    public Button createBtn;
    public Image createProgress;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    public TheFingers fingers { get; private set; }
    public Minion chosenMinion { get; private set; }

    #region General
    public void ShowPlayerCreateChaoticEventsUI(TheFingers fingers) {
        this.fingers = fingers;

        if (!fingers.hasActivatedCreateChaoticEvents) {
            chosenMinion = null;
            createBtn.interactable = false;
            createProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
        } else {
            SetChosenMinion(fingers.tileLocation.region.assignedMinion.character);
            UpdateSelectMinionBtn();
            UpdatePlayerCreateChaoticEventsUI();
        }

        gameObject.SetActive(true);
    }
    public void HidePlayerCreateChaoticEventsUI() {
        gameObject.SetActive(false);
    }
    public void OnClickCreate() {
        fingers.tileLocation.region.SetAssignedMinion(chosenMinion);
        chosenMinion.SetAssignedRegion(fingers.tileLocation.region);
        //fingers.StartDelay(0);
        UpdateCreateButton();
        UpdateSelectMinionBtn();
    }
    private void UpdateCreateButton() {
        createProgress.gameObject.SetActive(false);
        createBtn.interactable = chosenMinion != null && !fingers.hasActivatedCreateChaoticEvents;
        if (!createBtn.interactable) {
            if (fingers.hasActivatedCreateChaoticEvents) {
                createProgress.gameObject.SetActive(true);
                createProgress.fillAmount = 0;
            }
        }
    }
    public void UpdatePlayerCreateChaoticEventsUI() {
        if (fingers.hasActivatedCreateChaoticEvents && createProgress.gameObject.activeSelf) {
            createProgress.fillAmount = fingers.currentCreateTick / (float) fingers.createDuration;
        }
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion to Create Chaotic Events";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    private void SetChosenMinion(object c) {
        Character character = c as Character;
        chosenMinion = character.minion;
        minionPortrait.GeneratePortrait(chosenMinion.character);
        minionName.text = chosenMinion.character.name;
        minionPortrait.gameObject.SetActive(true);
        minionName.gameObject.SetActive(true);
        UpdateCreateButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = !fingers.hasActivatedCreateChaoticEvents;
    }
    #endregion
}
