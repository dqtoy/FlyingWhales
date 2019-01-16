using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSlotItem : MonoBehaviour {
    public CharacterPortrait portrait;
    public GameObject glowGO;
    public GameObject targetSelectGO;
    public SIDES side;

    public Character character { get; private set; }
    public string hoverInfo { get; private set; }
    public int gridNumber { get; private set; }

    public bool isTargetable {
        get { return targetSelectGO.activeSelf; }
    }

    public void SetCharacter(Character character) {
        this.character = character;
        if(this.character != null) {
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait(character);
            hoverInfo = this.character.name;
        } else {
            portrait.gameObject.SetActive(false);
        }
    }
    public void SetGridNumber(int number) {
        gridNumber = number;
    }
    public void ShowCharacterInfo() {
        if (UIManager.Instance.combatUI.CanSlotBeTarget(this)) {
            UIManager.Instance.combatUI.ShowTargetCharacters(this);
        } else {
            if (character != null) {
                UIManager.Instance.ShowSmallInfo(hoverInfo);
            }
        }
    }
    public void HideCharacterInfo() {
        if (character != null) {
            UIManager.Instance.HideSmallInfo();
        }
        UIManager.Instance.combatUI.HideTargetCharacters(this);
    }
    public void SetHighlight(bool state) {
        glowGO.SetActive(state);
    }
    public void SetTargetable(bool state) {
        targetSelectGO.SetActive(state);
    }
    public void OnClickCombatSlot() {
        UIManager.Instance.combatUI.SelectTargetCharacters(this);
    }
}
