using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackSlotItem : MonoBehaviour {
    public Image attackImg;
    public Button attackBtn;

    public void ShowAttackButton() {
        if (!PlayerManager.Instance.player.attackGrid.IsGridEmpty()) {
            attackBtn.gameObject.SetActive(true);
        }
    }
    public void HideAttackButton() {
        attackBtn.gameObject.SetActive(false);
    }
    public void OnClickAssign() {
        UIManager.Instance.ShowDraggableObjectPicker(PlayerManager.Instance.player.allOwnedCharacters, new CharacterLevelComparer(), CanAssignCharacterToAttack);
        PlayerUI.Instance.ShowAttackGrid();
    }
    public void OnClickConfirm() {
        CombatGrid savedCombatGrid = new CombatGrid();
        savedCombatGrid.Initialize();
        for (int i = 0; i < savedCombatGrid.slots.Length; i++) {
            savedCombatGrid.slots[i].OccupySlot(PlayerUI.Instance.attackGridReference.slots[i].character);
        }
        //PlayerManager.Instance.player.AssignAttackGrid(savedCombatGrid);
        UIManager.Instance.HideObjectPicker();
        PlayerUI.Instance.HideCombatGrid();
        UpdateVisuals();
    }
    private bool CanAssignCharacterToAttack(Character character) {
        //return PlayerManager.Instance.player.CanAssignCharacterToAttack(character);
        return false;
    }
    public void UpdateVisuals() {
        attackImg.gameObject.SetActive(PlayerManager.Instance.player.attackGrid != null && !PlayerManager.Instance.player.attackGrid.IsGridEmpty());
    }
}
