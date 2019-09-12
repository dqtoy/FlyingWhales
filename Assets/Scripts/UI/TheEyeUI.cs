using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheEyeUI : MonoBehaviour {

    [Header("General")]
    public Button assignBtn;
    public TextMeshProUGUI assignBtnLbl;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    public Minion chosenMinion { get; private set; }

    private TheEye theEye;

    #region General
    public void ShowTheEyeUI(TheEye theEye) {
        this.theEye = theEye;
        if (theEye.tileLocation.region.assignedMinion == null) {
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
            UpdateAssignButton();
        } else {
            SetChosenMinion(theEye.tileLocation.region.assignedMinion.character);
            UpdateSelectMinionBtn();
            UpdateAssignButton();
        }

        gameObject.SetActive(true);
    }
    public void HideTheEyeUI() {
        gameObject.SetActive(false);
    }
    public void OnClickAssign() {
        if (theEye.tileLocation.region.assignedMinion == null) {
            theEye.tileLocation.region.SetAssignedMinion(chosenMinion);
            chosenMinion.SetAssignedRegion(theEye.tileLocation.region);
            UpdateAssignButton();
            UpdateSelectMinionBtn();
        } else {
            theEye.tileLocation.region.assignedMinion.SetAssignedRegion(null);
            theEye.tileLocation.region.SetAssignedMinion(null);
            minionPortrait.gameObject.SetActive(false);
            minionName.gameObject.SetActive(false);
            UpdateAssignButton();
            UpdateSelectMinionBtn();
        }
    }
    private void UpdateAssignButton() {
        if (theEye.tileLocation.region.assignedMinion == null) {
            assignBtnLbl.text = "Assign";
            assignBtn.interactable = chosenMinion != null;
        } else {
            assignBtnLbl.text = "Unassign";
            assignBtn.interactable = true;
        }
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select minion that will scout";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INTERFERE);
    }
    private void SetChosenMinion(Character character) {
        chosenMinion = character.minion;
        minionPortrait.GeneratePortrait(chosenMinion.character);
        minionName.text = chosenMinion.character.name;
        minionPortrait.gameObject.SetActive(true);
        minionName.gameObject.SetActive(true);
        UpdateAssignButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = theEye.tileLocation.region.assignedMinion == null;
    }
    #endregion
}
