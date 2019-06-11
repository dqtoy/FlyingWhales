using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTestingUI : MonoBehaviour {
    //This script is used to test characters and actions
    //Most of the functions here will only work if there is a currently clicked/active character
    public RectTransform rt;
    public Character character { get; private set; }

    #region Utilities
    public void ShowUI(Character character) {
        this.character = character;
        UIManager.Instance.PositionTooltip(gameObject, rt, rt);
        gameObject.SetActive(true);
    }
    public void HideUI() {
        gameObject.SetActive(false);
        this.character = null;
    }
    #endregion

    #region Character Testing
    public void AssaultThisCharacter() {
        if(UIManager.Instance.characterInfoUI.activeCharacter != null) {
            UIManager.Instance.characterInfoUI.activeCharacter.CreateAssaultUndermineJobOnly(character, "idle");
        } else {
            Debug.LogError("No active/clicked character!");
        }
        HideUI();
    }
    #endregion
}
