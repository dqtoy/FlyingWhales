using ECS;
using EZObjectPools;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyAIPath : AIPath {

    [SerializeField] private ArmyIcon _icon;
    private Action onTargetReachedAction;
    //private bool _shouldRecalculatePath = false;

    #region getters/setters
    public ArmyIcon icon {
        get { return _icon; }
    }
    //protected override bool shouldRecalculatePath {
    //    get { return _shouldRecalculatePath; }
    //}
    #endregion

    #region overrides
    public override void OnTargetReached() {
        base.OnTargetReached();
        if (onTargetReachedAction != null) {
            onTargetReachedAction();
            onTargetReachedAction = null;
        }
        _icon.destinationSetter.target = null;
        _icon.army.ReachedTarget();
        //SetRecalculatePathState(false);
    }
    #endregion

    //public void SetRecalculatePathState(bool state) {
    //    _shouldRecalculatePath = state;
    //}

    public void SetActionOnTargetReached(Action action) {
        onTargetReachedAction = action;
    }
}
