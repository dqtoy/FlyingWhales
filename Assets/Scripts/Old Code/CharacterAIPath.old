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
        if (_icon.targetLocation is BaseLandmark) {
            _icon.targetLocation.AddCharacterToLocation(_icon.iparty);
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

    //#region Monobehaviours
    //private void OnMouseDown() {
    //    if (UIManager.Instance.IsMouseOnUI()) {
    //        return;
    //    }
    //    if (UIManager.Instance.characterInfoUI.isWaitingForAttackTarget) {
    //        if (icon.icharacter.icharacterType == ICHARACTER_TYPE.MONSTER || UIManager.Instance.characterInfoUI.currentlyShowingCharacter.faction.id != icon.icharacter.faction.id) { //TODO: Change this checker to relationship status checking instead of just faction
    //            CharacterAction attackAction = icon.icharacter.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
    //            UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(attackAction);
    //            return;
    //        }
    //    }
    //    if (icon.icharacter.icharacterType == ICHARACTER_TYPE.CHARACTER) {
    //        UIManager.Instance.ShowCharacterInfo(icon.icharacter as Character);
    //    }
    //}
    //#endregion

    #region Context Menus
    [ContextMenu("Log Remaining Distance")]
    public void LogRemainingDistance() {
        Debug.Log("Remaining distance of " + _icon.name + " to " + _icon.targetLocation.locationName + " is " + remainingDistance);
    }
    #endregion
}
