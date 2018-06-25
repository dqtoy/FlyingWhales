using ECS;
using EZObjectPools;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterAIPath : AIPath {
    [SerializeField] private CharacterIcon _icon;
    private Action onTargetReachedAction;

    #region getters/setters
    public CharacterIcon icon {
        get { return _icon; }
    }
    #endregion

    #region overrides
    public override void OnTargetReached() {
        base.OnTargetReached();
        if (onTargetReachedAction != null) {
            onTargetReachedAction();
            onTargetReachedAction = null;
        }
        if (_icon.character is Character) {
            if (_icon.targetLocation is BaseLandmark) {
                _icon.targetLocation.AddCharacterToLocation(_icon.character as Character);
            }
        }
        _icon.SetTarget(null);
        //SetRecalculatePathState(false);
    }
    #endregion

    public void RecalculatePath() {
        SearchPath();
    }

    public void SetActionOnTargetReached(Action action) {
        onTargetReachedAction = action;
    }

    #region Monobehaviours
    private void OnMouseDown() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        if (UIManager.Instance.characterInfoUI.isWaitingForAttackTarget) {
            if (icon.character is Monster || UIManager.Instance.characterInfoUI.currentlyShowingCharacter.faction.id != (icon.character as Character).faction.id) { //TODO: Change this checker to relationship status checking instead of just faction
                CharacterAction attackAction = icon.character.characterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(attackAction);
                return;
            }
        }
        if (icon.character is Character) {
            UIManager.Instance.ShowCharacterInfo(icon.character as Character);
        }
    }
    #endregion

    #region Context Menus
    [ContextMenu("Log Remaining Distance")]
    public void LogRemainingDistance() {
        Debug.Log("Remaining distance of " + _icon.name + " to " + _icon.targetLocation.locationName + " is " + remainingDistance);
    }
    #endregion
}
