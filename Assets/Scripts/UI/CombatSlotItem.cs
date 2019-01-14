using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSlotItem : MonoBehaviour {
    public CharacterPortrait portrait;
    public GameObject glowGO;

    public Character character { get; private set; }
    public string hoverInfo { get; private set; }
    public int gridNumber { get; private set; }

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
        if (character != null) {
            UIManager.Instance.ShowSmallInfo(hoverInfo);
        }
    }
    public void HideCharacterInfo() {
        if (character != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    public void SetHighlight(bool state) {
        glowGO.SetActive(state);
    }
}
