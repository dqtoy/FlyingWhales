using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterClick : MonoBehaviour {
    public CharacterIcon icon;

    //private void OnMouseOver() {
    //    if (Input.GetMouseButton(0)) {
    //        MouseDown();
    //    }
    //}
    private void OnMouseDown() {
        MouseDown();
    }
    private void MouseDown() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        if (UIManager.Instance.characterInfoUI.isWaitingForAttackTarget) {
            if(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.faction.id != icon.character.faction.id) { //TODO: Change this checker to relationship status checking instead of just faction
                CharacterAction attackAction = icon.character.characterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(attackAction);
                UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
                return;
            }
        }
        UIManager.Instance.ShowCharacterInfo(icon.character);
    }
}
