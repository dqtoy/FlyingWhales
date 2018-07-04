using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

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
            CharacterAction action = icon.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
            AttackAction attackAction = action as AttackAction;
            if (attackAction.CanBeDoneByTesting(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party)) { //TODO: Change this checker to relationship status checking instead of just faction
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(attackAction);
                UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
                return;
            }
        }else if (UIManager.Instance.characterInfoUI.isWaitingForJoinBattleTarget) {
            CharacterAction joinBattleAction = icon.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_BATTLE);
            if (joinBattleAction.CanBeDone() && joinBattleAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party)) { //TODO: Change this checker to relationship status checking instead of just faction
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(joinBattleAction);
                UIManager.Instance.characterInfoUI.SetJoinBattleButtonState(false);
                return;
            }
        }
        if (icon.iparty is CharacterParty) {
            UIManager.Instance.ShowCharacterInfo(icon.iparty.icharacters[0] as ECS.Character);
        }
        
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (icon.iparty is CharacterParty) {
            CharacterParty thisParty = icon.iparty as CharacterParty;
            if (thisParty.actionData.currentAction != null) {
                if (other.tag == "Character" && thisParty.actionData.currentAction.actionType == ACTION_TYPE.ATTACK) {
                    AttackAction attackAction = thisParty.actionData.currentAction as AttackAction;
                    CharacterIcon enemy = other.GetComponent<CharacterClick>().icon;
                    if (attackAction.icharacterObj.iparty.id == enemy.iparty.id) {//attackAction.icharacterObj.iparty == enemy.iparty.icharacterType && 
                        thisParty.actionData.DoAction();
                    }
                }
            }
        }

    }
}
