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

    public ThePortal portal { get; private set; }
    
    #region General
    public void ShowPlayerSummonMinionUI(ThePortal portal) {
        this.portal = portal;
        UpdatePlayerSummonMinionUI();
        gameObject.SetActive(true);
    }
    public void HidePlayerSummonMinionUI() {
        gameObject.SetActive(false);
    }
    public void OnClickSummon() {
        //show dual object picker, and allow only 1 object to be picked
        //column 1 should contain all minions to be summoned and column 2 should contain the players minions
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(PlayerManager.Instance.player.minionsToSummon.ToList(), PlayerManager.Instance.player.minions.Select(x => x.character).ToList(),
            "Choose Minion to Summon", "Choose Minion to Help (Optional)",
            null, CanChooseMinion,
            OnHoverMinionToSummonChoice, null,
            OnHoverExitMinionToSummonChoice, null,
            ConfirmSummon, "Summon", false
        );
    }
    private void ConfirmSummon(object summonObj, object minionObj) {
        UnsummonedMinionData data = (UnsummonedMinionData)summonObj;
        Minion minion = (minionObj as Character)?.minion ?? null;
        Debug.Log("Will summon " + data.className + " helped by " + (minion?.character.name ?? "No one"));

        int summonDuration = LandmarkManager.SUMMON_MINION_DURATION;
        if (minion != null) {
            int speedUpDuration = Mathf.CeilToInt(LandmarkManager.SUMMON_MINION_DURATION * 0.25f);
            summonDuration -= speedUpDuration;
        }

        portal.StartSummon(System.Array.IndexOf(PlayerManager.Instance.player.minionsToSummon, data), 0, summonDuration);

        UpdatePlayerSummonMinionUI();
    }
    public void UpdatePlayerSummonMinionUI() {
        if (portal.currentMinionToSummonIndex != -1) {
            summonProgress.gameObject.SetActive(true);
            summonBtn.interactable = false;
            summonProgress.fillAmount = portal.currentSummonTick / (float)portal.currentSummonDuration;
        } else {
            summonProgress.gameObject.SetActive(false);
            summonBtn.interactable = true;
        }
    }
    #endregion

    #region Minion
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    #endregion

    #region Summon
    public void OnHoverMinionToSummonChoice(UnsummonedMinionData minionClassName) {
        string info = "Duration: " + GameManager.Instance.GetCeilingHoursBasedOnTicks(LandmarkManager.SUMMON_MINION_DURATION) + " hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    public void OnHoverExitMinionToSummonChoice(UnsummonedMinionData minionClassName) {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

}
