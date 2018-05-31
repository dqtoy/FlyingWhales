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
    private bool _shouldRecalculatePath = false;

    #region getters/setters
    public CharacterIcon icon {
        get { return _icon; }
    }
    protected override bool shouldRecalculatePath {
        get { return _shouldRecalculatePath; }
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
            _icon.targetLocation.AddCharacterToLocation(_icon.character);
        }
        _icon.destinationSetter.target = null;
        //SetRecalculatePathState(false);
    }
    #endregion

    public void SetRecalculatePathState(bool state) {
        _shouldRecalculatePath = state;
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
            if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.faction.id != icon.character.faction.id) { //TODO: Change this checker to relationship status checking instead of just faction
                CharacterAction attackAction = icon.character.characterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(attackAction);
                return;
            }
        }
        UIManager.Instance.ShowCharacterInfo(icon.character);
    }
    #endregion



}
