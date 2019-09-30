using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheNeedlesUI : MonoBehaviour {

    [Header("General")]
    public Button convertBtn;
    public Image cooldownProgress;
    public TextMeshProUGUI convertBtnLbl;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    public TheNeedles needles { get; private set; }
    public Minion chosenMinion { get; private set; }

    #region General
    public void ShowTheNeedlesUI(TheNeedles needles) {
        this.needles = needles;

        if (!needles.isInCooldown) {
            chosenMinion = null;
            convertBtn.interactable = false;
            cooldownProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
        } else {
            chosenMinion = null;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            UpdateSelectMinionBtn();
            UpdateUI();
        }

        gameObject.SetActive(true);
    }
    public void HideTheNeedlesUI() {
        gameObject.SetActive(false);
    }
    public void OnClickConvert() {
        needles.tileLocation.region.SetAssignedMinion(chosenMinion);
        chosenMinion.SetAssignedRegion(needles.tileLocation.region);
        needles.Activate();
        UpdateConvertButton();
        UpdateSelectMinionBtn();
    }
    private void UpdateConvertButton() {
        cooldownProgress.gameObject.SetActive(false);
        convertBtn.interactable = chosenMinion != null && PlayerManager.Instance.player.mana >= needles.GetManaValue(chosenMinion) && !needles.isInCooldown;
        if (chosenMinion != null) {
            convertBtnLbl.text = "Convert to " + needles.GetManaValue(chosenMinion).ToString() + " Mana";
        } else {
            convertBtnLbl.text = "Convert";
        }

        if (!convertBtn.interactable) {
            cooldownProgress.gameObject.SetActive(true);
            cooldownProgress.fillAmount = 0;
        }
    }
    public void UpdateUI() {
        if (cooldownProgress.gameObject.activeSelf && needles.isInCooldown) {
            cooldownProgress.fillAmount = needles.currentCooldownTick / (float)needles.cooldownDuration;
        } else {
            UpdateConvertButton();
        }
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion to Convert";
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
        UpdateConvertButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = !needles.isInCooldown;
    }
    #endregion

}
