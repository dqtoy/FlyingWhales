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
        //Show Character Info
        UIManager.Instance.ShowCharacterInfo(_icon.character);
    }
    #endregion



}
