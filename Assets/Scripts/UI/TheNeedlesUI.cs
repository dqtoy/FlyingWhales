using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheNeedlesUI : MonoBehaviour {

    [Header("General")]
    public Button convertBtn;
    public Image cooldownProgress;

    public TheNeedles needles { get; private set; }

    #region General
    public void ShowTheNeedlesUI(TheNeedles needles) {
        this.needles = needles;
        UpdateUI();
        gameObject.SetActive(true);
    }
    public void HideTheNeedlesUI() {
        gameObject.SetActive(false);
    }
    public void OnClickConvert() {
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), "Choose Minion to Convert to Mana", CanChooseMinion, null, null, null, OnConfirmConvert, "Convert", false);
    }
    private void OnConfirmConvert(object minionObj, object obj) {
        Minion minion = (minionObj as Character).minion;
        needles.tileLocation.region.SetAssignedMinion(minion);
        minion.SetAssignedRegion(needles.tileLocation.region);
        needles.Activate();
    }
    //private void UpdateConvertButton() {
    //    cooldownProgress.gameObject.SetActive(false);
    //    convertBtn.interactable = chosenMinion != null && PlayerManager.Instance.player.mana >= needles.GetManaValue(chosenMinion) && !needles.isInCooldown;
    //    if (chosenMinion != null) {
    //        convertBtnLbl.text = "Convert to " + needles.GetManaValue(chosenMinion).ToString() + " Mana";
    //    } else {
    //        convertBtnLbl.text = "Convert";
    //    }

    //    if (!convertBtn.interactable) {
    //        cooldownProgress.gameObject.SetActive(true);
    //        cooldownProgress.fillAmount = 0;
    //    }
    //}
    public void UpdateUI() {
        if (needles.isInCooldown) {
            convertBtn.interactable = false;
            cooldownProgress.gameObject.SetActive(true);
            cooldownProgress.fillAmount = needles.currentCooldownTick / (float)needles.cooldownDuration;
        } else {
            convertBtn.interactable = true;
            cooldownProgress.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Minion
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    #endregion

}
