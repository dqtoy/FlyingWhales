using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour {
    public CombatSlotItem[] leftSlots;
    public CombatSlotItem[] rightSlots;

    public TextMeshProUGUI resultsText;
    public GameObject combatGO;

    // Use this for initialization
    void Start () {
        Initialize();
	}
	
	private void Initialize() {
        for (int i = 0; i < leftSlots.Length; i++) {
            leftSlots[i].SetGridNumber(i);
        }
        for (int i = 0; i < rightSlots.Length; i++) {
            rightSlots[i].SetGridNumber(i);
        }
        Messenger.AddListener<string>(Signals.ADD_TO_COMBAT_LOGS, AddCombatLogs);
        Messenger.AddListener<Character, SIDES>(Signals.HIGHLIGHT_ATTACKER, HighlightAttacker);
        Messenger.AddListener<Character, SIDES>(Signals.UNHIGHLIGHT_ATTACKER, UnhighlightAttacker);
        Messenger.AddListener(Signals.UPDATE_COMBAT_GRIDS, UpdateCombatSlotItems);
    }
    public void OpenCombatUI(bool triggerCombatFight) {
        GameManager.Instance.SetPausedState(true);
        combatGO.SetActive(true);
        if (triggerCombatFight) {
            resultsText.text = string.Empty;
            FillLeftSlots();
            FillRightSlots();
            CombatManager.Instance.newCombat.Fight();
        }
    }
    public void CloseCombatUI() {
        GameManager.Instance.SetPausedState(false);
        combatGO.SetActive(false);
    }
    public void FillLeftSlots() {
        for (int i = 0; i < CombatManager.Instance.newCombat.leftSide.slots.Length; i++) {
            leftSlots[i].SetCharacter(CombatManager.Instance.newCombat.leftSide.slots[i].character);
        }
    }
    public void FillRightSlots() {
        for (int i = 0; i < CombatManager.Instance.newCombat.rightSide.slots.Length; i++) {
            rightSlots[i].SetCharacter(CombatManager.Instance.newCombat.rightSide.slots[i].character);
        }
    }

    public void AddCombatLogs(string text) {
        if (!combatGO.activeSelf) {
            return;
        }
        resultsText.text += "\n" + text;
    }
    public void HighlightAttacker(Character character, SIDES side) {
        if (!combatGO.activeSelf) {
            return;
        }
        if(side == SIDES.A) {
            for (int i = 0; i < leftSlots.Length; i++) {
                if(leftSlots[i].character != null && leftSlots[i].character.id == character.id) {
                    leftSlots[i].SetHighlight(true);
                }
            }
        } else {
            for (int i = 0; i < rightSlots.Length; i++) {
                if (rightSlots[i].character != null && rightSlots[i].character.id == character.id) {
                    rightSlots[i].SetHighlight(true);
                }
            }
        }
    }
    public void UnhighlightAttacker(Character character, SIDES side) {
        if (!combatGO.activeSelf) {
            return;
        }
        if (side == SIDES.A) {
            for (int i = 0; i < leftSlots.Length; i++) {
                if (leftSlots[i].character != null && leftSlots[i].character.id == character.id) {
                    leftSlots[i].SetHighlight(false);
                }
            }
        } else {
            for (int i = 0; i < rightSlots.Length; i++) {
                if (rightSlots[i].character != null && rightSlots[i].character.id == character.id) {
                    rightSlots[i].SetHighlight(false);
                }
            }
        }
    }
    public void UpdateCombatSlotItems() {
        if (!combatGO.activeSelf) {
            return;
        }
        FillLeftSlots();
        FillRightSlots();
    }
}
