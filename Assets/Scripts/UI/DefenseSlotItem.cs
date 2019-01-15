using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefenseSlotItem : MonoBehaviour {
    public Image defenseImg;

    public void OnClickAssign() {
        UIManager.Instance.ShowDraggableObjectPicker(PlayerManager.Instance.player.allOwnedCharacters, new CharacterLevelComparer(), CanAssignCharacterToDefend);
        PlayerUI.Instance.ShowDefenseGrid();
    }
    public void OnClickConfirm() {
        CombatGrid savedCombatGrid = new CombatGrid();
        savedCombatGrid.Initialize();
        for (int i = 0; i < savedCombatGrid.slots.Length; i++) {
            savedCombatGrid.slots[i].OccupySlot(PlayerUI.Instance.defenseGridReference.slots[i].character);
        }
        PlayerManager.Instance.player.AssignDefenseGrid(savedCombatGrid);
        UIManager.Instance.HideObjectPicker();
        PlayerUI.Instance.HideCombatGrid();
        UpdateVisuals();
    }
    private bool CanAssignCharacterToDefend(Character character) {
        return PlayerManager.Instance.player.CanAssignCharacterToDefend(character);
    }
    public void UpdateVisuals() {
        defenseImg.gameObject.SetActive(PlayerManager.Instance.player.defenseGrid != null && !PlayerManager.Instance.player.defenseGrid.IsGridEmpty());
    }
}
