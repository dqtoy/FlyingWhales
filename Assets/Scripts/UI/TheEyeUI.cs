using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheEyeUI : MonoBehaviour {

    [Header("General")]
    public Button interfereBtn;
    public Image cooldownProgress;

    private TheEye theEye;

    #region General
    public void ShowTheEyeUI(TheEye theEye) {
        this.theEye = theEye;
        UpdateTheEyeUI();
        gameObject.SetActive(true);
    }
    public void HideTheEyeUI() {
        gameObject.SetActive(false);
    }
    public void UpdateTheEyeUI() {
        if (theEye.isInCooldown) {
            interfereBtn.interactable = false;
            cooldownProgress.gameObject.SetActive(true);
            cooldownProgress.fillAmount = theEye.currentCooldownTick / (float)theEye.cooldownDuration;
        } else {
            interfereBtn.interactable = true;
            cooldownProgress.gameObject.SetActive(false);
        }
    }
    private Minion minionToInterfere; 
    public void OnClickInterfere() {
        minionToInterfere = null;
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), "Choose Minion",
            CanChooseMinion, OnHoverEnterMinion, OnHoverExitMinion, OnPickFirstObject, ConfirmInterfere, "Interfere");
    }
    private void OnPickFirstObject(object obj) {
        List<Region> activeEventRegions = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.activeEvent != null) {
                activeEventRegions.Add(currRegion);
            }
        }
        minionToInterfere = (obj as Character).minion;
        UIManager.Instance.dualObjectPicker.PopulateColumn(activeEventRegions, CanInterfere, null, null, UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Ability");
    }
    private bool CanInterfere(Region region) {
        return region.activeEvent.CanBeInterferedBy(minionToInterfere);
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = character.name + " is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SABOTEUR) && !character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.FIGHTER)) {
                message = character.name + " does not have the required trait: Saboteur or Fighter";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void ConfirmInterfere(object minionObj, object regionObj) {
        Character character = minionObj as Character;
        Region targetRegion = regionObj as Region;

        (UIManager.Instance.regionInfoUI.activeRegion.mainLandmark as TheEye).StartInterference(targetRegion, character); //NOTE: This assumes that the Region Info UI is showing when the event item is clicked.
        UpdateTheEyeUI();
    }
    #endregion

    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && (character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SABOTEUR) || character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.FIGHTER));
    }

}