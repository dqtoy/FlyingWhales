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
    public void OnClickInterfere() {
        List<Region> activeEventRegions = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.activeEvent != null) {
                activeEventRegions.Add(currRegion);
            }
        }
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character, Region>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), activeEventRegions,
            "Choose Minion", "Choose Event to Interfere with",
            CanChooseMinion, null,
            null, null,
            null, null,
            ConfirmInterfere, "Interfere", column2Identifier: "WorldEvent");
    }
    private void ConfirmInterfere(object minionObj, object regionObj) {
        Character character = minionObj as Character;
        Region targetRegion = regionObj as Region;

        (UIManager.Instance.regionInfoUI.activeRegion.mainLandmark as TheEye).StartInterference(targetRegion, character); //NOTE: This assumes that the Region Info UI is showing when the event item is clicked.
        UpdateTheEyeUI();
    }
    #endregion

    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SABOTEUR);
    }

}