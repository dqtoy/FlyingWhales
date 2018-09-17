using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    public Text blueMagicText;
    public Text redMagicText;
    public Text greenMagicText;
    public Text lifestoneText;

    public Image threatFiller;


    void Awake() {
        Instance = this;
    }

    public void UpdateUI() {
        blueMagicText.text = "" + PlayerManager.Instance.player.blueMagic;
        redMagicText.text = "" + PlayerManager.Instance.player.redMagic;
        greenMagicText.text = "" + PlayerManager.Instance.player.greenMagic;
        lifestoneText.text = "" + PlayerManager.Instance.player.lifestones;
        threatFiller.fillAmount = (float) PlayerManager.Instance.player.threatLevel / 100f;
    }

    #region PlayerPicker
    public void ShowPlayerPickerAndPopulate() {
        if (PlayerManager.Instance.player.currentActiveAbility is ShareIntel) {
            UIManager.Instance.PopulatePlayerIntelsInPicker();
        } else if (PlayerManager.Instance.player.currentActiveAbility is GiveItem) {
            UIManager.Instance.PopulatePlayerItemsInPicker();
        } else if (PlayerManager.Instance.player.currentActiveAbility is TakeItem) {
            UIManager.Instance.PopulateLandmarkItemsInPicker();
        }
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerIntel() {
        PlayerManager.Instance.player.OnHidePlayerPicker();
        UIManager.Instance.PopulatePlayerIntelsInPicker();
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerInventory() {
        PlayerManager.Instance.player.OnHidePlayerPicker();
        UIManager.Instance.PopulatePlayerItemsInPicker();
        UIManager.Instance.ShowPlayerPicker();
    }
    #endregion

}
