using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnleashSummonUI : MonoBehaviour {
    [Header("Summon")]
    public Image summonIcon;
    public TextMeshProUGUI summonText;
    private Summon summon;

    public void ShowUnleashSummonUI(Summon summon) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowUnleashSummonUI(summon));
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        SetSummon(summon);
        gameObject.SetActive(true);
    }

    private void SetSummon(Summon summon) {
        this.summon = summon;
        if(this.summon != null) {
            summonIcon.sprite = CharacterManager.Instance.GetSummonSettings(summon.summonType).summonPortrait;
            string text = summon.name + " (" + summon.summonType.SummonName() + ")";
            text += "\nLevel: " + summon.level.ToString();
            text += "\nDescription: " + PlayerManager.Instance.player.GetSummonDescription(summon.summonType);
            summonText.text = text;
        }
    }

    public void OnClickConfirm() {
        Close();
        if(summon != null) {
            PlayerUI.Instance.TryPlaceSummon(summon);
        }
    }
    public void OnClickClose() {
        Close();
    }

    private void Close() {
        gameObject.SetActive(false);
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
    }
}
